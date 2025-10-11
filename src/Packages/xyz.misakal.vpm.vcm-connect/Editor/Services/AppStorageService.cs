using System;
using System.IO;

namespace VRChatContentManagerConnect.Editor.Services;

internal static class AppStorageService {
    public const string AppDataFolderName = "vrchat-content-manager-connect-81b7bca3";

    public static string GetStoragePath()
    {
        var osAppDataPath = GetOsAppDataPath();
        var appDataPath = Path.Combine(osAppDataPath, AppDataFolderName);
        if (!Directory.Exists(appDataPath))
            Directory.CreateDirectory(appDataPath);

        return appDataPath;
    }

    public static string GetTempPath()
    {
        var tempPath = Path.Combine(GetStoragePath(), "temp");
        if (!Directory.Exists(tempPath))
            Directory.CreateDirectory(tempPath);
        
        return tempPath;
    }
    
    private static string GetOsAppDataPath()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            // usually LOCALAPPDATA (C:\Users\{User}\AppData\Local)
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (Environment.OSVersion.Platform == PlatformID.Unix)
            // usually XDG_CONFIG_HOME (HOME/.config)
            // ReSharper disable once DuplicatedStatements
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }
}