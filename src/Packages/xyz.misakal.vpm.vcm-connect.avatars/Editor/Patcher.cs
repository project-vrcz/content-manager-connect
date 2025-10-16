using UnityEditor;

namespace VRChatContentManagerConnect.Avatars.Editor {
    internal sealed class Patcher {
        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod() {
            var harmony = new HarmonyLib.Harmony("xyz.misakal.vpm.vcm-connect.avatars.patcher");
            
            harmony.PatchAll();
        }
    }
}