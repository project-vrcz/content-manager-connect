using System;
using System.Threading.Tasks;
using VRChatContentPublisherConnect.Editor.Services.Rpc;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace VRChatContentPublisherConnect.Editor.Services;

internal sealed class EditorAppLifetimeService {
    private readonly RpcClientService _rpcClientService;
    private readonly AppSettingsService _appSettingsService;
    private readonly MenuItemService _menuItemService;

    private readonly YesLogger _logger = new(LoggerConst.LoggerPrefix + nameof(EditorAppLifetimeService));

    public EditorAppLifetimeService(
        RpcClientService rpcClientService,
        AppSettingsService appSettingsService,
        MenuItemService menuItemService) {
        _rpcClientService = rpcClientService;
        _appSettingsService = appSettingsService;
        _menuItemService = menuItemService;
    }

    public async Task StartAsync() {
        _menuItemService.Init();
        var settings = _appSettingsService.GetSettings();

        try {
            await _rpcClientService.RestoreSessionAsync(false, settings.LaunchAppWhenStartup);
        }
        catch (Exception ex) {
            _logger.LogDebug(ex, "Failed to auto restore session");
        }
    }
}