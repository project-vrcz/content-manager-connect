using System;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;
using UnityEngine.UIElements;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Editor.Views.Pages.Connected;

internal sealed class ConnectedPage : VisualElement {
    private readonly RpcClientService _rpcClientService;

    private readonly Label _instanceNameLabel;
    private readonly Button _disconnectButton;

    private const string VisualTreeAssetGuid = "53ac2986a9ff4c81bb6ee5ff9978953c";

    public ConnectedPage() {
        var path = UnityEditor.AssetDatabase.GUIDToAssetPath(VisualTreeAssetGuid);
        var visualTreeAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);

        visualTreeAsset.CloneTree(this);

        _instanceNameLabel = this.Q<Label>("instance-name");
        _disconnectButton = this.Q<Button>("disconnect-button");

        if (ConnectEditorApp.Instance is not { } app) {
            Debug.LogWarning("ConnectEditorApp instance is not available.");
            throw new InvalidOperationException("ConnectEditorApp instance is not available.");
        }

        _rpcClientService = app.ServiceProvider.GetRequiredService<RpcClientService>();

        _disconnectButton.clicked += async () => await _rpcClientService.ForgetAndDisconnectAsync();

        RegisterCallback<AttachToPanelEvent>(_ => {
            _rpcClientService.StateChanged += OnRpcClientServiceStateChanged;
        });

        RegisterCallback<DetachFromPanelEvent>(_ => {
            _rpcClientService.StateChanged -= OnRpcClientServiceStateChanged;
        });

        UpdateUi();
    }

    private void UpdateUi() {
        if (_rpcClientService.State != RpcClientState.Connected || _rpcClientService.InstanceName is null) {
            _instanceNameLabel.text = "Invalid Status, Try reconnect?";
        }

        _instanceNameLabel.text = _rpcClientService.InstanceName;
    }

    private void OnRpcClientServiceStateChanged(object sender, RpcClientState e) {
        MainThreadDispatcher.Dispatch(UpdateUi);
    }
}