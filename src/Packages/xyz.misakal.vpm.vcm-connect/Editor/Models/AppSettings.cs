namespace VRChatContentPublisherConnect.Editor.Models;

internal sealed class AppSettings {
    public bool UseContentManager { get; set; } = true;

    public bool LaunchAppWhenStartup { get; set; }
    public bool LaunchAppWhenReconnect { get; set; }
}