using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRChatContentManagerConnect.Editor.Views {
    public class ContentManagerSettingsWindow : EditorWindow {
        [MenuItem("Window/VRChat Content Manager Connect/Settings", priority = 2000)]
        [MenuItem("Tools/VRChat Content Manager Connect/Settings")]
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