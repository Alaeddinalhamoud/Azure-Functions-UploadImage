using MassTransit;
using Shop.Shared.Models.Medias;
using ShopV2.Web.Data;
using ShopV2.Web.Models;

namespace ShopV2.Web.Services;

public class MediaConsumer(ILogger<MediaConsumer> logger, ApplicationDbContext dbContext) : IConsumer<MediaResponse>
{
	public async Task Consume(ConsumeContext<MediaResponse> context)
	{
		logger.LogInformation($"MediaConsumer - {context.Message.Uri}");

		var imageUrl = new Image { ProductId = context.Message.ProductId, Url = context.Message.Uri };

		await dbContext.Images.AddAsync(imageUrl);

		await dbContext.SaveChangesAsync();
	}
}