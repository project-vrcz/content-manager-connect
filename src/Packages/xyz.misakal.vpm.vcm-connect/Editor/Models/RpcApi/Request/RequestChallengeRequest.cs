using System.Text.Json.Serialization;

namespace VRChatContentManagerConnect.Editor.Models.RpcApi.Request;

internal record RequestChallengeRequest(
    [property: JsonPropertyName("ClientId")]
    string ClientId,
    [property: JsonPropertyName("IdentityPrompt")]
    string IdentityPrompt
);