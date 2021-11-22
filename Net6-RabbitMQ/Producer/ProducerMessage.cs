using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using System.Text.Json;
using System.Threading.Tasks;

namespace Net6_RabbitMQ.Producer
{
    public class ProducerMessage<T> : IProducerMessage<T>
    {
        private readonly ObjectPool<IModel> channelPool;
        private readonly ILogger<ProducerMessage<T>> logger;

        public ProducerMessage(
            ILogger<ProducerMessage<T>> logger,
            ObjectPool<IModel> channelPool)
        {
            this.channelPool = channelPool;
            this.logger = logger;
        }

        public Task SendMessage(T message,
            string exchange = "",
            string routingKey = "",
            IBasicProperties? properties = default)
        {
            IModel channel = channelPool.Get();

            logger.LogDebug("SendMessage exchange: '{exchange}'", exchange);
            logger.LogDebug("SendMessage routingKey: '{routingKey}'", routingKey);

            try
            {
                var serializedMessage = JsonSerializer.SerializeToUtf8Bytes(message);

                logger.LogDebug("SendMessage channel number: '{channelNumbar}'", channel.ChannelNumber);
                logger.LogDebug("SendMessage channel isOpen: {isOpen}", channel.IsOpen);

                channel.BasicPublish(exchange: exchange,
                                     routingKey: routingKey,
                                     basicProperties: properties,
                                     body: serializedMessage);

                return Task.CompletedTask;
            }
            finally
            {
                channelPool.Return(channel);
            }
        }
    }
}
