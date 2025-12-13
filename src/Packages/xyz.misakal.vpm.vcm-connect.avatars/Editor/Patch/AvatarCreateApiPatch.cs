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
    internal class AvatarCreateApiPatch : YesPatchBase {
        public override string Id => "xyz.misakal.vpm.vcm-connect.avatars.redirect-create-new-avatar";
        public override string DisplayName => "Redirect Avatar Creation to Content Manager";

        public override string Description =>
            "Redirects avatar creation requests to VRChat Content Manager when enabled in settings.";

        public override string Category => PatchConst.Category;

        public override bool IsDefaultEnabled => true;

        private readonly Harmony _harmony = new("xyz.misakal.vpm.vcm-connect.avatars.redirect-create-new-avatar");

        public override void Patch() {
            _harmony.PatchAll(typeof(AvatarCreateApiPatch));
        }

        public override void UnPatch() {
            _harmony.UnpatchSelf();
        }

        // public static async Task<VRCAvatar> CreateNewAvatar(
        // string id, VRCAvatar data, string pathToBundle, string pathToImage,
        // Action<string, float> onProgress = null, CancellationToken cancellationToken = default)
        [HarmonyPatch(typeof(VRCApi), nameof(VRCApi.CreateNewAvatar),
            typeof(string), typeof(VRCAvatar), typeof(string), typeof(string)
            , typeof(Action<string, float>), typeof(CancellationToken))]
        [HarmonyPrefix]
        internal static bool VrcApiCreateNewAvatarPrefix(ref Task<VRCAvatar> __result,
            string id, VRCAvatar data, string pathToBundle, string pathToImage,
            Action<string, float> onProgress = null,
            CancellationToken cancellationToken = default
        ) {
        #if VCCM_AVATAR_SDK_3_9_0_OR_NEWER
            var app = ConnectEditorApp.Instance;
            if (app == null) {
                Debug.LogWarning(
                    "[VRCCM.Connect] AvatarCreateApiPatch: ConnectEditorApp instance is null. Skipping patch.");
                return true;
            }

            var settings = app.ServiceProvider.GetRequiredService<AppSettingsService>();
            if (!settings.GetSettings().UseContentManager)
                return true;

            __result = Task.Run(async () => {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException(nameof(id), "Avatar ID cannot be null or empty.");

                if (string.IsNullOrWhiteSpace(pathToBundle) || string.IsNullOrWhiteSpace(pathToImage)) {
                    throw new ArgumentException("Path to bundle or image cannot be null or empty.");
                }

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                // Send Create Avatar Request to App, Start new task

                var rpcClient = app.ServiceProvider.GetRequiredService<RpcClientService>();
                if (rpcClient.State == RpcClientState.Disconnected) {
                    Debug.Log(
                        "[VRCCM.Connect] RPC client is disconnected. Attempting to restore session...");

                    try {
                        await rpcClient.RestoreSessionAsync();
                    }
                    catch (Exception ex) {
                        Debug.LogError(
                            $"[VRCCM.Connect] Failed to restore RPC session: {ex.Message}");
                        throw;
                    }
                }

                var bundleFileName = "Avatar - " + data.Name + " - Asset bundle - " + Application.unityVersion + "_" +
                                     ApiAvatar.VERSION.ApiVersion +
                                     "_" + Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl();
                var imageFileName = "Avatar - " + data.Name + " - Thumbnail - " + Application.unityVersion + "_" +
                                    ApiAvatar.VERSION.ApiVersion +
                                    "_" + Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl() +
                                    Path.GetExtension(pathToImage);

                Debug.Log($"AvatarId: {id} PathToBundle: {pathToBundle} BundleFileName: {bundleFileName}");

                var fileId = await rpcClient.UploadFileAsync(pathToBundle, bundleFileName);
                Debug.Log("Bundle File Id: " + fileId);
                var imageFileId = await rpcClient.UploadFileAsync(pathToImage, imageFileName);
                Debug.Log("Image File Id: " + imageFileId);

                await rpcClient.CreateAvatarPublishTaskAsync(
                    id,
                    fileId,
                    data.Name,
                    Tools.Platform,
                    Tools.UnityVersion.ToString(),
                    imageFileId,
                    data.Description,
                    data.Tags.ToArray(),
                    data.ReleaseStatus
                );
                return data;
            }, cancellationToken);

            return false;
        #else
            return true;
        #endif
        }
    }
}