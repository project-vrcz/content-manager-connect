using Microsoft.Extensions.DependencyInjection;
using VRChatContentPublisherConnect.Editor;
using VRChatContentPublisherConnect.Editor.Services;

namespace VRChatContentPublisherConnect.Avatars.Editor.ContinuousAvatarUploader;

internal static class AppSettingsServiceInstance {
    private static AppSettingsService? _appSettingsService;
    
    public static AppSettingsService? TryGetAppSettingsService() {
        if (_appSettingsService != null)
            return _appSettingsService;

        if (ConnectEditorApp.Instance is not { } app) {
            return null;
        }

        _appSettingsService = app.ServiceProvider.GetRequiredService<AppSettingsService>();
        return _appSettingsService;
    }
}