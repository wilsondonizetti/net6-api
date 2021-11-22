using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Net6_RabbitMQ;
using Net6_RabbitMQ.Consumer;
using Net6_RabbitMQ.Producer;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Security;
using System.Security.Authentication;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<RabbitOptions>().Bind(configuration.GetSection("RabbitMQ"));
            services.AddOptions<ConsumerOptions>().Bind(configuration.GetSection("RabbitMQ:Consumer"));

            services.AddTransient(typeof(IProducerMessage<>), typeof(ProducerMessage<>));

            services.AddSingleton<IConnection>(CreateConnection);
            services.AddSingleton<ObjectPool<IModel>>(CreateObjectPool);

            return services;
        }

        private static IConnection CreateConnection(IServiceProvider serviceProvider)
        {
            try
            {
                var options = serviceProvider.GetRequiredService<IOptions<RabbitOptions>>();

                var hosts = options.Value.Hosts?.Split(',').ToList<string>();

                var factory = new ConnectionFactory
                {
                    Port = options.Value.Port,
                    UserName = options.Value.UserName,
                    Password = options.Value.Password,
                    VirtualHost = options.Value.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    DispatchConsumersAsync = true,
                    NetworkRecoveryInterval = options.Value.NetworkRecoveryInterval,
                    Ssl =
                    {
                        ServerName = hosts?.FirstOrDefault(),
                        Enabled = true,
                        Version = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12,
                        AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateChainErrors
                    }
                };

                return factory.CreateConnection(hosts);
            }
            catch (BrokerUnreachableException ex)
            {
                throw new ApplicationException("RabbitMQ can't connect to server", ex);
            }
        }

        private static ObjectPool<IModel> CreateObjectPool(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<RabbitOptions>>();
            var connection = serviceProvider.GetRequiredService<IConnection>();

            var policy = new RabbitModelPooledObjectPolicy(connection, options);
            var provider = new DefaultObjectPoolProvider();
            provider.MaximumRetained = options.Value.MaximumChannelInPool;

            return provider.Create(policy);
        }
    }
}
