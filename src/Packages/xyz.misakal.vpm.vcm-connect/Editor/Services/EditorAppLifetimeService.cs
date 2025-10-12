using System.Threading.Tasks;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Editor.Services;

internal sealed class EditorAppLifetimeService {
    private readonly RpcClientService _rpcClientService;
    
    public EditorAppLifetimeService(RpcClientService rpcClientService) {
        _rpcClientService = rpcClientService;
    }

    public async Task StartAsync() {
        await _rpcClientService.TryRestoreSessionAsync();
    }
}