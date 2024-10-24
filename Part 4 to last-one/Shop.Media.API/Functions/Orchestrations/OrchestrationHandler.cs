using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Shop.Media.API.Functions.ActivityTriggers;
using Shop.Shared.Models.Medias;
using System.Text;
using System.Text.Json;

namespace Shop.Media.API.Functions.Orchestrations
{
    public static class OrchestrationHandler
    {
        [Function(nameof(OrchestrationHandler))]
        public static async Task<IActionResult> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
		{
			ILogger logger = context.CreateReplaySafeLogger(nameof(OrchestrationHandler));

			try
			{
				logger.LogInformation("Started orchestration with ID = '{instanceId}'.", context.InstanceId);

				var content = context.GetInput<string>();

				if (string.IsNullOrWhiteSpace(content)) return new BadRequestObjectResult("No content to process.");

				var jsonData = JsonSerializer.Deserialize<List<MediaRequest>>(content);

				if (jsonData == null || !jsonData.Any()) return new BadRequestResult();

				List<MediaResponse> outputs = new();

				foreach (var item in jsonData)
					outputs.Add(await context.CallActivityAsync<MediaResponse>(nameof(ImageProcessorActivity), item));

				if (outputs.Any())
					await context.CallActivityAsync(nameof(SendToQueueActivity), outputs);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, ex.Message);
			}
			return new OkObjectResult($"Images processed at {DateTime.UtcNow}");
		}


		[Function("OrchestrationHandler_HttpStart")]
        public static async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("OrchestrationHandler_HttpStart");

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(OrchestrationHandler));

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            return await client.CreateCheckStatusResponseAsync(req, instanceId);
        }


        [Function("OrchestrationHandler_BlobStart")]
        public static async Task BlobStart(
          [BlobTrigger("processing-media/{name}", Connection = "AzureWebJobsStorage")] Stream stream, string name,
          [DurableClient] DurableTaskClient client,
          FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("OrchestrationHandler_HttpStart");
            try
            {

                using var blobStreamReader = new StreamReader(stream);

                var content = await blobStreamReader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    logger.LogError("Error processing http trigger {Name}", nameof(BlobStart));
                }

                logger.LogInformation("Saying hello to {name}.", content);


                // Function input comes from the request content.
                string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(OrchestrationHandler), content);

                logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing http trigger {Name}", nameof(BlobStart));
            }

        }
    }
}
