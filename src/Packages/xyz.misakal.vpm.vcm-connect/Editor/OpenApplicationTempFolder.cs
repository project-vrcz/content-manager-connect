using UnityEditor;
using UnityEngine;
using VRChatContentManagerConnect.Editor.MenuItems;

namespace VRChatContentManagerConnect.Editor;

internal static class OpenApplicationTempFolder {
    [MenuItem(MenuItemPath.RootMenuItemPath + "Open Application Temp Folder")]
    public static void Open() {
        Application.OpenURL(Application.temporaryCachePath);
    }
}