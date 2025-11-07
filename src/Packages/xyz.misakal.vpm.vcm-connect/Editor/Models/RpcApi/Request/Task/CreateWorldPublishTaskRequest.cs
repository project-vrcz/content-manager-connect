using System.Text.Json.Serialization;

namespace VRChatContentManagerConnect.Editor.Models.RpcApi.Request.Task;

public record CreateWorldPublishTaskRequest(
    [property: JsonPropertyName("WorldId")]
    string WorldId,
    [property: JsonPropertyName("WorldBundleFileId")]
    string WorldBundleFileId,
    [property: JsonPropertyName("Name")]
    string Name,
    [property: JsonPropertyName("Platform")]
    string Platform,
    [property: JsonPropertyName("UnityVersion")]
    string UnityVersion,
    [property: JsonPropertyName("AuthorId")]
    string? AuthorId,
    [property: JsonPropertyName("WorldSignature")]
    string? WorldSignature = null,
    [property: JsonPropertyName("ThumbnailFileId")]
    string? ThumbnailFileId = null,
    [property: JsonPropertyName("Description")]
    string? Description = null,
    [property: JsonPropertyName("Tags")]
    string[]? Tags = null,
    [property: JsonPropertyName("ReleaseStatus")]
    string? ReleaseStatus = null,
    [property: JsonPropertyName("Capacity")]
    int? Capacity = null,
    [property: JsonPropertyName("RecommendedCapacity")]
    int? RecommendedCapacity = null,
    [property: JsonPropertyName("PreviewYoutubeId")]
    string? PreviewYoutubeId = null,
    [property: JsonPropertyName("UdonProducts")]
    string[]? UdonProducts = null
);