using System;
using System.Threading.Tasks;
using UnityEngine;
using VRChatContentManagerConnect.Editor.Services.Rpc;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace VRChatContentManagerConnect.Editor.Services;

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
        _appSettingsService.GetSettings();

        try {
            await _rpcClientService.RestoreSessionAsync();
        }
        catch (Exception ex) {
            _logger.LogDebug(ex, "Failed to auto restore session");
        }
    }
}