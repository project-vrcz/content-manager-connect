using UnityEditor;

namespace VRChatContentManagerConnect.Worlds.Editor {
    internal sealed class Patcher {
        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod() {
            var harmony = new HarmonyLib.Harmony("xyz.misakal.vpm.vcm-connect.worlds.patcher");
            
            harmony.PatchAll();
        }
    }
}