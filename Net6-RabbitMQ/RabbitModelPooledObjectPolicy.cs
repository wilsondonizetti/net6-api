using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Net6_RabbitMQ
{
    //https://www.c-sharpcorner.com/article/publishing-rabbitmq-message-in-asp-net-core/
    public class RabbitModelPooledObjectPolicy : IPooledObjectPolicy<IModel>
    {
        private readonly RabbitOptions options;
        private readonly IConnection connection;

        public RabbitModelPooledObjectPolicy(IConnection connection, IOptions<RabbitOptions> options)
        {
            this.options = options.Value;
            this.connection = connection;
        }

        public IModel Create()
        {
            var channel = connection.CreateModel();
            channel.BasicQos(0, options.PrefetchCount, global: false);

            return channel;
        }

        public bool Return(IModel obj)
        {
            if (obj?.IsOpen ?? false)
            {
                return true;
            }
            else
            {
                //Disposing channel and connection objects is not enough, they must be explicitly closed with the API methods
                //https://www.rabbitmq.com/dotnet-api-guide.html
                obj?.Dispose();

                return false;
            }
        }
    }
}