using UnityEditor;
using UnityEngine;

namespace VRChatContentManagerConnect.Editor;

internal static class OpenApplicationTempFolder {
    [MenuItem("Tools/VRChat Content Manager Connect/Open Application Temp Folder")]
    public static void Open() {
        Application.OpenURL(Application.temporaryCachePath);
    }
}