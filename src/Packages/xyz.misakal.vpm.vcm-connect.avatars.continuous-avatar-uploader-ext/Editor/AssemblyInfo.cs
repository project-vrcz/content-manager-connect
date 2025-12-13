using VRChatContentManagerConnect.Avatars.Editor.ContinuousAvatarUploader.Patch;
using YesPatchFrameworkForVRChatSdk.PatchApi;

[assembly: ExportYesPatch(typeof(ConnectionStatusPatch))]
[assembly: ExportYesPatch(typeof(PreUploadCheckPatch))]