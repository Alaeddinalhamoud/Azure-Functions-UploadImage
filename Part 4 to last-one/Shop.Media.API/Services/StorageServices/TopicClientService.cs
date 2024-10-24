using Azure;
using Microsoft.Azure.ServiceBus;
using Shop.Media.API.Services.Interfaces;
using System.Text;

namespace Shop.Media.API.Services.StorageServices;

public class TopicClientService(string connectionString) : ITopicClientService
{
	public async Task SendMessageAsync(string topicName, string message)
	{
		if (string.IsNullOrWhiteSpace(connectionString))
			throw new RequestFailedException($" {nameof(TopicClientService)} Connection string is empty");

		var topicClient = new TopicClient(connectionString, topicName);

		var content = new Message(Encoding.UTF8.GetBytes(message));

		content.ContentType = "application/json";

		await topicClient.SendAsync(content);
	}
}

