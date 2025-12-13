using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using VRChatContentManagerConnect.Editor.MenuItems;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace VRChatContentManagerConnect.Editor;

internal static class DelayedDelete {
    private static readonly YesLogger Logger = new(LoggerConst.LoggerPrefix + nameof(DelayedDelete));

    private static readonly Dictionary<string, int> _fileDeleteAttempts = new();

    private const int MaxDeleteAttempts = 5;

    public static void Delete(string path) {
        RunDeleteAttempts();

        if (!File.Exists(path)) {
            Logger.LogWarning("File not found, skipping delete: " + path);
            return;
        }

        Logger.LogDebug("new file to delete: " + path);
        try {
            File.Delete(path);
            Logger.LogInfo("File deleted: " + path);
        }
        catch (Exception ex) {
            Logger.LogWarning(ex, "Failed to delete file, push to delayed delete: " + path);

            _fileDeleteAttempts[path] = 1;
        }
    }

    [MenuItem(MenuItemPath.RootMenuItemPath + "Delayed Delete/List Delayed Delete Attempts")]
    public static void ListDeleteAttempts() {
        if (_fileDeleteAttempts.Count == 0) {
            Logger.LogInfo("No files to delete.");
            return;
        }

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("Files pending deletion:");
        foreach (var kvp in _fileDeleteAttempts) {
            stringBuilder.AppendLine($"  {kvp.Key} - Attempts: {kvp.Value}");
        }

        Logger.LogInfo(stringBuilder.ToString());
    }

    [MenuItem(MenuItemPath.RootMenuItemPath + "Delayed Delete/Run Delayed Delete Attempts")]
    public static void RunDeleteAttempts() {
        var filesToDelete = _fileDeleteAttempts.Keys.ToList();

        if (filesToDelete.Count == 0) {
            Logger.LogInfo("No files to delete.");
            return;
        }

        foreach (var filePath in filesToDelete) {
            try {
                Logger.LogDebug("Try Deleting file: " + filePath);
                if (File.Exists(filePath)) {
                    File.Delete(filePath);
                    Logger.LogInfo("File deleted: " + filePath);
                    _fileDeleteAttempts.Remove(filePath);
                }
                else {
                    Logger.LogWarning("File not found: " + filePath);
                    _fileDeleteAttempts.Remove(filePath);
                }
            }
            catch (Exception ex) {
                Logger.LogWarning(ex, "Failed to delete file: " + filePath);
                _fileDeleteAttempts[filePath]++;
                if (_fileDeleteAttempts[filePath] >= MaxDeleteAttempts) {
                    Logger.LogWarning("Max delete attempts reached for file: " + filePath);
                    _fileDeleteAttempts.Remove(filePath);
                }
            }
        }
    }
}