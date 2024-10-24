using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shop.Media.API.Services.Interfaces;
using Shop.Media.API.Services.StorageServices;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddTransient<IContainerService>(x => new ContainerService(Environment.GetEnvironmentVariable("AzureWebJobsStorage")!));
		services.AddTransient<IQueueService>(x => new QueueService(Environment.GetEnvironmentVariable("AzureWebJobsStorage")!));
		services.AddTransient<ITopicClientService>(x => new TopicClientService(Environment.GetEnvironmentVariable("TopicClient.ConnectionString")!));
	})
    .Build();

host.Run();
