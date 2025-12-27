using System.Text.Json.Serialization;

namespace VRChatContentPublisherConnect.Editor.Models.RpcApi.Request;

internal record ChallengeRequest(
    [property: JsonPropertyName("ClientId")]
    string ClientId,
    [property: JsonPropertyName("Code")] string Code,
    [property: JsonPropertyName("IdentityPrompt")] string IdentityPrompt
);