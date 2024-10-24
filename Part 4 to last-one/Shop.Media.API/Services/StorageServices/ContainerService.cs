using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Shop.Media.API.Services.Interfaces;

namespace Shop.Media.API.Services.StorageServices;

public class ContainerService(string connectionString) : IContainerService
{
	public async Task<string> UploadFileAsync(string containerName, string content, string contentType)
	{
		var fileExtension = GetFileExtensionFromContentType(contentType);

		if (string.IsNullOrWhiteSpace(fileExtension))
			throw new RequestFailedException($"ContentType named '{contentType}' is not supported");

		// Get a reference to a container using _containerName
		var container = new BlobContainerClient(connectionString, containerName);

		// Could be added if we have a reliable way of creating configs
		await container.CreateIfNotExistsAsync();

		// Get a reference to a blob based on filename in the previous container

		var blob = container.GetBlobClient($"{Guid.NewGuid()}{fileExtension}");

		byte[] data = Convert.FromBase64String(content);

		// Upload serialized item to blob storage
		await blob.UploadAsync(new MemoryStream(data), new BlobHttpHeaders { ContentType = contentType });

		// Return filename
		return blob.Uri.ToString();
	}

	#region Private Methods
	static string? GetFileExtensionFromContentType(string contentType) => contentType switch
	{
		"image/jpeg" => ".jpg",
		"image/png" => ".png",
		"image/gif" => ".gif",
		"image/bmp" => ".bmp",
		"image/webp" => ".webp",
		"application/json" => ".json",
		_ => null
	};
	#endregion
}
