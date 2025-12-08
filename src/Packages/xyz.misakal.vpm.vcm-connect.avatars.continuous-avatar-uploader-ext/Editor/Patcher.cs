using UnityEditor;

namespace VRChatContentManagerConnect.Avatars.Editor.ContinuousAvatarUploader;

internal sealed class Patcher {
    [InitializeOnLoadMethod]
    private static void InitializeOnLoadMethod() {
        var harmony = new HarmonyLib.Harmony("xyz.misakal.vpm.vcm-connect.avatars.continuous-avatar-uploader.patcher");
            
        harmony.PatchAll();
    }
}