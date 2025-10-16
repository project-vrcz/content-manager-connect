using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEditor;
using VRC.SDK3.Builder;

namespace VRChatContentManagerConnect.Avatars.Editor.Patch {
    //   private static bool ExportCurrentAvatarResource(
    // UnityEngine.Object avatarResource, bool testAsset, bool buildAssetBundle,
    // out string avatarPrefabPath,
    // Action<string> onProgress = null, Action<object> onContentProcessed = null)
    [HarmonyPatch(typeof(VRCAvatarBuilder), "ExportCurrentAvatarResource")]
    internal static class AvatarBuilderPatch {
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
                code.opcode == OpCodes.Ldc_I4_0 && code.operand is null);
            if (buildOptionsCodeIndex == -1) {
                return originalCodes;
            }

            var buildOptionsCode = codes[buildOptionsCodeIndex];
            buildOptionsCode.opcode = OpCodes.Ldc_I4_S;
            buildOptionsCode.operand = (sbyte)BuildAssetBundleOptions.UncompressedAssetBundle;

            return codes;
        }
    }
}