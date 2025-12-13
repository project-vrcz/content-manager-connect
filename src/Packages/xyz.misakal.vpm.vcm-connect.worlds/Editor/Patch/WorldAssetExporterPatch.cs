using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Editor.Builder;
using VRChatContentManagerConnect.Editor;
using VRChatContentManagerConnect.Editor.Services;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;

namespace VRChatContentManagerConnect.Worlds.Editor.Patch {
    [HarmonyPatch]
    internal class WorldAssetExporterPatch : YesPatchBase {
        public override string Id => "xyz.misakal.vpm.vcm-connect.worlds.fix-build-failed-due-to-file-delete-failed";
        public override string DisplayName => "Fix Worlds Build Failed due to File Delete Failed";
        public override string Description => "Fixes worlds build failures caused by file deletion issues.";

        public override string Category => PatchConst.Category;

        public override bool IsDefaultEnabled => true;

        private readonly Harmony _harmony =
            new("xyz.misakal.vpm.vcm-connect.worlds.fix-build-failed-due-to-file-delete-failed");

        public override void Patch() {
            _harmony.PatchAll(typeof(WorldAssetExporterPatch));
        }

        public override void UnPatch() {
            _harmony.UnpatchSelf();
        }

        [HarmonyPatch(typeof(VRCWorldAssetExporter),
            nameof(VRCWorldAssetExporter.ExportCurrentSceneResource),
            typeof(bool), typeof(Action<string>), typeof(Action<object>))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var codes = instructions.ToList();

            var codeMatcher = new CodeMatcher(codes)
                .MatchStartForward(
                    CodeMatch.Calls(() => BuildPipeline.BuildAssetBundles(default, default, default, default)))
                .ThrowIfInvalid("Could not find BuildAssetBundles calls.")
                .RemoveInstruction()
                .InsertAndAdvance(
                    CodeInstruction.Call(() => BuildAssetBundles(default, default, default, default)))
                .MatchStartForward(
                    CodeMatch.Calls(() => File.Delete(default))
                )
                .ThrowIfInvalid("Could not find File.Delete calls.")
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