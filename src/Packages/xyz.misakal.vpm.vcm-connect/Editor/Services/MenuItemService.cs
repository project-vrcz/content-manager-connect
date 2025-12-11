using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VRChatContentManagerConnect.Editor.MenuItems;
using VRChatContentManagerConnect.Editor.Models;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Editor.Services;

internal sealed class MenuItemService {
    private static MenuItemService? Instance { get; set; }

    private const string IsRpcConnectedMenuItemPath = MenuItemPath.RootMenuItemPath + "Is RPC Connected";

    private const string EnableContentManagerPublishFlowMenuItemPath =
        MenuItemPath.RootMenuItemPath + "Enable Content Manager Publish Flow";

    private const string RestoreSessionMenuItemPath = MenuItemPath.RootMenuItemPath + "Restore Session";

    private readonly RpcClientService _rpcClientService;
    private readonly AppSettingsService _appSettingsService;

    private readonly AppSettings _appSettings;

    public MenuItemService(RpcClientService rpcClientService, AppSettingsService appSettingsService) {
        _rpcClientService = rpcClientService;
        _appSettingsService = appSettingsService;

        _appSettings = _appSettingsService.GetSettings();
    }

    public void Init() {
        _rpcClientService.StateChanged += (_, _) => MainThreadDispatcher.Dispatch(UpdateMenuItem);

        Instance = this;

        MainThreadDispatcher.Dispatch(() => {
            EditorApplication.update += () =>
                Menu.SetChecked(EnableContentManagerPublishFlowMenuItemPath, _appSettings.UseContentManager);

            UpdateMenuItem();
        });
    }

    private void UpdateMenuItem() {
        Menu.SetChecked(IsRpcConnectedMenuItemPath, _rpcClientService.State == RpcClientState.Connected);
    }

#region Menu Items

    [MenuItem(IsRpcConnectedMenuItemPath, priority = 0)]
    private static void IsRpcConnected() {
        // Menu item with no action
    }

    [MenuItem(IsRpcConnectedMenuItemPath, true)]
    private static bool MakeIsRpcConnectedDisabled() => false;

    [MenuItem(RestoreSessionMenuItemPath, priority = 0)]
    private static async void RestoreSession() {
        if (Instance is not { } service)
            return;

        try {
            await service._rpcClientService.RestoreSessionAsync();
        }
        catch (Exception ex) {
            EditorUtility.DisplayDialog(
                "Error Restoring Session",
                $"An error occurred while restoring the session:\n{ex.Message}",
                "OK"
            );
        }
    }

    [MenuItem(EnableContentManagerPublishFlowMenuItemPath, priority = 100)]
    private static void ToggleContentManagerPublishFlow() {
        if (Instance is not { } service)
            return;

        var settings = service._appSettingsService.GetSettings();
        settings.UseContentManager = !settings.UseContentManager;
        service._appSettingsService.SaveSettings();
    }

#endregion
}