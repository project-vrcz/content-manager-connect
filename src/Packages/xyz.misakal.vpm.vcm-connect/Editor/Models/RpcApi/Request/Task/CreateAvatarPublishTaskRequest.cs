using System.Text.Json.Serialization;

namespace VRChatContentManagerConnect.Editor.Models.RpcApi.Request.Task;

internal record CreateAvatarPublishTaskRequest(
    [property: JsonPropertyName("AvatarId")]
    string AvatarId,
    [property: JsonPropertyName("AvatarBundleFileId")]
    string AvatarBundleFileId,
    [property: JsonPropertyName("Name")]
    string Name,
    [property: JsonPropertyName("Platform")]
    string Platform,
    [property: JsonPropertyName("UnityVersion")]
    string UnityVersion);