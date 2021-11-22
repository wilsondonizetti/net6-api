namespace Net6_RabbitMQ.Consumer
{
    public class ConsumerOptions
    {
        public string QueueName { get; set; } = "";
        public string Exchange { get; set; } = "";
        public string RetryExchange { get; set; } = "";
        public string RetryRoutingKey { get; set; } = "";
    }
}
