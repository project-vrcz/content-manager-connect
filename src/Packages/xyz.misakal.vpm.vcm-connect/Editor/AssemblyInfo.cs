using System.Runtime.CompilerServices;
using VRChatContentManagerConnect.Editor.Patch;
using YesPatchFrameworkForVRChatSdk.PatchApi;

[assembly: ExportYesPatch(typeof(AssetBundleValidationPatch))]

[assembly: InternalsVisibleTo("VRChatContentManagerConnect.Worlds.Editor")]
[assembly: InternalsVisibleTo("VRChatContentManagerConnect.Avatars.Editor")]
[assembly: InternalsVisibleTo("VRChatContentManagerConnect.Avatars.Editor.ContinuousAvatarUploader")]