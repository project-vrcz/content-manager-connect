using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace VRChatContentManagerConnect.Avatars.Editor.Utils {
    internal static class CauUtils {
        internal static bool IsCauUploadInProgress() {
        #if VCCM_SUPPORTED_CAU_VERSION
            var cauAssembly = GetCauAssembly();

            if (cauAssembly is null) {
                Debug.LogWarning("[VRCCM.Connect] CAU assembly not found.");
                return false;
            }

            var uploadOrchestratorType = GetUploadOrchestratorType();
            if (uploadOrchestratorType is null) {
                Debug.LogWarning("[VRCCM.Connect] CAU UploadOrchestrator type not found.");
                return false;
            }

            var isUploadInProgressMethod = uploadOrchestratorType.GetMethod(
                "IsUploadInProgress",
                0,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                Type.EmptyTypes,
                Array.Empty<ParameterModifier>());

            if (isUploadInProgressMethod is null || isUploadInProgressMethod.ReturnType != typeof(bool)) {
                Debug.LogWarning("[VRCCM.Connect] CAU static bool IsUploadInProgress() method not found.");
                return false;
            }

            var isUploadInProgress = (bool)isUploadInProgressMethod.Invoke(null, Array.Empty<object>());
            return isUploadInProgress;
        #else
            return false;
        #endif
        }

        internal static void TryStopContinuousAvatarUploader() {
        #if VCCM_SUPPORTED_CAU_VERSION
            if (!IsCauUploadInProgress())
                return;

            var uploadOrchestratorType = GetUploadOrchestratorType();
            if (uploadOrchestratorType is null) {
                Debug.LogWarning("[VRCCM.Connect] CAU UploadOrchestrator type not found.");
                return;
            }

            var cancelUploadsMethod = uploadOrchestratorType.GetMethod(
                "CancelUpload",
                0,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                Type.EmptyTypes,
                Array.Empty<ParameterModifier>());

            if (cancelUploadsMethod is null) {
                Debug.LogWarning("[VRCCM.Connect] CAU static CancelUpload() method not found.");
                return;
            }

            Debug.Log("[VRCCM.Connect] Detected ongoing CAU upload. Cancelling CAU upload.");
            cancelUploadsMethod.Invoke(null, Array.Empty<object>());
        #endif
        }

        [CanBeNull]
        private static Type GetUploadOrchestratorType() {
        #if VCCM_SUPPORTED_CAU_VERSION
            var cauAssembly = GetCauAssembly();

            if (cauAssembly is null) {
                Debug.LogWarning("[VRCCM.Connect] CAU assembly not found.");
                return null;
            }

            return cauAssembly.GetType("Anatawa12.ContinuousAvatarUploader.Editor.UploadOrchestrator");
        #else
            return null;
        #endif
        }

        [CanBeNull] private static Assembly GetCauAssembly() {
        #if VCCM_SUPPORTED_CAU_VERSION
            var cauAssembly = AccessTools.AllAssemblies()
                .FirstOrDefault(assembly =>
                    assembly.GetName().Name == "com.anatawa12.continuous-avatar-uploader.editor");
            return cauAssembly;
        #else
            return null;
        #endif
        }
    }
}