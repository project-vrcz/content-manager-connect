using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Editor.Views.Pages.Reconnect;

internal sealed class ReconnectPage : VisualElement {
    private readonly RpcClientService _rpcClientService;

    private readonly Label _lastInstanceHostUrlLabel;

    private readonly Button _reconnectButton;
    private readonly Button _forgetButton;

    private const string VisualTreeAssetGuid = "65e607d07a234b81bbe09fbeae6521d9";

    public ReconnectPage(Action sessionForgotCallback) {
        var path = AssetDatabase.GUIDToAssetPath(VisualTreeAssetGuid);
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
        visualTree.CloneTree(this);

        _lastInstanceHostUrlLabel = this.Q<Label>("instance-host-url");

        _reconnectButton = this.Q<Button>("reconnect-button");
        _forgetButton = this.Q<Button>("forget-button");

        if (ConnectEditorApp.Instance is not { } app) {
            Debug.LogWarning("ConnectEditorApp instance is not available.");
            throw new InvalidOperationException("ConnectEditorApp instance is not available.");
        }

        _rpcClientService = app.ServiceProvider.GetRequiredService<RpcClientService>();

        _reconnectButton.clicked += async () => {
            try {
                await _rpcClientService.RestoreSessionAsync();
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                Debug.LogError("Failed to restore RPC session.");

                EditorUtility.DisplayDialog("Reconnect Failed",
                    "Failed to reconnect to the last session:\n\n" +
                    ex,
                    "OK");
            }
        };

        _forgetButton.clicked += async () => {
            try {
                await _rpcClientService.DisconnectAsync();
                sessionForgotCallback();
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                Debug.LogError("Failed to forget RPC session.");

                EditorUtility.DisplayDialog("Forget Failed",
                    "Failed to forget the last session:\n\n" +
                    ex,
                    "OK");
            }
        };

        RegisterCallback<AttachToPanelEvent>(_ => Load());
    }

    private async void Load() {
        try {
            var session = await _rpcClientService.GetLastSessionInfoAsync();
            if (session is null) {
                _lastInstanceHostUrlLabel.text = "Invalid session info. Try restart Editor?";
                return;
            }

            _lastInstanceHostUrlLabel.text = session.Host;
        }
        catch (Exception ex) {
            Debug.LogException(ex);
            Debug.LogError("Failed to get last RPC session info.");
        }
    }
}