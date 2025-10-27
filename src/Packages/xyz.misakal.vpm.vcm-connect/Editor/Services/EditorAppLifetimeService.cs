using System.Threading.Tasks;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Editor.Services;

internal sealed class EditorAppLifetimeService {
    private readonly RpcClientService _rpcClientService;
    private readonly AppSettingsService _appSettingsService;
    
    public EditorAppLifetimeService(RpcClientService rpcClientService, AppSettingsService appSettingsService) {
        _rpcClientService = rpcClientService;
        _appSettingsService = appSettingsService;
    }

    public async Task StartAsync() {
        _appSettingsService.GetSettings();
        await _rpcClientService.TryRestoreSessionAsync();
    }
}