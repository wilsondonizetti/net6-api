using System.Threading;
using System.Threading.Tasks;
namespace Net6_RabbitMQ.Consumer
{
    public interface IConsumerMessage<T>
    {
        ConsumerOptions Options { get; }

        Task BeginReceiveMessagesAsync(CancellationToken cancellationToken);
        void EndReceiveMessages();
    }
}