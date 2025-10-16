using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEditor;
using VRC.SDK3.Editor.Builder;

namespace VRChatContentManagerConnect.Worlds.Editor.Patch {
    [HarmonyPatch(typeof(VRCWorldAssetExporter),
        nameof(VRCWorldAssetExporter.ExportCurrentSceneResource),
        typeof(bool), typeof(Action<string>), typeof(Action<object>))]
    internal static class WorldAssetExporterPatch {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var codes = instructions.ToList();
            var originalCodes = codes.ToList();

            var buildAssetBundleMethodInfo = AccessTools.Method(typeof(BuildPipeline),
                nameof(BuildPipeline.BuildAssetBundles),
                new[] {
                    typeof(string), typeof(AssetBundleBuild[]), typeof(BuildAssetBundleOptions), typeof(BuildTarget)
                });

            var buildAssetBundleCallIndex = codes.FindIndex(code =>
                code.opcode == OpCodes.Call && code.operand is MethodInfo callMethodInfo &&
                callMethodInfo == buildAssetBundleMethodInfo);

            if (buildAssetBundleCallIndex == -1) {
                return originalCodes;
            }

            var buildOptionsCodeIndex = codes.FindLastIndex(buildAssetBundleCallIndex, code =>
                code.opcode == OpCodes.Ldc_I4_S && code.operand is (sbyte)32);
            if (buildOptionsCodeIndex == -1) {
                return originalCodes;
            }

            var buildOptionsCode = codes[buildOptionsCodeIndex];
            buildOptionsCode.operand = (sbyte)(BuildAssetBundleOptions.ForceRebuildAssetBundle |
                                               BuildAssetBundleOptions.UncompressedAssetBundle);

            return codes;
        }
    }
}