using System.Text.Json.Serialization;

namespace VRChatContentPublisherConnect.Editor.Models.RpcApi.Request;

internal record RequestChallengeRequest(
    [property: JsonPropertyName("ClientId")]
    string ClientId,
    [property: JsonPropertyName("IdentityPrompt")]
    string IdentityPrompt,
    [property: JsonPropertyName("ClientName")]
    string ClientName
);