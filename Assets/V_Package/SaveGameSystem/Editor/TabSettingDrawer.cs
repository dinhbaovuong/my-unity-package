using UnityEditor;
using UnityEngine;

namespace VPackage.SaveGameSystem.EditorNS
{
    public class TabSettingDrawer
    {
        public SaveGameEditorWindow window;
        public SGEditorSettingDM editorSetting;
        
        public TabSettingDrawer(SaveGameEditorWindow window, SGEditorSettingDM editorSetting)
        {
            this.window = window;
            this.editorSetting = editorSetting;
        }

        public void OnEnable()
        {
            
        }

        public void OnGUI()
        {
            GUILayout.Label("Coming soon...", EditorStyles.boldLabel);
        }
    }
}