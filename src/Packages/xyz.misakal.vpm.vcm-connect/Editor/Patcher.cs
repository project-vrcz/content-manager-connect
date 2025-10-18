using UnityEditor;

namespace VRChatContentManagerConnect.Editor {
    internal sealed class Patcher {
        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod() {
            var harmony = new HarmonyLib.Harmony("xyz.misakal.vpm.vcm-connect.patcher");
            
            harmony.PatchAll();
        }
    }
}