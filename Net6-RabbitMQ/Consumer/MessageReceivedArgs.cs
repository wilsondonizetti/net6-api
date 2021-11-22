using System.Collections.Generic;

namespace Net6_RabbitMQ.Consumer
{
    public class MessageReceivedArgs
    {
        public IDictionary<string, object> MessageHeaders { get; }
        public string ConsumerTag { get; }
        public ulong DeliveryTag { get; }
        public string Exchange { get; }
        public bool Redelivered { get; }
        public string RoutingKey { get;}
        public int RetryCount { get; }

        internal MessageReceivedArgs(IDictionary<string, object> messageHeaders, string consumerTag, 
            ulong deliveryTag, string exchange, bool redelivered, string routingKey, int retryCount)
        {
            MessageHeaders = messageHeaders;
            ConsumerTag = consumerTag;
            DeliveryTag = deliveryTag;
            Exchange = exchange;
            Redelivered = redelivered;
            RoutingKey = routingKey;
            RetryCount = retryCount;
        }
    }
}