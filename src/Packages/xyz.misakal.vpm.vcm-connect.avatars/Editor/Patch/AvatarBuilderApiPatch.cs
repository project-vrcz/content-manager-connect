using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;
using VRC.SDK3A.Editor;
using VRC.SDKBase.Editor.Api;
using VRChatContentManagerConnect.Editor;
using VRChatContentManagerConnect.Editor.Services;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Avatars.Editor.Patch {
    // public async Task BuildAndUpload(
    // GameObject target, List<PerPlatformOverrides.Option> overrides,
    // VRCAvatar avatar, string thumbnailPath = null,
    // CancellationToken cancellationToken = default)
#if VCCM_AVATAR_SDK_3_7_6_OR_NEWER
    [HarmonyPatch(typeof(VRCSdkControlPanelAvatarBuilder), nameof(VRCSdkControlPanelAvatarBuilder.BuildAndUpload),
        typeof(GameObject), typeof(List<PerPlatformOverrides.Option>),
        typeof(VRCAvatar), typeof(string),
        typeof(CancellationToken))]
#else
    // public async Task BuildAndUpload(
    // GameObject target, VRCAvatar avatar,
    // string thumbnailPath = null, CancellationToken cancellationToken = default)
    [HarmonyPatch(typeof(VRCSdkControlPanelAvatarBuilder), nameof(VRCSdkControlPanelAvatarBuilder.BuildAndUpload),
        typeof(GameObject), typeof(VRCAvatar),
        typeof(string), typeof(CancellationToken))]
#endif
    internal static class AvatarBuilderApiPatch {
        public static bool Prefix(ref Task __result) {
            if (ConnectEditorApp.Instance is not { } app) {
                Debug.LogError("Failed to Build and Upload: VRChat Content Manager Connect is not initialized.");
                __result = Task.FromException(
                    new InvalidOperationException("VRChat Content Manager Connect is not initialized."));
                return false;
            }

            var settings = app.ServiceProvider.GetRequiredService<AppSettingsService>();
            if (!settings.GetSettings().UseContentManager)
                return false;

            var rpcClient = app.ServiceProvider.GetRequiredService<RpcClientService>();
            if (rpcClient.State != RpcClientState.Connected) {
                Debug.LogError("Failed to Build and Upload: RPC Client is not connected.");
                __result = Task.FromException(new InvalidOperationException("RPC Client is not connected."));
                return false;
            }

            return true;
        }
    }
}