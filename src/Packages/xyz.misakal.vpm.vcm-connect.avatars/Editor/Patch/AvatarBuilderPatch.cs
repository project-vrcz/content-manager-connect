using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using HarmonyLib;
using UnityEditor;
using UnityEngine;
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
                Debug.LogError("[VRCCM.Connect] AvatarBuilderPatch: Could not find BuildAssetBundles call.");
                return originalCodes;
            }

            var buildOptionsCodeIndex = codes.FindLastIndex(buildAssetBundleCallIndex, code =>
                code.opcode == OpCodes.Ldc_I4_0 && code.operand is null);
            if (buildOptionsCodeIndex == -1) {
                Debug.LogError("[VRCCM.Connect] AvatarBuilderPatch: Could not find BuildAssetBundleOptions load.");
                return originalCodes;
            }

            var buildOptionsCode = codes[buildOptionsCodeIndex];
            buildOptionsCode.opcode = OpCodes.Ldc_I4_S;
            buildOptionsCode.operand = (sbyte)BuildAssetBundleOptions.UncompressedAssetBundle;

            var getTemporaryCachePathMethodInfo = AccessTools.PropertyGetter(typeof(Application),
                nameof(Application.temporaryCachePath));
            var assetBundleNameFieldInfo =
                AccessTools.Field(typeof(AssetBundleBuild), nameof(AssetBundleBuild.assetBundleName));

            var pathCombineMethodInfo = AccessTools.Method(typeof(System.IO.Path),
                nameof(System.IO.Path.Combine),
                new[] { typeof(string), typeof(string) });

            var pathCombineCalls = codes
                .Where(code => code.opcode == OpCodes.Call &&
                               code.operand is MethodInfo methodInfo &&
                               methodInfo == pathCombineMethodInfo)
                .ToArray();
            if (pathCombineCalls.Length == 0) {
                Debug.LogError("[VRCCM.Connect] AvatarBuilderPatch: Could not find any PathCombine calls.");
                return originalCodes;
            }

            LocalBuilder bundlePathLocal = null;
            foreach (var pathCombineCall in pathCombineCalls) {
                var callIndex = codes.IndexOf(pathCombineCall);
                var firstArgIndex = callIndex - 3;
                var secArgIndex = callIndex - 1;
                var localIndex = callIndex + 1;
                if (secArgIndex - 1 < 0 || firstArgIndex - 1 < 0 || localIndex >= codes.Count)
                    continue;

                var firstArg = codes[firstArgIndex];
                var secArg = codes[secArgIndex];
                var local = codes[localIndex];

                if (
                    // Application.temporaryCachePath
                    firstArg.opcode == OpCodes.Call && firstArg.operand is MethodInfo methodInfo &&
                    methodInfo == getTemporaryCachePathMethodInfo &&
                    // assetBundleBuild.assetBundleName
                    secArg.opcode == OpCodes.Ldfld && secArg.operand is FieldInfo fieldInfo &&
                    fieldInfo == assetBundleNameFieldInfo) {
                    if (local.opcode == OpCodes.Stloc_S && local.operand is LocalBuilder localBuilder) {
                        bundlePathLocal = localBuilder;
                        break;
                    }
                }
            }

            if (bundlePathLocal is null) {
                Debug.LogError("[VRCCM.Connect] AvatarBuilderPatch: Could not find bundle path local variable.");
                return originalCodes;
            }
            
            var sessionStateGetStringMethodInfo = AccessTools.Method(typeof(SessionState), nameof(SessionState.GetString),
                new[] { typeof(string), typeof(string) });

            LocalBuilder previousBuildingAssetBundlePathLocal = null;
            var sessionStateGetStringCalls = codes
                .Where(code => code.opcode == OpCodes.Call &&
                               code.operand is MethodInfo methodInfo &&
                               methodInfo == sessionStateGetStringMethodInfo)
                .ToArray();
            foreach (var sessionStateGetStringCall in sessionStateGetStringCalls) {
                var callIndex = codes.IndexOf(sessionStateGetStringCall);
                var firstArgIndex = callIndex - 2;
                
                if (firstArgIndex < 0 || firstArgIndex >= codes.Count)
                    continue;
                
                var firstArg = codes[firstArgIndex];
                if (firstArg.opcode == OpCodes.Ldstr && firstArg.operand is "previousBuildingAssetBundlePath") {
                    var localIndex = callIndex + 1;
                    if (localIndex < 0 || localIndex >= codes.Count)
                        continue;

                    var local = codes[localIndex];
                    if (local.opcode == OpCodes.Stloc_S && local.operand is LocalBuilder localBuilder) {
                        previousBuildingAssetBundlePathLocal = localBuilder;
                        break;
                    }
                }
            }

            if (previousBuildingAssetBundlePathLocal is null) {
                Debug.LogError("[VRCCM.Connect] AvatarBuilderPatch: Could not find previousBuildingAssetBundlePath local variable.");
                return originalCodes;
            }

            var fileDeleteMethodInfo = AccessTools.Method(typeof(System.IO.File),
                nameof(System.IO.File.Delete),
                new[] { typeof(string) });

            var fileDeleteCalls = codes.Where(code =>
                    code.opcode == OpCodes.Call && code.operand is MethodInfo methodInfo &&
                    methodInfo == fileDeleteMethodInfo)
                .ToArray();
            foreach (var fileDeleteCall in fileDeleteCalls) {
                var callIndex = codes.IndexOf(fileDeleteCall);
                var argIndex = callIndex - 1;
                if (argIndex < 0)
                    continue;

                var arg = codes[argIndex];
                if (arg.opcode == OpCodes.Ldloc_S && arg.operand is LocalBuilder localBuilder &&
                    (localBuilder == bundlePathLocal || localBuilder == previousBuildingAssetBundlePathLocal)) {
                    codes[argIndex].opcode = OpCodes.Nop;
                    codes[argIndex].operand = null;
                    codes[callIndex].opcode = OpCodes.Nop;
                    codes[callIndex].operand = null;
                }
            }

            return codes;
        }
    }
}