namespace Shop.Media.API.Services.Interfaces;

public interface ITopicClientService
{
	Task SendMessageAsync(string topicName, string message);
}
