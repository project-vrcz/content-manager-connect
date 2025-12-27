using System.Text.Json.Serialization;

namespace VRChatContentPublisherConnect.Editor.Models.RpcApi.Response;

internal record UploadFileResponse(
    [property: JsonPropertyName("FileId")] string FileId
);