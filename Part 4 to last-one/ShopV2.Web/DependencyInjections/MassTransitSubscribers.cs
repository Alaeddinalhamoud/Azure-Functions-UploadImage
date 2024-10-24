using MassTransit;
using Shop.Shared.Models;
using Shop.Shared.Models.Medias;
using ShopV2.Web.Services;
using System.Reflection;

namespace ShopV2.Web.DependencyInjections;

public static class MassTransitSubscribers
{
	public static IServiceCollection AddMassTransitSubscriber(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddMassTransit(x =>
		{
			x.AddServiceBusMessageScheduler();
			x.AddConsumers(Assembly.GetExecutingAssembly());
			x.SetKebabCaseEndpointNameFormatter();

			x.UsingAzureServiceBus((ctx, cfg) =>
			{
				cfg.Host(configuration["ServiceBus.ConnectionString"]);
				cfg.UseServiceBusMessageScheduler();

				cfg.Message<MediaResponse>(m => m.SetEntityName(Constants.MediaTopic));

				cfg.SubscriptionEndpoint<MediaResponse>(Constants.MediaProductSubscription, e =>
				{
					e.ConfigureConsumer<MediaConsumer>(ctx);
					e.UseRawJsonSerializer();
					e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(3)));
				});

			});

		});
		return services;
	}
}
