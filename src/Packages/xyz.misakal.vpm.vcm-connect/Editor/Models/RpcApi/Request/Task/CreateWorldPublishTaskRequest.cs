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
    [property: JsonPropertyName("WorldSignature")]
    string? WorldSignature = null
);