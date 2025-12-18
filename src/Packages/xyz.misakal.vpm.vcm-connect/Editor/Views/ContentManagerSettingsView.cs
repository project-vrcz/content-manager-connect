using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRChatContentManagerConnect.Editor.Models;
using VRChatContentManagerConnect.Editor.Services;
using VRChatContentManagerConnect.Editor.Services.Rpc;
using VRChatContentManagerConnect.Editor.Views.Pages.Connected;
using VRChatContentManagerConnect.Editor.Views.Pages.NewConnection;
using VRChatContentManagerConnect.Editor.Views.Pages.Reconnect;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace VRChatContentManagerConnect.Editor.Views;

internal sealed class ContentManagerSettingsView : VisualElement {
    private const string VisualTreeAssetGuid = "56cbde71eccc3d24d85b8e0020fefc67";

    private readonly VisualElement _pageContainer;

    private readonly VisualElement _noRequiredPackageInstalledContainer;

    private readonly Label _clientIdLabel;

    private readonly Toggle _enableLaunchAppWhenStartupToggle;
    private readonly Toggle _enableLaunchAppWhenReconnectToggle;

    private readonly Toggle _enableContentManagerPublishFlowToggle;
    private readonly TextField _clientNameInputField;

    private readonly VisualElement _upgradeWarningContainer;

    private VisualElement? _currentPage;

    private readonly AppSettingsService _appSettingsService;
    private readonly RpcClientService _rpcClientService;
    private readonly IRpcClientIdProvider _rpcClientIdProvider;
    private readonly YesLogger _logger = new(LoggerConst.LoggerPrefix + nameof(ContentManagerSettingsView));

    private readonly AppSettings _appSettings;

    public ContentManagerSettingsView() {
        var path = AssetDatabase.GUIDToAssetPath(VisualTreeAssetGuid);
        var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);

        visualTreeAsset.CloneTree(this);

        _pageContainer = this.Q<VisualElement>("page-container");

        _noRequiredPackageInstalledContainer = this.Q<VisualElement>("no-required-packages-installed-container");

        _clientIdLabel = this.Q<Label>("client-id-label");

        _enableLaunchAppWhenReconnectToggle = this.Q<Toggle>("enable-auto-start-app-when-reconnect-toggle");
        _enableLaunchAppWhenStartupToggle = this.Q<Toggle>("enable-auto-start-app-when-launch-toggle");

        _enableContentManagerPublishFlowToggle = this.Q<Toggle>("enable-content-manager-toggle");
        _clientNameInputField = this.Q<TextField>("client-name-inputfield");

        _upgradeWarningContainer = this.Q<VisualElement>("upgrade-warning-container");

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

        if (ConnectEditorApp.Instance is not { } app) {
            _logger.LogError("VRChat Content Manager Connect App is not initialized.");
            throw new InvalidOperationException("VRChat Content Manager Connect App is not initialized.");
        }

        _rpcClientService = app.ServiceProvider.GetRequiredService<RpcClientService>();
        _rpcClientIdProvider = app.ServiceProvider.GetRequiredService<IRpcClientIdProvider>();

        _clientIdLabel.text = _rpcClientService.GetClientId();

        _appSettingsService = app.ServiceProvider.GetRequiredService<AppSettingsService>();
        _appSettings = _appSettingsService.GetSettings();

        _enableLaunchAppWhenStartupToggle.value = _appSettings.LaunchAppWhenStartup;
        _enableLaunchAppWhenReconnectToggle.value = _appSettings.LaunchAppWhenReconnect;

        _enableLaunchAppWhenStartupToggle.RegisterValueChangedCallback(args => {
            _appSettings.LaunchAppWhenStartup = args.newValue;
            _appSettingsService.SaveSettings();
        });

        _enableLaunchAppWhenReconnectToggle.RegisterValueChangedCallback(args => {
            _appSettings.LaunchAppWhenReconnect = args.newValue;
            _appSettingsService.SaveSettings();
        });

        _enableContentManagerPublishFlowToggle.value = _appSettings.UseContentManager;
        _enableContentManagerPublishFlowToggle.RegisterValueChangedCallback(args => {
            _appSettings.UseContentManager = args.newValue;
            _appSettingsService.SaveSettings();
        });

        _clientNameInputField.value = _rpcClientIdProvider.GetClientName();
        _clientNameInputField.RegisterValueChangedCallback(args => {
            _rpcClientIdProvider.SetClientName(args.newValue);
        });

        RegisterCallback<AttachToPanelEvent>(_ => {
            EditorApplication.update += UpdateSettings;
            _rpcClientService.StateChanged += RpcClientServiceOnStateChanged;
        });

        RegisterCallback<DetachFromPanelEvent>(_ => {
            EditorApplication.update -= UpdateSettings;
            _rpcClientService.StateChanged -= RpcClientServiceOnStateChanged;
        });

        UpdateConnectionState();
    }

    private void UpdateSettings() {
        _enableContentManagerPublishFlowToggle.value = _appSettings.UseContentManager;
    }

    private void UpdateConnectionState() {
        var state = _rpcClientService.State;

        switch (state) {
            case RpcClientState.Connected:
                if (_currentPage is ConnectedPage)
                    break;

                UpdateCurrentPage(new ConnectedPage());
                break;
            case RpcClientState.AwaitingChallenge:
                if (_currentPage is NewConnectionPage)
                    break;

                UpdateCurrentPage(new NewConnectionPage());
                break;
            case RpcClientState.Disconnected:
            default:
                _ = Task.Run(async () => {
                    try {
                        var canRestoreSession = await _rpcClientService.GetLastSessionInfoAsync() is not null;
                        MainThreadDispatcher.Dispatch(() => {
                            if (canRestoreSession) {
                                if (_currentPage is ReconnectPage)
                                    return;

                                UpdateCurrentPage(new ReconnectPage(() =>
                                    UpdateCurrentPage(new NewConnectionPage())));
                                return;
                            }

                            if (_currentPage is NewConnectionPage)
                                return;

                            UpdateCurrentPage(new NewConnectionPage());
                        });
                    }
                    catch (Exception ex) {
                        _logger.LogError(ex, "Failed to check if session can be restored.");

                        MainThreadDispatcher.Dispatch(() => {
                            if (_currentPage is NewConnectionPage)
                                return;

                            UpdateCurrentPage(new NewConnectionPage());
                        });
                    }
                });
                break;
        }
    }

    private void UpdateCurrentPage(VisualElement currentPage) {
        _currentPage = currentPage;
        _pageContainer.Clear();
        _pageContainer.Add(currentPage);
    }

    private void RpcClientServiceOnStateChanged(object sender, RpcClientState e) {
        MainThreadDispatcher.Dispatch(UpdateConnectionState);
    }
}