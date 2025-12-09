using System;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRChatContentManagerConnect.Editor.Services;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Editor.Views {
    public class ContentManagerSettingsWindow : EditorWindow {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        private VisualElement _noRequiredPackageInstalledContainer;

        private VisualElement _disconnectedStateContainer;
        private TextField _rpcHostInputField;
        private Button _requestChallengeButton;
        private Button _tryRestoreLastSessionButton;

        private VisualElement _awaitingChallengeStateContainer;
        private Label _identityPromptLabel;
        private TextField _challengeCodeInputField;
        private Button _challengeButton;
        private Button _cancelChallengeButton;

        private VisualElement _connectedStateContainer;
        private Button _disconnectButton;

        private Label _stateDisplayLabel;
        private Label _clientIdLabel;

        private Toggle _enableContentManagerPublishFlowToggle;
        private TextField _clientNameInputFIeld;

        private VisualElement _upgradeWarningContainer;

        [MenuItem("Window/VRChat Content Manager Connect/Settings", priority = 2000)]
        [MenuItem("Tools/VRChat Content Manager Connect/Settings")]
        public static void ShowSettings() {
            var window = GetWindow<ContentManagerSettingsWindow>();
            window.titleContent = new GUIContent("Connect Settings");
        }

        public void CreateGUI() {
            var root = rootVisualElement;
            var content = m_VisualTreeAsset.Instantiate();

            root.Add(content);

            _noRequiredPackageInstalledContainer = content.Q<VisualElement>("no-required-packages-installed-container");

            _disconnectedStateContainer = content.Q<VisualElement>("disconnected-state-container");
            _rpcHostInputField = content.Q<TextField>("host-inputfield");
            _requestChallengeButton = content.Q<Button>("request-challenge-button");
            _tryRestoreLastSessionButton = content.Q<Button>("try-restore-session-button");

            _awaitingChallengeStateContainer = content.Q<VisualElement>("challenge-state-container");
            _identityPromptLabel = content.Q<Label>("identity-prompt-label");
            _challengeCodeInputField = content.Q<TextField>("code-inputfield");
            _challengeButton = content.Q<Button>("challenge-button");
            _cancelChallengeButton = content.Q<Button>("cancel-challenge-button");

            _connectedStateContainer = content.Q<VisualElement>("connected-state-container");
            _disconnectButton = content.Q<Button>("disconnect-button");

            _stateDisplayLabel = content.Q<Label>("state-display-label");
            _clientIdLabel = content.Q<Label>("client-id-label");

            _enableContentManagerPublishFlowToggle = content.Q<Toggle>("enable-content-manager-toggle");
            _clientNameInputFIeld = content.Q<TextField>("client-name-inputfield");

            _upgradeWarningContainer = content.Q<VisualElement>("upgrade-warning-container");

        #if !VCCM_AVATAR_SDK_EXIST || VCCM_AVATAR_SDK_3_9_0_OR_NEWER
            _upgradeWarningContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        #else
            _upgradeWarningContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        #endif

        #if VCCM_AVATARS_PACKAGE_EXIST || VCCM_WORLDS_PACKAGE_EXIST
            _noRequiredPackageInstalledContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        #else
            _noRequiredPackageInstalledContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        #endif

            if (ConnectEditorApp.Instance is not { } app)
                return;

            var rpcClientService = app.ServiceProvider.GetRequiredService<RpcClientService>();
            var rpcClientIdProvider = app.ServiceProvider.GetRequiredService<IRpcClientIdProvider>();

            EditorApplication.update += () => UpdateConnectionState(rpcClientService);
            UpdateConnectionState(rpcClientService);

            rpcClientService.IdentityPromptChanged += (_, prompt) => { _identityPromptLabel.text = prompt; };
            _identityPromptLabel.text = rpcClientService.GetIdentityPrompt() ?? "";

            _clientIdLabel.text = rpcClientService.GetClientId();

            _requestChallengeButton.clicked += async () => {
                await rpcClientService.RequestChallengeAsync(_rpcHostInputField.value);
            };

            _tryRestoreLastSessionButton.clicked += async () => {
                try {
                    await rpcClientService.RestoreSessionAsync();
                }
                catch (Exception ex) {
                    Debug.LogError("Failed to restore last session: " + ex.Message);
                    Debug.LogException(ex);

                    EditorUtility.DisplayDialog("Failed to Restore Session",
                        "Could not restore the last session. Please try connecting again.\n\nError: " + ex,
                        "Ok");
                }
            };

            _challengeButton.clicked += async () => {
                await rpcClientService.CompleteChallengeAsync(_challengeCodeInputField.value);
            };

            _disconnectButton.clicked += async () => { await rpcClientService.DisconnectAsync(); };
            _cancelChallengeButton.clicked += async () => { await rpcClientService.DisconnectAsync(); };

            var settings = app.ServiceProvider.GetRequiredService<AppSettingsService>();

            _enableContentManagerPublishFlowToggle.value = settings.GetSettings().UseContentManager;
            _enableContentManagerPublishFlowToggle.RegisterValueChangedCallback(args => {
                settings.GetSettings().UseContentManager = args.newValue;
                settings.SaveSettings();
            });

            _clientNameInputFIeld.value = rpcClientIdProvider.GetClientName();
            _clientNameInputFIeld.RegisterValueChangedCallback(args => {
                rpcClientIdProvider.SetClientName(args.newValue);
            });
        }

        private void UpdateConnectionState(RpcClientService rpcClientService) {
            var state = rpcClientService.State;
            _stateDisplayLabel.text = state.ToString();

            if (rpcClientService.InstanceName is not null)
                _stateDisplayLabel.text += " - " + rpcClientService.InstanceName;

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
}