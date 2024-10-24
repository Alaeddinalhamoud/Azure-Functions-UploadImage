namespace Shop.Media.API.Services.Interfaces;

public interface IQueueService
{
	Task SendMessageAsync(string queueName, object message, int delay = 0);
}
