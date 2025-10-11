using System;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRChatContentManagerConnect.Editor.Services;

namespace VRChatContentManagerConnect.Editor.Views;

internal class ContentManagerSettingsWindow : EditorWindow {
    private const string VisualTreeAssetPath =
        "Packages/xyz.misakal.vpm.vcm-connect/Editor/Views/ContentManagerSettingsWindow.uxml";

    private VisualElement _disconnectedStateContainer;
    private TextField _rpcHostInputField;
    private Button _requestChallengeButton;

    private VisualElement _awaitingChallengeStateContainer;
    private Label _identityPromptLabel;
    private TextField _challengeCodeInputField;
    private Button _challengeButton;
    private Button _cancelChallengeButton;

    private VisualElement _connectedStateContainer;
    private Button _disconnectButton;

    private Label _stateDisplayLabel;
    private Label _clientIdLabel;

    [MenuItem("Window/VRChat Content Manager/Settings", priority = 2000)]
    public static void ShowSettings() {
        var window = GetWindow<ContentManagerSettingsWindow>();
        window.titleContent = new GUIContent("Connect Settings");
    }

    public void CreateGUI() {
        var root = rootVisualElement;
        var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(VisualTreeAssetPath);
        var content = visualTreeAsset.Instantiate();

        root.Add(content);

        _disconnectedStateContainer = content.Q<VisualElement>("disconnected-state-container");
        _rpcHostInputField = content.Q<TextField>("host-inputfield");
        _requestChallengeButton = content.Q<Button>("request-challenge-button");

        _awaitingChallengeStateContainer = content.Q<VisualElement>("challenge-state-container");
        _identityPromptLabel = content.Q<Label>("identity-prompt-label");
        _challengeCodeInputField = content.Q<TextField>("code-inputfield");
        _challengeButton = content.Q<Button>("challenge-button");
        _cancelChallengeButton = content.Q<Button>("cancel-challenge-button");

        _connectedStateContainer = content.Q<VisualElement>("connected-state-container");
        _disconnectButton = content.Q<Button>("disconnect-button");

        _stateDisplayLabel = content.Q<Label>("state-display-label");
        _clientIdLabel = content.Q<Label>("client-id-label");

        if (ConnectEditorApp.Instance is not { } app)
            return;

        var rpcClientService = app.ServiceProvider.GetRequiredService<RpcClientService>();

        EditorApplication.update += () => UpdateConnectionState(rpcClientService);
        UpdateConnectionState(rpcClientService);

        rpcClientService.IdentityPromptChanged += (_, prompt) => { _identityPromptLabel.text = prompt; };
        _identityPromptLabel.text = rpcClientService.GetIdentityPrompt() ?? "";

        _clientIdLabel.text = rpcClientService.GetClientId();

        _requestChallengeButton.clicked += async () => {
            await rpcClientService.RequestChallengeAsync(_rpcHostInputField.value);
        };

        _challengeButton.clicked += async () => {
            await rpcClientService.CompleteChallengeAsync(_challengeCodeInputField.value);
        };

        _disconnectButton.clicked += async () => { await rpcClientService.DisconnectAsync(); };
        _cancelChallengeButton.clicked += async () => { await rpcClientService.DisconnectAsync(); };
    }

    private void UpdateConnectionState(RpcClientService rpcClientService) {
        var state = rpcClientService.State;
        _stateDisplayLabel.text = state.ToString();

        _disconnectedStateContainer.style.display = DisplayStyle.None;
        _awaitingChallengeStateContainer.style.display = DisplayStyle.None;
        _connectedStateContainer.style.display = DisplayStyle.None;

        switch (state) {
            case RpcClientState.AwaitingChallenge:
                _awaitingChallengeStateContainer.style.display = DisplayStyle.Flex;
                break;
            case RpcClientState.Connected:
                _connectedStateContainer.style.display = DisplayStyle.Flex;
                break;
            case RpcClientState.Disconnected:
            default:
                _disconnectedStateContainer.style.display = DisplayStyle.Flex;
                break;
        }
    }
}