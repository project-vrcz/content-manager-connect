using System.Reflection;
using HarmonyLib;
using UnityEditor;
using VRChatContentPublisherConnect.Editor;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;
using Uploader = Anatawa12.ContinuousAvatarUploader.Editor.ContinuousAvatarUploader;

namespace VRChatContentPublisherConnect.Avatars.Editor.ContinuousAvatarUploader.Patch;

internal class PreUploadCheckPatch : YesPatchBase {
    public override string Id => "xyz.misakal.vpm.vcm-connect.avatars.continuous-avatar-uploader-ext.pre-upload-check";
    public override string DisplayName => "Pre-Upload Check";

    public override string Description =>
        "Check RPC connection before uploading an avatar from Continuous Avatar Uploader.";

    public override string Category => CauPatchConst.Category;

    public override bool IsDefaultEnabled => true;

    private readonly Harmony _harmony =
        new("xyz.misakal.vpm.vcm-connect.avatars.continuous-avatar-uploader-ext.pre-upload-check");

    public override void Patch() {
        _harmony.PatchAll(typeof(PreUploadCheckPatch));
    }

    public override void UnPatch() {
        _harmony.UnpatchSelf();
    }

    private static MethodInfo? DoStartUploadMethod;

    private static bool _isCheckSucceed;

    [HarmonyPatch(typeof(Uploader), "DoStartUpload")]
    [HarmonyPrefix]
    private static bool Prefix(Uploader? __instance) {
        if (PreUploadCheck.IsTaskRunning)
            return false;

        if (_isCheckSucceed) {
            _isCheckSucceed = false;
            return true;
        }

        PreUploadCheck.RunPreUploadCheck(() => {
                DoStartUploadMethod ??=
                    AccessTools.Method(typeof(Uploader),
                        "DoStartUpload");
                if (DoStartUploadMethod is null) {
                    EditorUtility.DisplayDialog(
                        "Failed to Start Upload",
                        "Auto reconnect is succeed, but we can start the upload for you.\n" +
                        "Click the upload button again may solve your problem.\n\n" +
                        "Report this issue to the developer: ContinuousAvatarUploader.DoStartUpload MethodInfo is null.",
                        "OK");
                    return;
                }

                if (__instance is null) {
                    EditorUtility.DisplayDialog(
                        "Failed to Start Upload",
                        "Auto reconnect is succeed, but we can start the upload for you.\n" +
                        "Click the upload button again may solve your problem.\n\n" +
                        "Report this issue to the developer: ContinuousAvatarUploader instance is null.",
                        "OK");
                    return;
                }

                _isCheckSucceed = true;
                DoStartUploadMethod.Invoke(__instance, null);
            },
            () => _isCheckSucceed = false
        );

        return false;
    }
}