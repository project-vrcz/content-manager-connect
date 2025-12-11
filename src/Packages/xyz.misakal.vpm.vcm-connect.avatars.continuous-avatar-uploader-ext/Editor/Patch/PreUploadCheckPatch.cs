using System.Reflection;
using HarmonyLib;
using UnityEditor;
using VRChatContentManagerConnect.Editor;
using Uploader = Anatawa12.ContinuousAvatarUploader.Editor.ContinuousAvatarUploader;

namespace VRChatContentManagerConnect.Avatars.Editor.ContinuousAvatarUploader.Patch;

[HarmonyPatch(typeof(Uploader), "DoStartUpload")]
internal static class PreUploadCheckPatch {
    private static MethodInfo? DoStartUploadMethod;

    private static bool _isCheckSucceed;

    private static bool Prefix(Uploader? __instance) {
        if (PreUploadCheck.IsTaskRunning)
            return false;

        if (_isCheckSucceed) {
            _isCheckSucceed = false;
            return true;
        }

        PreUploadCheck.RunPreUploadCheck(() => {
                DoStartUploadMethod ??=
                    AccessTools.Method(typeof(Uploader),
                        "DoStartUpload");
                if (DoStartUploadMethod is null) {
                    EditorUtility.DisplayDialog(
                        "Failed to Start Upload",
                        "Auto reconnect is succeed, but we can start the upload for you.\n" +
                        "Click the upload button again may solve your problem.\n\n" +
                        "Report this issue to the developer: ContinuousAvatarUploader.DoStartUpload MethodInfo is null.",
                        "OK");
                    return;
                }

                if (__instance is null) {
                    EditorUtility.DisplayDialog(
                        "Failed to Start Upload",
                        "Auto reconnect is succeed, but we can start the upload for you.\n" +
                        "Click the upload button again may solve your problem.\n\n" +
                        "Report this issue to the developer: ContinuousAvatarUploader instance is null.",
                        "OK");
                    return;
                }

                _isCheckSucceed = true;
                DoStartUploadMethod.Invoke(__instance, null);
            },
            () => _isCheckSucceed = false
        );

        return false;
    }
}