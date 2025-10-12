using System.Text.Json.Serialization;

namespace VRChatContentManagerConnect.Editor.Models.RpcApi.Response;

internal record UploadFileResponse(
    [property: JsonPropertyName("FileId")] string FileId
);