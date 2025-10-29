using System;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using VRC.SDK3.Editor;
using VRC.SDKBase.Editor.Api;
using VRChatContentManagerConnect.Editor;
using VRChatContentManagerConnect.Editor.Services;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Worlds.Editor.Patch {
    // public async Task BuildAndUpload(VRCWorld world, string signature, string thumbnailPath = null,
    // CancellationToken cancellationToken = default)
#if VCCM_WORLD_SDK_3_7_2_OR_NEWER
    [HarmonyPatch(typeof(VRCSdkControlPanelWorldBuilder), nameof(VRCSdkControlPanelWorldBuilder.BuildAndUpload),
        typeof(VRCWorld), typeof(string), typeof(string), typeof(CancellationToken))]
#else
    [HarmonyPatch(typeof(VRCSdkControlPanelWorldBuilder), nameof(VRCSdkControlPanelWorldBuilder.BuildAndUpload),
        typeof(VRCWorld), typeof(string), typeof(CancellationToken))]
#endif
    internal static class WorldBuilderApiPatch {
        public static bool Prefix(ref Task __result) {
            if (ConnectEditorApp.Instance is not { } app) {
                __result = Task.FromException(
                    new InvalidOperationException("VRChat Content Manager Connect is not initialized."));
                return false;
            }

            var settings = app.ServiceProvider.GetRequiredService<AppSettingsService>();
            if (!settings.GetSettings().UseContentManager)
                return false;

            var rpcClient = app.ServiceProvider.GetRequiredService<RpcClientService>();
            if (rpcClient.State != RpcClientState.Connected) {
                __result = Task.FromException(new InvalidOperationException("RPC Client is not connected."));
                return false;
            }

            return true;
        }
    }
}