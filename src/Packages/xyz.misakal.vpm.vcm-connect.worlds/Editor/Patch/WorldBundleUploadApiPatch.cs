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
using VRChatContentManagerConnect.Editor.Services.Rpc;

namespace VRChatContentManagerConnect.Worlds.Editor.Patch {
    // public static async Task<VRCWorld> UpdateWorldBundle(
    // string id, VRCWorld data, string pathToBundle, string worldSignature, 
    // Action<string, float> onProgress = null, CancellationToken cancellationToken = default)
    [HarmonyPatch(typeof(VRCApi), nameof(VRCApi.UpdateWorldBundle),
        typeof(string), typeof(VRCWorld), typeof(string), typeof(string),
        typeof(Action<string, float>), typeof(CancellationToken))]
    internal static class WorldBundleUploadApiPatch {
        internal static bool Prefix(ref Task<VRCWorld> __result,
            string id, VRCWorld data, string pathToBundle, string worldSignature,
            Action<string, float> onProgress = null,
            CancellationToken cancellationToken = default
        ) {
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
                               "_" + VRC.Tools.Platform + "_" + API.GetServerEnvironmentForApiUrl();
                
                Debug.Log($"WorldId: {id} PathToBundle: {pathToBundle} BundleFileName: {bundleFileName}");
                // Send Bundle File to App, Start new task

                var app = ConnectEditorApp.Instance;
                if (app == null)
                    throw new InvalidOperationException("Connect Editor App instance is null.");

                var rpcClient = app.ServiceProvider.GetRequiredService<RpcClientService>();
                var fileId = await rpcClient.UploadFileAsync(pathToBundle, bundleFileName);
                Debug.Log("Bundle File Id: " + fileId);
                
                await rpcClient.CreateWorldPublishTaskAsync(id, fileId, data.Name, Tools.Platform, Tools.UnityVersion.ToString(), worldSignature);
                
                return data;
            });

            // replace method
            return false;
        }
    }
}