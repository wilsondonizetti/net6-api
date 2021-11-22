using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Net6_RabbitMQ.Consumer;
using RabbitMQ.Client;

namespace Net6_Common
{
    //Essa classe deve ser substituida pelo cenario real
    public class TestConsumerMessage : ConsumerMessageBase<TestMessage>
    {        
        public TestConsumerMessage(
            IServiceProvider serviceProvider,
            ObjectPool<IModel> objectPool,
            IOptions<ConsumerOptions> options,
            IConfiguration configuration,
            ILogger<ConsumerMessageBase<TestMessage>> logger)
        : base(serviceProvider, objectPool, options, configuration, logger)
        {            
        }

        protected override async Task<MessageProcessResult> ProcessMessageAsync(
            TestMessage message, MessageReceivedArgs args, CancellationToken cancellationToken)
        {
            try
            {                

                cancellationToken.ThrowIfCancellationRequested();

                await Task.Run(() =>
                {
                    logger.LogInformation("Message received: '{mensagem}'", message);

                    logger.LogInformation("Message read: '{mensagem}'", message);
                });
                
                return MessageProcessResult.Success;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error to process message: {mensagem}", message);
                return MessageProcessResult.Fail;
            }
        }
    }
}
