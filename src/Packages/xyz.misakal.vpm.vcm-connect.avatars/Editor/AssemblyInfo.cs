using VRChatContentManagerConnect.Avatars.Editor.Patch;
using YesPatchFrameworkForVRChatSdk.PatchApi;

[assembly: ExportYesPatch(typeof(RedirectUploadApiPatch))]
[assembly: ExportYesPatch(typeof(AvatarBuilderPatch))]
[assembly: ExportYesPatch(typeof(PreUploadCheckPatch))]