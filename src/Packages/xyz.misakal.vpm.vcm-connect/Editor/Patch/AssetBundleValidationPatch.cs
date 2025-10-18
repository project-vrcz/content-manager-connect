using System;
using System.IO;
using HarmonyLib;
using UnityEngine;
using VRC;
using VRC.SDKBase.Editor.Validation;

namespace VRChatContentManagerConnect.Editor.Patch {
    // public static bool CheckIfAssetBundleFileTooLarge(
    //  ContentType contentType,
    //  string vrcFilePath,
    //  out int fileSize,
    //  bool mobilePlatform
    // )
    [HarmonyPatch(typeof(ValidationEditorHelpers), nameof(ValidationEditorHelpers.CheckIfAssetBundleFileTooLarge))]
    internal static class AssetBundleValidationPatch {
        public static bool Prefix(ref bool __result, ContentType contentType, string vrcFilePath, ref int fileSize,
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
                Debug.LogError("[VRCCM.Connect] Failed to validate asset bundle size: exception occurred when accessing file at path " + vrcFilePath);
                return false;
            }
        }
    }
}