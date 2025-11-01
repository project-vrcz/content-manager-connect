namespace VRChatContentManagerConnect.Editor.Models.RpcApi.Response;

public record MetadataResponse(
    string InstanceName,
    string Implementation,
    string ImplementationVersion,
    string ApiVersion);