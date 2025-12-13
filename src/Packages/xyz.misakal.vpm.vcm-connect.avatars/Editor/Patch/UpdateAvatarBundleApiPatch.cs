using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDKBase.Editor.Api;
using VRChatContentManagerConnect.Editor;
using VRChatContentManagerConnect.Editor.Services;
using VRChatContentManagerConnect.Editor.Services.Rpc;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;

namespace VRChatContentManagerConnect.Avatars.Editor.Patch {
    [HarmonyPatch]
    internal class UpdateAvatarBundleApiPatch : YesPatchBase {
        public override string Id => "xyz.misakal.vpm.vcm-connect.avatars.redirect-update-avatar-bundle";
        public override string DisplayName => "Redirect Avatar Update to Content Manager";

        public override string Description =>
            "Redirects avatar bundle update requests to VRChat Content Manager when enabled in settings.";

        public override string Category => PatchConst.Category;

        public override bool IsDefaultEnabled => true;

        private readonly Harmony _harmony = new("xyz.misakal.vpm.vcm-connect.avatars.redirect-update-avatar-bundle");

        public override void Patch() {
            _harmony.PatchAll(typeof(UpdateAvatarBundleApiPatch));
        }

        public override void UnPatch() {
            _harmony.UnpatchSelf();
        }

        // public static async Task<VRCAvatar> UpdateAvatarBundle(
        // string id, VRCAvatar data, string pathToBundle,
        // Action<string, float> onProgress = null, CancellationToken cancellationToken = default)
        [HarmonyPatch(typeof(VRCApi), nameof(VRCApi.UpdateAvatarBundle),
            typeof(string), typeof(VRCAvatar), typeof(string), typeof(Action<string, float>),
            typeof(CancellationToken))]
        [HarmonyPrefix]
        internal static bool UploadAvatarBundlePrefix(
            ref Task<VRCAvatar> __result,
            string id, VRCAvatar data, string pathToBundle,
            Action<string, float> onProgress = null,
            CancellationToken cancellationToken = default
        ) {
            var app = ConnectEditorApp.Instance;
            if (app == null) {
                Debug.LogWarning(
                    "[VRCCM.Connect] AvatarBundleUploadApiPatch: ConnectEditorApp instance is null. Skipping patch.");
                return true;
            }

            var settings = app.ServiceProvider.GetRequiredService<AppSettingsService>();
            if (!settings.GetSettings().UseContentManager)
                return true;

            __result = Task.Run(async () => {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException(nameof(id), "Avatar ID cannot be null or empty.");

                if (string.IsNullOrEmpty(pathToBundle))
                    throw new ArgumentNullException(nameof(pathToBundle), "Path to bundle cannot be null or empty.");

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                if (!File.Exists(pathToBundle))
                    throw new FileNotFoundException("The specified bundle file does not exist.", pathToBundle);

                var bundleFileName = "Avatar - " + data.Name + " - Asset bundle - " + Application.unityVersion + "_" +
                                     ApiAvatar.VERSION.ApiVersion +
                                     "_" + Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl();

                Debug.Log($"AvatarId: {id} PathToBundle: {pathToBundle} BundleFileName: {bundleFileName}");
                // Send Bundle File to App, Start new task

                var rpcClient = app.ServiceProvider.GetRequiredService<RpcClientService>();
                if (rpcClient.State == RpcClientState.Disconnected) {
                    Debug.Log("[VRCCM.Connect] RPC client is disconnected. Attempting to restore session...");

                    try {
                        await rpcClient.RestoreSessionAsync();
                        Debug.Log("[VRCCM.Connect] RPC session restored successfully.");
                    }
                    catch (Exception ex) {
                        Debug.LogException(ex);
                        Debug.LogError("[VRCCM.Connect] Failed to restore RPC session. Aborting avatar bundle upload.");

                        throw new InvalidOperationException("RPC client is not connected.", ex);
                    }
                }

                var fileId = await rpcClient.UploadFileAsync(pathToBundle, bundleFileName);
                Debug.Log("Bundle File Id: " + fileId);

                await rpcClient.CreateAvatarPublishTaskAsync(id, fileId, data.Name, Tools.Platform,
                    Tools.UnityVersion.ToString());

                return data;
            });

            // replace method
            return false;
        }
    }
}