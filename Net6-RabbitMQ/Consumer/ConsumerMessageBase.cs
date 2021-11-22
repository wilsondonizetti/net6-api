using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Net6_RabbitMQ.Extensions;
using Net6_RabbitMQ.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Net6_RabbitMQ.Consumer
{
    public enum MessageProcessResult
    {
        Fail = 0,
        Success = 1,
        Retry = 2
    }

    public abstract class ConsumerMessageBase<T> : IConsumerMessage<T>
    {
        private const bool NOT_AUTOACK = false;

        private readonly ObjectPool<IModel> objectPool;
        private readonly IConfiguration configuration;
        protected readonly ILogger<ConsumerMessageBase<T>> logger;

        protected AsyncEventingBasicConsumer? Consumer { get; private set; }
        protected CancellationToken CancellationToken { get; private set; }
        protected IModel? Channel { get; set; }

        public string? ConsumerTag { get; private set; }

        public ConsumerOptions Options { get; private set; }
        public IServiceProvider ServiceProvider { get; }

        public ConsumerMessageBase(
            IServiceProvider serviceProvider,
            ObjectPool<IModel> objectPool,
            IOptions<ConsumerOptions> options,
            IConfiguration configuration,
            ILogger<ConsumerMessageBase<T>> logger)
        {
            ServiceProvider = serviceProvider;
            this.objectPool = objectPool;
            this.Options = options.Value;
            this.configuration = configuration;
            this.logger = logger;
        }

        public Task BeginReceiveMessagesAsync(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            CancellationToken.ThrowIfCancellationRequested();

            Channel = objectPool.Get();

            Consumer = new AsyncEventingBasicConsumer(Channel);
            Consumer.Received += MessageReceived;

            ConsumerTag = Channel.BasicConsume(
                queue: Options.QueueName,
                autoAck: NOT_AUTOACK,
                consumer: Consumer);

            logger.LogInformation("Consumer '{consumerTag}' receiving mensage from '{queueName}'",
                ConsumerTag, Options.QueueName);

            CancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
        public void EndReceiveMessages()
        {
            //Method beginReceiveMessages wasn't called
            if (!(Consumer?.IsRunning ?? false) || (Channel?.IsClosed ?? true))
            {
                return;
            }

            Consumer.Received -= MessageReceived;

            Channel.BasicCancel(ConsumerTag);
            logger.LogWarning("End messages Consumer '{consumerTag}'", ConsumerTag);

            Channel.Close();

            objectPool.Return(Channel);
        }

        protected async Task MessageReceived(object? obj, BasicDeliverEventArgs ea)
        {
            T? message = default;
            int retryCount = ea.BasicProperties.GetRetryCount();

            try
            {
                message = JsonSerializer.Deserialize<T>(ea.Body.Span);                

                CancellationToken.ThrowIfCancellationRequested();

                var args = new MessageReceivedArgs(ea.BasicProperties.Headers, ea.ConsumerTag,
                    ea.DeliveryTag, ea.Exchange, ea.Redelivered, ea.RoutingKey, retryCount);

                var result = await ProcessMessageAsync(message, args, CancellationToken);

                switch (result)
                {
                    case MessageProcessResult.Success:
                        AckMessage(ea.DeliveryTag);
                        break;

                    case MessageProcessResult.Retry:
                        ProcessMessageForRetry(message, ea.DeliveryTag, retryCount, ea.BasicProperties);
                        break;
                    case MessageProcessResult.Fail:
                        NackMessage(ea.DeliveryTag);
                        break;
                }
            }
            catch (TaskCanceledException)
            {
                if (message != null)
                    ProcessMessageForRetry(message, ea.DeliveryTag, retryCount, ea.BasicProperties);

                logger.LogInformation("Consumer '{consumerTag}' Canceled", ea.ConsumerTag);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError("Error to read message", ex);

                try
                {
                    if (message != null)
                        ProcessMessageForRetry(message, ea.DeliveryTag, retryCount, ea.BasicProperties);
                }
                catch (Exception ex2)
                {
                    logger.LogCritical("Error to post retry message", ex2);
                }
            }
        }

        protected abstract Task<MessageProcessResult> ProcessMessageAsync(
            T message, MessageReceivedArgs args, CancellationToken CancellationToken);

        protected virtual void AckMessage(ulong deliveryTag)
        {
            if (Channel == null)
                throw new InvalidOperationException("Channel was not initialize");

            if (deliveryTag != 0)
                Channel.BasicAck(deliveryTag, multiple: false);
        }

        protected virtual void NackMessage(ulong deliveryTag)
        {
            if (Channel == null)
                throw new InvalidOperationException("Channel was not initialize");

            if (deliveryTag != 0)
                Channel.BasicNack(deliveryTag, multiple: false, requeue: false);
        }

        private void ProcessMessageForRetry(T message, ulong deliveryTag,
            int retryCount, IBasicProperties properties)
        {
            if (retryCount < 2)
            {
                var enqueueHasSucceeded = TryToEnqueueMessageForRetry(message, retryCount, properties);

                if (enqueueHasSucceeded)
                {
                    AckMessage(deliveryTag);                    
                }
                else
                {
                    NackMessage(deliveryTag);                    
                }
            }
            else
            {
                NackMessage(deliveryTag);
            }
        }

        private bool TryToEnqueueMessageForRetry(
            T message, int currentRetryCount, IBasicProperties properties)
        {
            try
            {
                if (Channel == null)
                    throw new InvalidOperationException("Channel was not initialize");

                IBasicProperties retryProperties = Channel.CreateBasicProperties();

                properties.CloneHeadersTo(ref retryProperties);
                retryProperties.Headers[HeaderConstants.RETRY_HEADER] = ++currentRetryCount;

                var retryExchange = configuration["RabbitMQ:Consumer:RetryExchange"];
                var retryRoutingKey = configuration["RabbitMQ:Consumer:RetryRoutingKey"];

                var producer = ServiceProvider.GetRequiredService<IProducerMessage<T>>();

                producer.SendMessage(message, retryExchange, retryRoutingKey, retryProperties);

                logger.LogInformation("Message '{mesagem}' sent to retry", message);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Error to sent message to retry", ex);
                return false;
            }
        }
    }
}
