using VRChatContentManagerConnect.Worlds.Editor.Patch;
using YesPatchFrameworkForVRChatSdk.PatchApi;

[assembly: ExportYesPatch(typeof(WorldAssetExporterPatch))]
[assembly: ExportYesPatch(typeof(WorldBundleUploadApiPatch))]
[assembly: ExportYesPatch(typeof(WorldBuilderApiPatch))]
[assembly: ExportYesPatch(typeof(WorldCreateApiPatch))]