using System;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;
using UnityEngine.UIElements;
using VRChatContentManagerConnect.Editor.Services.Rpc;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace VRChatContentManagerConnect.Editor.Views.Pages.NewConnection;

internal sealed class NewConnectionPage : VisualElement {
    private const string VisualTreeAssetGuid = "280eea869a434efd8a9abf4dd83eee43";

    private readonly YesLogger _logger = new(LoggerConst.LoggerPrefix + nameof(NewConnectionPage));

    private readonly RpcClientService _rpcClientService;

    private readonly TextField _hostField;
    private readonly Button _connectButton;

    private readonly VisualElement _connectContainer;
    private readonly VisualElement _challengeContainer;

    private readonly Label _identityPromptLabel;
    private readonly TextField _challengeCodeInputField;
    private readonly Button _challengeButton;
    private readonly Button _cancelChallengeButton;

    public NewConnectionPage() {
        var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(VisualTreeAssetGuid);
        var visualTreeAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
        visualTreeAsset.CloneTree(this);

        _hostField = this.Q<TextField>("host-input-field");
        _connectButton = this.Q<Button>("connect-button");

        _connectContainer = this.Q<VisualElement>("connect-container");
        _challengeContainer = this.Q<VisualElement>("challenge-container");

        _identityPromptLabel = this.Q<Label>("identity-prompt-label");
        _challengeCodeInputField = this.Q<TextField>("code-input-field");
        _challengeButton = this.Q<Button>("challenge-button");
        _cancelChallengeButton = this.Q<Button>("cancel-challenge-button");

        if (ConnectEditorApp.Instance is not { } app) {
            _logger.LogWarning("ConnectEditorApp instance is not available.");
            throw new InvalidOperationException("ConnectEditorApp instance is not available.");
        }

        _rpcClientService = app.ServiceProvider.GetRequiredService<RpcClientService>();

        RegisterCallback<AttachToPanelEvent>(_ => {
            _rpcClientService.StateChanged += OnRpcClientServiceStateChanged;
        });

        RegisterCallback<DetachFromPanelEvent>(_ => {
            _rpcClientService.StateChanged -= OnRpcClientServiceStateChanged;
        });

        _connectButton.clicked += async () => {
            try {
                await _rpcClientService.RequestChallengeAsync(_hostField.value);
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"Failed to request challenge: {ex}");

                UnityEditor.EditorUtility.DisplayDialog(
                    "Connection Error",
                    $"Failed to connect to {_hostField.value}:\n\n{ex}",
                    "OK");
            }
        };

        _challengeCodeInputField.RegisterValueChangedCallback(args => {
            _challengeCodeInputField.SetValueWithoutNotify(args.newValue.ToUpperInvariant());
        });

        _challengeButton.clicked += async () => {
            try {
                await _rpcClientService.CompleteChallengeAsync(_challengeCodeInputField.text);
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"Failed to complete challenge: {ex}");

                UnityEditor.EditorUtility.DisplayDialog(
                    "Challenge Error",
                    $"Failed to complete challenge:\n\n{ex}",
                    "OK");
            }
        };

        _cancelChallengeButton.clicked += async () => {
            try {
                await _rpcClientService.ForgetAndDisconnectAsync();
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to cancel challenge: " + ex);

                UnityEditor.EditorUtility.DisplayDialog(
                    "Cancel Challenge Error",
                    $"Failed to cancel challenge:\n\n{ex}",
                    "OK");
            }
        };

        UpdateUi();
    }

    private void UpdateUi() {
        _identityPromptLabel.text =
            _rpcClientService.GetIdentityPrompt() ?? "Invalid Status, Try restart connect process?";

        if (_rpcClientService.State == RpcClientState.AwaitingChallenge) {
            _connectContainer.style.display = DisplayStyle.None;
            _challengeContainer.style.display = DisplayStyle.Flex;
        }
        else {
            _connectContainer.style.display = DisplayStyle.Flex;
            _challengeContainer.style.display = DisplayStyle.None;
        }
    }

    private void OnRpcClientServiceStateChanged(object sender, RpcClientState e) {
        MainThreadDispatcher.Dispatch(UpdateUi);
    }
}