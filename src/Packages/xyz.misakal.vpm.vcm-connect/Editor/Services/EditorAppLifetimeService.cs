using System;
using System.Threading.Tasks;
using UnityEngine;
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

        try {
            await _rpcClientService.RestoreSessionAsync();
        }
        catch (Exception ex) {
            Debug.LogException(ex);
            Debug.LogError("[VRCCM.Connect] Failed to restore RPC client session: " + ex.Message);
        }
    }
}