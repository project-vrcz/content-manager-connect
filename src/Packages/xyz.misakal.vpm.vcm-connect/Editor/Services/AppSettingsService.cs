using System.IO;
using System.Text.Json;
using VRChatContentManagerConnect.Editor.Models;

namespace VRChatContentManagerConnect.Editor.Services;

internal sealed class AppSettingsService {
    private AppSettings? _settings;

    public AppSettings GetSettings() {
        if (_settings is not null)
            return _settings;

        var settingsPath = GetSettingsPath();
        if (File.Exists(settingsPath)) {
            var json = File.ReadAllText(settingsPath);
            _settings = JsonSerializer.Deserialize<AppSettings>(json);

            if (_settings is not null)
                return _settings;
        }

        SaveSettings();
        return _settings!;
    }

    public void SaveSettings() {
        _settings ??= new AppSettings();

        var settingsPath = GetSettingsPath();
        var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions {
            WriteIndented = true
        });

        File.WriteAllText(settingsPath, json);
    }

    private static string GetSettingsPath() {
        return Path.Combine(AppStorageService.GetStoragePath(), "settings.json");
    }
}