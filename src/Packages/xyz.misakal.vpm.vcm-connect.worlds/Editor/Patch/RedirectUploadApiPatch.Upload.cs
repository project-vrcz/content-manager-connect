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
using VRChatContentManagerConnect.Editor.Models.RpcApi.Request.Task;
using VRChatContentManagerConnect.Editor.Services;
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Worlds.Editor.Patch {
    internal partial class RedirectUploadApiPatch {
        // public static async Task<VRCWorld> UpdateWorldBundle(
        // string id, VRCWorld data, string pathToBundle, string worldSignature, 
        // Action<string, float> onProgress = null, CancellationToken cancellationToken = default)
        [HarmonyPatch(typeof(VRCApi), nameof(VRCApi.UpdateWorldBundle),
            typeof(string), typeof(VRCWorld), typeof(string), typeof(string),
            typeof(Action<string, float>), typeof(CancellationToken))]
        [HarmonyPrefix]
        internal static bool UpdateWorldBundlePrefix(ref Task<VRCWorld> __result,
            string id, VRCWorld data, string pathToBundle, string worldSignature,
            Action<string, float> onProgress = null,
            CancellationToken cancellationToken = default
        ) {
            var app = ConnectEditorApp.Instance;
            if (app == null) {
                _logger.LogWarning("ConnectEditorApp instance is null. Skipping patch.");
                return true;
            }

            var settings = app.ServiceProvider.GetRequiredService<AppSettingsService>();
            if (!settings.GetSettings().UseContentManager)
                return true;

            __result = Task.Run(async () => {
                if (string.IsNullOrEmpty(id))
                    throw new ArgumentNullException(nameof(id), "World ID cannot be null or empty.");

                if (string.IsNullOrEmpty(pathToBundle))
                    throw new ArgumentNullException(nameof(pathToBundle), "Path to bundle cannot be null or empty.");

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                if (!File.Exists(pathToBundle))
                    throw new FileNotFoundException("The specified bundle file does not exist.", pathToBundle);

                var bundleFileName = "World - " + data.Name + " - Asset bundle - " + Application.unityVersion + "_" +
                                     ApiWorld.VERSION.ApiVersion +
                                     "_" + Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl();

                _logger.LogDebug($"WorldId: {id} PathToBundle: {pathToBundle} BundleFileName: {bundleFileName}");

                var currentUserId = APIUser.CurrentUser?.id;
                if (currentUserId is null)
                    throw new InvalidOperationException("Current user is not logged in.");

                // Send Bundle File to App, Start new task

                var rpcClient = app.ServiceProvider.GetRequiredService<RpcClientService>();
                if (rpcClient.State == RpcClientState.Disconnected) {
                    _logger.LogDebug("RPC client is disconnected. Attempting to restore session...");

                    try {
                        await rpcClient.RestoreSessionAsync();
                        _logger.LogDebug("RPC session restored successfully.");
                    }
                    catch (Exception ex) {
                        _logger.LogError(ex, "Failed to restore RPC session. Aborting world bundle upload.");

                        throw new InvalidOperationException("RPC client is not connected.", ex);
                    }
                }

                if (rpcClient.State != RpcClientState.Connected) {
                    throw new InvalidOperationException("RPC client is not connected.");
                }

                var fileId = await rpcClient.UploadFileAsync(pathToBundle, bundleFileName);
                _logger.LogDebug("Bundle File Id: " + fileId);

                await rpcClient.CreateWorldPublishTaskAsync(new CreateWorldPublishTaskRequest(
                    id,
                    fileId,
                    data.Name,
                    Tools.Platform,
                    Tools.UnityVersion.ToString(),
                    currentUserId,
                    worldSignature,
                    null,
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

            // replace method
            return false;
        }
    }
}