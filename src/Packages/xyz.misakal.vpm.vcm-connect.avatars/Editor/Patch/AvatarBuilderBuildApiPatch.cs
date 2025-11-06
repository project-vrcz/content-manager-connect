using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;
using VRC.SDK3A.Editor;
using VRChatContentManagerConnect.Avatars.Editor.Utils;
using VRChatContentManagerConnect.Editor;
using VRChatContentManagerConnect.Editor.Services;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Avatars.Editor.Patch {
    // public async Task<string> Build(GameObject target)
    [HarmonyPatch(typeof(VRCSdkControlPanelAvatarBuilder), nameof(VRCSdkControlPanelAvatarBuilder.Build),
        typeof(GameObject))]
#if VCCM_AVATAR_SDK_3_7_6_OR_NEWER
    // public async Task<string> Build(GameObject target, List<PerPlatformOverrides.Option> overrides)
    [HarmonyPatch(typeof(VRCSdkControlPanelAvatarBuilder), nameof(VRCSdkControlPanelAvatarBuilder.Build),
        typeof(GameObject), typeof(List<PerPlatformOverrides.Option>))]
#else
    // public async Task BuildAndUpload(
    // GameObject target, VRCAvatar avatar,
    // string thumbnailPath = null, CancellationToken cancellationToken = default)
    [HarmonyPatch(typeof(VRCSdkControlPanelAvatarBuilder), nameof(VRCSdkControlPanelAvatarBuilder.BuildAndUpload),
        typeof(GameObject), typeof(VRCAvatar),
        typeof(string), typeof(CancellationToken))]
#endif
    internal static class AvatarBuilderBuildApiPatch {
        public static bool Prefix(ref Task __result) {
            if (!CauUtils.IsCauUploadInProgress())
                return true;

            if (ConnectEditorApp.Instance is not { } app)
                return true;

            var settings = app.ServiceProvider.GetRequiredService<AppSettingsService>();
            if (!settings.GetSettings().UseContentManager)
                return true;

            var rpcClient = app.ServiceProvider.GetRequiredService<RpcClientService>();
            if (rpcClient.State != RpcClientState.Connected) {
                Debug.LogError("Failed to Build and Upload: RPC Client is not connected.");
                __result = Task.FromException(new InvalidOperationException("RPC Client is not connected."));
                CauUtils.TryStopContinuousAvatarUploader();
                return false;
            }

            return true;
        }
    }
}