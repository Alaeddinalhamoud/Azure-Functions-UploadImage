using Azure.Storage.Queues;
using Shop.Media.API.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace Shop.Media.API.Services.StorageServices;

public class QueueService(string connectionString) : IQueueService
{
	public async Task SendMessageAsync(string queueName, object message, int delay = 0)
	{
		var queueMessage = JsonSerializer.Serialize(message);

		// Instantiate a QueueClient which will be used to create and manipulate the queue
		var queueClient = new QueueClient(connectionString, queueName);

		await queueClient.CreateIfNotExistsAsync();

		await queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(queueMessage)), delay == 0 ? default : TimeSpan.FromSeconds(delay));
	}
}
