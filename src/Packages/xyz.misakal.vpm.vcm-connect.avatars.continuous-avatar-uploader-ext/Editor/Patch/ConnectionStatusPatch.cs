using HarmonyLib;
using UnityEditor;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Avatars.Editor.ContinuousAvatarUploader.Patch;

[HarmonyPatch(typeof(Anatawa12.ContinuousAvatarUploader.Editor.ContinuousAvatarUploader), "OnGUI")]
internal static class ConnectionStatusPatch {
    public static void Prefix() {
        if (RpcClientServiceInstance.TryGetRpcClientService() is not { } rpcClientService ||
            AppSettingsServiceInstance.TryGetAppSettingsService() is not { } appSettingsService) {
            EditorGUILayout.HelpBox("VRChat Content Manager Connect is not initialized yet.", MessageType.Error);
            return;
        }

        var messageType = rpcClientService.State switch {
            RpcClientState.Disconnected => MessageType.Error,
            RpcClientState.AwaitingChallenge => MessageType.Warning,
            RpcClientState.Connected => MessageType.Info,
            _ => MessageType.None
        };

        EditorGUILayout.HelpBox("RPC Client Status: " + rpcClientService.State, messageType);

        if (!appSettingsService.GetSettings().UseContentManager)
            EditorGUILayout.HelpBox("VRChat Content Manager Connect is disabled in the settings.", MessageType.Warning);
    }
}