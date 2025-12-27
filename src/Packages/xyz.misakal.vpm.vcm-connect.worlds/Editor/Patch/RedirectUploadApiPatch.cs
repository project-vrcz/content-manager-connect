using HarmonyLib;
using VRChatContentPublisherConnect.Editor;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;
using YesPatchFrameworkForVRChatSdk.PatchApi.Logging;

namespace VRChatContentPublisherConnect.Worlds.Editor.Patch {
    [HarmonyPatch]
    internal partial class RedirectUploadApiPatch : YesPatchBase {
        public override string Id => "xyz.misakal.vpm.vcm-connect.worlds.redirect-world-upload-api";
        public override string DisplayName => "Redirect World Upload to Content Publisher";

        public override string Description =>
            "Redirects world upload requests to VRChat Content Publisher when enabled in settings.";

        public override string Category => PatchConst.Category;

        public override bool IsDefaultEnabled => true;

        private readonly Harmony _harmony = new("xyz.misakal.vpm.vcm-connect.avatars.redirect-world-upload-api");
        private static readonly YesLogger _logger = new(LoggerConst.LoggerPrefix + nameof(RedirectUploadApiPatch));

        public override void Patch() {
            _harmony.PatchAll(typeof(RedirectUploadApiPatch));
        }

        public override void UnPatch() {
            _harmony.UnpatchSelf();
        }
    }
}