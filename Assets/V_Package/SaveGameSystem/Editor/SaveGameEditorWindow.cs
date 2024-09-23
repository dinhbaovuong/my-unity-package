using System;
using UnityEditor;
using UnityEngine;

namespace VPackage.SaveGameSystem.EditorNS
{
    public class SaveGameEditorWindow : EditorWindow
    {
        private SGEditorSettingDM editorSetting;
        private string[] listTabName = new[] {"Explorer", "Settings"};

        private TabExplorerDrawer tabExplorerDrawer;
        private TabSettingDrawer tabSettingDrawer;
        
        [MenuItem("Tools/ZeroX/Save Game")]
        public static SaveGameEditorWindow OpenWindow()
        {
            var window = GetWindow<SaveGameEditorWindow>();
            window.titleContent = new GUIContent("Save Game");
            window.Show();
            return window;
        }
        
        private void OnEnable()
        {
            editorSetting = SGEditorSettingDI.Load();
            
            tabExplorerDrawer = new TabExplorerDrawer(this, editorSetting);
            tabExplorerDrawer.OnEnable();
            
            tabSettingDrawer = new TabSettingDrawer(this, editorSetting);
            tabSettingDrawer.OnEnable();
        }

        private void OnGUI()
        {
            DrawToolBar();
            DrawTabSelected();
        }

        void DrawToolBar()
        {
            int newTabIndexSelected = GUILayout.Toolbar(editorSetting.tabIndexSelected, listTabName);
            if (newTabIndexSelected != editorSetting.tabIndexSelected)
            {
                editorSetting.tabIndexSelected = newTabIndexSelected;
                SGEditorSettingDI.Save(editorSetting);
            }
        }
        
        void DrawTabSelected()
        {
            if (editorSetting.tabIndexSelected < 0)
            {
                editorSetting.tabIndexSelected = 0;
                SGEditorSettingDI.Save(editorSetting);
            }

            if (editorSetting.tabIndexSelected >= listTabName.Length)
            {
                editorSetting.tabIndexSelected = listTabName.Length - 1;
                SGEditorSettingDI.Save(editorSetting);
            }
            
            string tabName = listTabName[editorSetting.tabIndexSelected];
            switch (tabName)
            {
                case "Explorer":
                {
                    tabExplorerDrawer.OnGUI();
                    return;
                }
                case "Settings":
                {
                    tabSettingDrawer.OnGUI();
                    return;
                }
            }
        }
    }
}