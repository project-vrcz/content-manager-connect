using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRChatContentManagerConnect.Editor.MenuItems;

namespace VRChatContentManagerConnect.Editor.Views {
    public class ContentManagerSettingsWindow : EditorWindow {
        [MenuItem(MenuItemPath.RootWindowMenuItemPath + "Settings", priority = 2000)]
        [MenuItem(MenuItemPath.RootMenuItemPath + "Settings")]
        public static void ShowSettings() {
            var window = GetWindow<ContentManagerSettingsWindow>();
            window.titleContent = new GUIContent("Connect Settings");
        }

        public void CreateGUI() {
            var root = rootVisualElement;

            root.Add(new ContentManagerSettingsView() {
                style = { height = new StyleLength(Length.Percent(100)) }
            });
        }
    }
}