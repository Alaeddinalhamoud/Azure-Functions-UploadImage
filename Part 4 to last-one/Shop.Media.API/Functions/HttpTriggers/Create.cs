using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Shop.Media.API.Functions.HttpTriggers
{
    public class Create(ILogger<Create> _logger)
    {

        [Function("Create")]
        [BlobOutput("processing-media/{rand-guid}.json")]
        public async Task<string> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string body = await new StreamReader(req.Body).ReadToEndAsync();

                if (string.IsNullOrEmpty(body))
                    throw new ArgumentNullException(nameof(body));

                _logger.LogInformation("Http tigger {Name} processed a request", nameof(Create));

                return body;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing http trigger {Name}", nameof(Create));
                throw;
            }
        }
    }
}
