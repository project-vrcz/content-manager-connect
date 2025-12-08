using HarmonyLib;
using UnityEditor;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Avatars.Editor.ContinuousAvatarUploader.Patch;

[HarmonyPatch(typeof(Anatawa12.ContinuousAvatarUploader.Editor.ContinuousAvatarUploader), "DoStartUpload")]
internal static class PreUploadCheckPatch {
    private static bool Prefix() {
        if (RpcClientServiceInstance.TryGetRpcClientService() is not { } rpcClientService ||
            AppSettingsServiceInstance.TryGetAppSettingsService() is not { } appSettingsService) {
            EditorUtility.DisplayDialog(
                "Failed to Start Upload",
                "VRChat Content Manager Connect is not initialized.",
                "OK");
            return false;
        }

        if (!appSettingsService.GetSettings().UseContentManager)
            return true;

        if (rpcClientService.State != RpcClientState.Connected) {
            EditorUtility.DisplayDialog(
                "Failed to Start Upload",
                "RPC Client is not connected.",
                "OK");
            return false;
        }

        return true;
    }
}