namespace Shop.Media.API.Services.Interfaces;

public interface IContainerService
{
	Task<string> UploadFileAsync(string containerName, string content, string contentType);
}
