using Microsoft.Extensions.DependencyInjection;
using VRChatContentManagerConnect.Editor;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Avatars.Editor.ContinuousAvatarUploader;

internal static class RpcClientServiceInstance {
    private static RpcClientService? _rpcClientService;
    
    public static RpcClientService? TryGetRpcClientService() {
        if (_rpcClientService != null)
            return _rpcClientService;

        if (ConnectEditorApp.Instance is not { } app) {
            return null;
        }

        _rpcClientService = app.ServiceProvider.GetRequiredService<RpcClientService>();
        return _rpcClientService;
    }
}