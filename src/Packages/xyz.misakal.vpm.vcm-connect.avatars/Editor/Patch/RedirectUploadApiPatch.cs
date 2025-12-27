using HarmonyLib;
using VRChatContentPublisherConnect.Editor;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace VRChatContentPublisherConnect.Avatars.Editor.Patch {
    [HarmonyPatch]
    internal partial class RedirectUploadApiPatch : YesPatchBase {
        public override string Id => "xyz.misakal.vpm.vcm-connect.avatars.redirect-avatar-upload-api";
        public override string DisplayName => "Redirect Avatar Upload to Content Manager";

        public override string Description =>
            "Redirects avatar upload requests to VRChat Content Manager when enabled in settings.";

        public override string Category => PatchConst.Category;

        public override bool IsDefaultEnabled => true;

        private static readonly YesLogger _logger = new(LoggerConst.LoggerPrefix + nameof(RedirectUploadApiPatch));

        private readonly Harmony _harmony = new("xyz.misakal.vpm.vcm-connect.avatars.redirect-avatar-upload-api");

        public override void Patch() {
            _harmony.PatchAll(typeof(RedirectUploadApiPatch));
        }

        public override void UnPatch() {
            _harmony.UnpatchSelf();
        }
    }
}