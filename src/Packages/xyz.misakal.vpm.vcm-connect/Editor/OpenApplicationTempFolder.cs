using UnityEditor;
using UnityEngine;
using VRChatContentPublisherConnect.Editor.MenuItems;

namespace VRChatContentPublisherConnect.Editor;

internal static class OpenApplicationTempFolder {
    [MenuItem(MenuItemPath.RootMenuItemPath + "Open Application Temp Folder")]
    public static void Open() {
        Application.OpenURL(Application.temporaryCachePath);
    }
}