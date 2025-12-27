using System.Runtime.CompilerServices;
using VRChatContentPublisherConnect.Editor.Patch;
using YesPatchFrameworkForVRChatSdk.PatchApi;

[assembly: ExportYesPatch(typeof(AssetBundleValidationPatch))]

[assembly: InternalsVisibleTo("VRChatContentPublisherConnect.Worlds.Editor")]
[assembly: InternalsVisibleTo("VRChatContentPublisherConnect.Avatars.Editor")]
[assembly: InternalsVisibleTo("VRChatContentPublisherConnect.Avatars.Editor.ContinuousAvatarUploader")]