using System.Text.Json.Serialization;

namespace Shop.Shared.Models.Medias;

public sealed class MediaRequest
{
    [JsonPropertyName("productId")]
    public int ProductId { get; set; }

	[JsonPropertyName("contentType")]
    public string ContentType { get; set; } = null!;

	[JsonPropertyName("content")]
    public string Content { get; set; } = null!;
}
