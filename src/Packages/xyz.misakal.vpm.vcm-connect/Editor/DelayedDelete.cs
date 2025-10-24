using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VRChatContentManagerConnect.Editor;

internal static class DelayedDelete {
    private static readonly Dictionary<string, int> _fileDeleteAttempts = new();

    private const int MaxDeleteAttempts = 5;

    public static void Delete(string path) {
        RunDeleteAttempts();
        
        if (!File.Exists(path)) {
            throw new FileNotFoundException(path);
        }

        Debug.Log("[VRCCM.Connect] DelayedDelete: new file to delete: " + path);
        try {
            File.Delete(path);
            Debug.Log("[VRCCM.Connect] DelayedDelete: File deleted: " + path);
        }
        catch (Exception ex) {
            Debug.LogException(ex);
            Debug.LogWarning("[VRCCM.Connect] DelayedDelete: Failed to delete file, push to delayed delete: " + path);
            
            _fileDeleteAttempts[path] = 1;
        }
    }
    
    [MenuItem("Tools/VRChat Content Manager Connect/Delayed Delete/List Delayed Delete Attempts")]
    public static void ListDeleteAttempts() {
        if (_fileDeleteAttempts.Count == 0) {
            Debug.Log("[VRCCM.Connect] DelayedDelete: No files to delete.");
            return;
        }
        
        var stringBuilder = new StringBuilder();
        
        stringBuilder.AppendLine("[VRCCM.Connect] DelayedDelete: Files pending deletion:");
        foreach (var kvp in _fileDeleteAttempts) {
            stringBuilder.AppendLine($"  {kvp.Key} - Attempts: {kvp.Value}");
        }
        
        Debug.Log(stringBuilder.ToString());
    }

    [MenuItem("Tools/VRChat Content Manager Connect/Delayed Delete/Run Delayed Delete Attempts")]
    public static void RunDeleteAttempts() {
        var filesToDelete = _fileDeleteAttempts.Keys.ToList();

        if (filesToDelete.Count == 0) {
            Debug.Log("[VRCCM.Connect] DelayedDelete: No files to delete.");
            return;
        }

        foreach (var filePath in filesToDelete) {
            try {
                Debug.Log("[VRCCM.Connect] DelayedDelete: Try Deleting file: " + filePath);
                if (File.Exists(filePath)) {
                    File.Delete(filePath);
                    Debug.Log("[VRCCM.Connect] DelayedDelete: File deleted: " + filePath);
                    _fileDeleteAttempts.Remove(filePath);
                }
                else {
                    Debug.LogWarning("[VRCCM.Connect] DelayedDelete: File not found: " + filePath);
                    _fileDeleteAttempts.Remove(filePath);
                }
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                Debug.LogWarning("[VRCCM.Connect] DelayedDelete: Failed to delete file: " + filePath);
                _fileDeleteAttempts[filePath]++;
                if (_fileDeleteAttempts[filePath] >= MaxDeleteAttempts) {
                    Debug.LogWarning("[VRCCM.Connect] DelayedDelete: Max delete attempts reached for file: " +
                                     filePath);
                    _fileDeleteAttempts.Remove(filePath);
                }
            }
        }
    }
}