using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Builder;
using VRChatContentManagerConnect.Editor;
using VRChatContentManagerConnect.Editor.Services;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;

namespace VRChatContentManagerConnect.Avatars.Editor.Patch {
    [HarmonyPatch]
    internal class AvatarBuilderPatch : YesPatchBase {
        public override string Id => "xyz.misakal.vpm.vcm-connect.avatars.fix-build-failed-due-to-file-delete-failed";
        public override string DisplayName => "Fix Avatar Build Failed";
        public override string Description => "Fixes avatar build failures caused by file deletion issues.";

        public override string Category => PatchConst.Category;

        public override bool IsDefaultEnabled => true;

        private readonly Harmony _harmony =
            new("xyz.misakal.vpm.vcm-connect.avatars.fix-build-failed-due-to-file-delete-failed");

        public override void Patch() {
            _harmony.PatchAll(typeof(AvatarBuilderPatch));
        }

        public override void UnPatch() {
            _harmony.UnpatchSelf();
        }

        //   private static bool ExportCurrentAvatarResource(
        // UnityEngine.Object avatarResource, bool testAsset, bool buildAssetBundle,
        // out string avatarPrefabPath,
        // Action<string> onProgress = null, Action<object> onContentProcessed = null)
        [HarmonyPatch(typeof(VRCAvatarBuilder), "ExportCurrentAvatarResource")]
        [HarmonyTranspiler]
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