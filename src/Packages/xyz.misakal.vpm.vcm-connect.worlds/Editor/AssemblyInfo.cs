using VRChatContentPublisherConnect.Worlds.Editor.Patch;
using YesPatchFrameworkForVRChatSdk.PatchApi;

[assembly: ExportYesPatch(typeof(WorldAssetExporterPatch))]
[assembly: ExportYesPatch(typeof(WorldBuilderApiPatch))]
[assembly: ExportYesPatch(typeof(RedirectUploadApiPatch))]