using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase.Editor.Api;
using VRChatContentManagerConnect.Editor;
using VRChatContentManagerConnect.Editor.Models.RpcApi.Request.Task;
using VRChatContentManagerConnect.Editor.Services;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Worlds.Editor.Patch {
    // public static async Task<VRCWorld> CreateNewWorld(
    // string id, VRCWorld data, string pathToBundle, string pathToImage, string worldSignature,
    // Action<string, float> onProgress = null, CancellationToken cancellationToken = default)
    [HarmonyPatch(typeof(VRCApi), nameof(VRCApi.CreateNewWorld),
        typeof(string), typeof(VRCWorld), typeof(string), typeof(string), typeof(string),
        typeof(Action<string, float>), typeof(CancellationToken))]
    internal static class WorldCreateApiPatch {
        internal static bool Prefix(ref Task<VRCWorld> __result,
            string id, VRCWorld data, string pathToBundle, string pathToImage, string worldSignature,
            Action<string, float> onProgress = null,
            CancellationToken cancellationToken = default
        ) {
            var app = ConnectEditorApp.Instance;
            if (app == null) {
                UnityEngine.Debug.LogWarning(
                    "[VRCCM.Connect] WorldCreateApiPatch: ConnectEditorApp instance is null. Skipping patch.");
                return true;
            }

            var settings = app.ServiceProvider.GetRequiredService<AppSettingsService>();
            if (!settings.GetSettings().UseContentManager)
                return true;

            __result = Task.Run(async () => {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException(nameof(id), "World ID cannot be null or empty.");

                if (string.IsNullOrWhiteSpace(pathToBundle) || string.IsNullOrWhiteSpace(pathToImage)) {
                    throw new ArgumentException("Path to bundle or image cannot be null or empty.");
                }

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                var currentUserId = APIUser.CurrentUser?.id;
                if (currentUserId is null)
                    throw new InvalidOperationException("Current user is not logged in.");

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

                var bundleFileName = "World - " + data.Name + " - Asset bundle - " + Application.unityVersion + "_" +
                                     ApiAvatar.VERSION.ApiVersion +
                                     "_" + VRC.Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl();
                var imageFileName = "World - " + data.Name + " - Thumbnail - " + Application.unityVersion + "_" +
                                    ApiAvatar.VERSION.ApiVersion +
                                    "_" + VRC.Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl() +
                                    Path.GetExtension(pathToImage);

                Debug.Log($"WorldId: {id} PathToBundle: {pathToBundle} BundleFileName: {bundleFileName}");

                var fileId = await rpcClient.UploadFileAsync(pathToBundle, bundleFileName);
                Debug.Log("Bundle File Id: " + fileId);
                var imageFileId = await rpcClient.UploadFileAsync(pathToImage, imageFileName);
                Debug.Log("Image File Id: " + imageFileId);

                await rpcClient.CreateWorldPublishTaskAsync(new CreateWorldPublishTaskRequest(
                    id,
                    fileId,
                    data.Name,
                    VRC.Tools.Platform,
                    VRC.Tools.UnityVersion.ToString(),
                    currentUserId,
                    worldSignature,
                    imageFileId,
                    data.Description,
                    data.Tags.ToArray(),
                    data.ReleaseStatus,
                    data.Capacity,
                    data.RecommendedCapacity,
                    data.PreviewYoutubeId,
                    data.UdonProducts.ToArray()
                ));

                return data;
            });

            return false;
        }
    }
}