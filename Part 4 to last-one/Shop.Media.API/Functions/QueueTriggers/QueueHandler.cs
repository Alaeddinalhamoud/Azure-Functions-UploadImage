using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shop.Media.API.Services.Interfaces;
using Shop.Shared.Models;
using Shop.Shared.Models.Medias;
using System.Text.Json;

namespace Shop.Media.API.Functions.QueueTriggers;

public class QueueHandler(ILogger<QueueHandler> logger, ITopicClientService topicClientService)
{
	[Function(nameof(QueueHandler))]
	public async Task Run([QueueTrigger("media-uri", Connection = "AzureWebJobsStorage")] string message)
	{
		logger.LogInformation("{Name} Queue trigger function processed: {Time}", nameof(QueueHandler), DateTime.UtcNow);

		try
		{
			if (string.IsNullOrWhiteSpace(message))
			{
				logger.LogWarning("{Name} - Message is null", nameof(QueueHandler));
				throw new Exception($"{nameof(QueueHandler)} - Message is null");
			}

			var serviceBusMessage = JsonSerializer.Deserialize<MediaResponse>(message);

			await topicClientService.SendMessageAsync(Constants.MediaTopic, message);

			logger.LogInformation("{Name} function processed a request for {ProductId}", nameof(QueueHandler), serviceBusMessage!.ProductId);

		}
		catch (Exception ex)
		{
			logger.LogError(ex, "{Name} function error", nameof(QueueHandler));
		}
	}
}
