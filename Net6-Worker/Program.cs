using Net6_Worker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;

        services.AddHostedService<Worker>();
        services.AddRabbitMQ(configuration);
        services.AddSingleton<IConsumerMessage<Post>, PostConsumerMessage>();
                
    })
    .Build();

await host.RunAsync();
