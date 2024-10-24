using Microsoft.Azure.Functions.Worker;
using Shop.Media.API.Services.Interfaces;
using Shop.Shared.Models.Medias;

namespace Shop.Media.API.Functions.ActivityTriggers;

public sealed class ImageProcessorActivity(IContainerService _containerService)
{

	[Function(nameof(ImageProcessorActivity))]
	public async Task<MediaResponse> Run([ActivityTrigger] MediaRequest request, FunctionContext executionContext)
	{

		try
		{
			var containerName = Environment.GetEnvironmentVariable("Container.Processed.Media");
			var uri = await _containerService.UploadFileAsync(containerName!, request.Content, request.ContentType);

			return new MediaResponse(request.ProductId, uri);
		}
		catch (Exception ex)
		{
			return default!;
		}
	}
}