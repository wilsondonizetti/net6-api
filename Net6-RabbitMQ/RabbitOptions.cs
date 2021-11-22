using System;
namespace Net6_RabbitMQ
{
    public class RabbitOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public int MaximumChannelInPool { get; set; } = 10;
        public ushort PrefetchCount { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Hosts { get; set; }
        public string? VirtualHost { get; set; } = "/";
        public int Port { get; set; } = 5671;
        public TimeSpan NetworkRecoveryInterval { get; set; } = new TimeSpan(2000);

    }
}
