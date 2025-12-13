using HarmonyLib;
using UnityEditor;
using VRChatContentManagerConnect.Editor.Services.Rpc;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;

namespace VRChatContentManagerConnect.Avatars.Editor.ContinuousAvatarUploader.Patch;

[HarmonyPatch]
internal class ConnectionStatusPatch : YesPatchBase {
    public override string Id => "xyz.misakal.vpm.vcm-connect.avatars.continuous-avatar-uploader-ext.connection-status";
    public override string DisplayName => "Show Connection Status";

    public override string Description =>
        "Display the RPC client connection status in the Continuous Avatar Uploader UI.";

    public override string Category => CauPatchConst.Category;

    public override bool IsDefaultEnabled => true;

    private readonly Harmony _harmony =
        new("xyz.misakal.vpm.vcm-connect.avatars.continuous-avatar-uploader-ext.connection-status");

    public override void Patch() {
        _harmony.PatchAll(typeof(ConnectionStatusPatch));
    }

    public override void UnPatch() {
        _harmony.UnpatchSelf();
    }

    [HarmonyPatch(typeof(Anatawa12.ContinuousAvatarUploader.Editor.ContinuousAvatarUploader), "OnGUI")]
    [HarmonyPrefix]
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