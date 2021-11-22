using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Net6_RabbitMQ.Producer
{
    public interface IProducerMessage<T>
    {
        Task SendMessage(
            T message, string exchange = "", string routingKey = "", 
            IBasicProperties? properties = null);
    }
}
