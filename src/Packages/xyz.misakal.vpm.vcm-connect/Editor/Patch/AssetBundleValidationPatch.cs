using System;
using System.IO;
using HarmonyLib;
using UnityEngine;
using VRC;
using VRC.SDKBase.Editor.Validation;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;

namespace VRChatContentManagerConnect.Editor.Patch {
    // public static bool CheckIfAssetBundleFileTooLarge(
    //  ContentType contentType,
    //  string vrcFilePath,
    //  out int fileSize,
    //  bool mobilePlatform
    // )
    [HarmonyPatch]
    internal class AssetBundleValidationPatch : YesPatchBase {
        public override string Id => "xyz.misakal.vpm.vcm-connect.skip-compression-asset-bundle-validation-size-patch";
        public override string DisplayName => "Skip Compression Asset Bundle Size Validation Patch";

        public override string Description =>
            "Skip the asset bundle size check for \"compression\" bundle to allow send large uncompressed asset bundles to App.";

        public override string Category => PatchConst.Category;

        public override bool IsDefaultEnabled => true;

        private readonly Harmony _harmony =
            new("xyz.misakal.vpm.vcm-connect.skip-compression-asset-bundle-validation-size-patch");

        public override void Patch() {
            _harmony.PatchAll(typeof(AssetBundleValidationPatch));
        }

        public override void UnPatch() {
            _harmony.UnpatchSelf();
        }

        [HarmonyPatch(typeof(ValidationEditorHelpers), nameof(ValidationEditorHelpers.CheckIfAssetBundleFileTooLarge))]
        [HarmonyPrefix]
        public static bool CheckIfAssetBundleFileTooLargePrefix(
            ref bool __result,
            ContentType contentType,
            string vrcFilePath,
            ref int fileSize,
            bool mobilePlatform) {
            fileSize = 0;
            __result = true;

            if (!File.Exists(vrcFilePath)) {
                Debug.LogError("[VRCCM.Connect] Failed to validate asset bundle size: file does not exist at path " +
                               vrcFilePath);
                return true;
            }

            try {
                fileSize = (int)new FileInfo(vrcFilePath).Length;
                __result = false;
                return false;
            }
            catch (Exception ex) {
                Debug.LogException(ex);
                Debug.LogError(
                    "[VRCCM.Connect] Failed to validate asset bundle size: exception occurred when accessing file at path " +
                    vrcFilePath);
                return false;
            }
        }
    }
}