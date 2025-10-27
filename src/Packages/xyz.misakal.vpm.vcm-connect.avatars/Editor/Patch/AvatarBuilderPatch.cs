using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Builder;
using VRChatContentManagerConnect.Editor;
using VRChatContentManagerConnect.Editor.Services;

namespace VRChatContentManagerConnect.Avatars.Editor.Patch {
    //   private static bool ExportCurrentAvatarResource(
    // UnityEngine.Object avatarResource, bool testAsset, bool buildAssetBundle,
    // out string avatarPrefabPath,
    // Action<string> onProgress = null, Action<object> onContentProcessed = null)
    [HarmonyPatch(typeof(VRCAvatarBuilder), "ExportCurrentAvatarResource")]
    internal static class AvatarBuilderPatch {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var codes = instructions.ToList();

            var codeMatcher = new CodeMatcher(codes)
                .MatchStartForward(
                    CodeMatch.Calls(() => BuildPipeline.BuildAssetBundles(default, default, default, default)))
                .ThrowIfInvalid("[VRCCM.Connect] AvatarBuilderPatch: Could not find BuildAssetBundles calls.")
                .RemoveInstruction()
                .InsertAndAdvance(
                    CodeInstruction.Call(() => BuildAssetBundles(default, default, default, default)))
                .MatchStartForward(
                    CodeMatch.Calls(() => File.Delete(default))
                )
                .ThrowIfInvalid("[VRCCM.Connect] AvatarBuilderPatch: Could not find File.Delete calls.")
                .Repeat(codeMatcher => {
                    codeMatcher
                        .RemoveInstruction()
                        .InsertAndAdvance(
                            CodeInstruction.Call(() => DelayedDelete.Delete(default))
                        );
                });

            return codeMatcher.Instructions();
        }

        private static AssetBundleManifest BuildAssetBundles(string outputPath, AssetBundleBuild[] builds,
            BuildAssetBundleOptions options, BuildTarget target) {
            if (ConnectEditorApp.Instance is { } app) {
                var settings = app.ServiceProvider.GetRequiredService<AppSettingsService>();

                if (settings.GetSettings().UseContentManager) {
                    if ((options & BuildAssetBundleOptions.UncompressedAssetBundle) == 0) {
                        options |= BuildAssetBundleOptions.UncompressedAssetBundle;
                    }
                }
            }

            return BuildPipeline.BuildAssetBundles(outputPath, builds,
                options, target);
        }
    }
}