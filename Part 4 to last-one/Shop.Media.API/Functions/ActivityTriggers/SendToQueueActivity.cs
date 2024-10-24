using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shop.Media.API.Services.Interfaces;
using Shop.Shared.Models.Medias;

namespace Shop.Media.API.Functions.ActivityTriggers;

public sealed class SendToQueueActivity(IQueueService _queueService)
{
	private readonly string _uriQueueName = Environment.GetEnvironmentVariable("Queue.Uri.Media")!;

	[Function(nameof(SendToQueueActivity))]
	public async Task Run([ActivityTrigger] List<MediaResponse> response, FunctionContext executionContext)
	{
		ILogger logger = executionContext.GetLogger(nameof(SendToQueueActivity));

		try
		{
			logger.LogInformation("SendToQueueActivity triggered.");

			if (string.IsNullOrWhiteSpace(_uriQueueName))
				throw new Exception($"{nameof(SendToQueueActivity)} - Environment variable 'Queue.Uri.Media' not set.");

			if (response is null)
			{
				logger.LogError("{Name} function error Empty/Null data passed", nameof(SendToQueueActivity));
				throw new ArgumentNullException(nameof(response));
			}

			foreach (var item in response)
			{
				if (item is null && string.IsNullOrWhiteSpace(item!.Uri)) continue;

				await _queueService.SendMessageAsync(_uriQueueName, item).ConfigureAwait(false);
				logger.LogInformation("{Name} function processed a request for {Uri}", nameof(SendToQueueActivity), item.Uri);
			}

			logger.LogInformation("{Name} function processed a request", nameof(SendToQueueActivity));
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "{Name} function error", nameof(SendToQueueActivity));
		}
	}
}