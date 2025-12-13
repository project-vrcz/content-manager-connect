using VRChatContentManagerConnect.Avatars.Editor.Patch;
using YesPatchFrameworkForVRChatSdk.PatchApi;

[assembly: ExportYesPatch(typeof(AvatarCreateApiPatch))]
[assembly: ExportYesPatch(typeof(UpdateAvatarBundleApiPatch))]
[assembly: ExportYesPatch(typeof(AvatarBuilderPatch))]
[assembly: ExportYesPatch(typeof(AvatarBuilderBuildAndUploadApiPatch))]