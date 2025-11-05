using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                TryStopContinuousAvatarUploader();
                return false;
            }

            var settings = app.ServiceProvider.GetRequiredService<AppSettingsService>();
            if (!settings.GetSettings().UseContentManager)
                return true;

            var rpcClient = app.ServiceProvider.GetRequiredService<RpcClientService>();
            if (rpcClient.State != RpcClientState.Connected) {
                Debug.LogError("Failed to Build and Upload: RPC Client is not connected.");
                __result = Task.FromException(new InvalidOperationException("RPC Client is not connected."));
                TryStopContinuousAvatarUploader();
                return false;
            }

            return true;
        }

        private static void TryStopContinuousAvatarUploader() {
        #if VCCM_SUPPORTED_CAU_VERSION
            var cauAssembly = AccessTools.AllAssemblies()
                .FirstOrDefault(assembly =>
                    assembly.GetName().Name == "com.anatawa12.continuous-avatar-uploader.editor");

            if (cauAssembly is null) {
                Debug.LogWarning("[VRCCM.Connect] CAU assembly not found.");
                return;
            }

            var uploadOrchestratorType =
                cauAssembly.GetType("Anatawa12.ContinuousAvatarUploader.Editor.UploadOrchestrator");
            if (uploadOrchestratorType is null) {
                Debug.LogWarning("[VRCCM.Connect] CAU UploadOrchestrator type not found.");
                return;
            }

            var isUploadInProgressMethod = uploadOrchestratorType.GetMethod(
                "IsUploadInProgress",
                0,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                Type.EmptyTypes,
                Array.Empty<ParameterModifier>());
            var cancelUploadsMethod = uploadOrchestratorType.GetMethod(
                "CancelUpload",
                0,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                Type.EmptyTypes,
                Array.Empty<ParameterModifier>());

            if (isUploadInProgressMethod is null || isUploadInProgressMethod.ReturnType != typeof(bool)) {
                Debug.LogWarning("[VRCCM.Connect] CAU static bool IsUploadInProgress() method not found.");
                return;
            }

            if (cancelUploadsMethod is null) {
                Debug.LogWarning("[VRCCM.Connect] CAU static CancelUpload() method not found.");
                return;
            }

            var isUploadInProgress = (bool)isUploadInProgressMethod.Invoke(null, Array.Empty<object>());
            if (!isUploadInProgress)
                return;

            Debug.Log("[VRCCM.Connect] Detected ongoing CAU upload. Cancelling CAU upload.");
            cancelUploadsMethod.Invoke(null, Array.Empty<object>());
        #endif
        }
    }
}