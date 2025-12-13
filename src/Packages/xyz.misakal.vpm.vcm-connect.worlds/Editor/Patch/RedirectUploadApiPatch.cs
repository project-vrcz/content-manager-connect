using HarmonyLib;
using VRChatContentManagerConnect.Editor;
using YesPatchFrameworkForVRChatSdk.PatchApi;
using YesPatchFrameworkForVRChatSdk.PatchApi.Extensions;

namespace VRChatContentManagerConnect.Worlds.Editor.Patch {
    [HarmonyPatch]
    internal partial class RedirectUploadApiPatch : YesPatchBase {
        public override string Id => "xyz.misakal.vpm.vcm-connect.worlds.redirect-world-upload-api";
        public override string DisplayName => "Redirect World Upload to Content Manager";

        public override string Description =>
            "Redirects world upload requests to VRChat Content Manager when enabled in settings.";

        public override string Category => PatchConst.Category;

        public override bool IsDefaultEnabled => true;

        private readonly Harmony _harmony = new("xyz.misakal.vpm.vcm-connect.avatars.redirect-world-upload-api");

        public override void Patch() {
            _harmony.PatchAll(typeof(RedirectUploadApiPatch));
        }

        public override void UnPatch() {
            _harmony.UnpatchSelf();
        }
    }
}