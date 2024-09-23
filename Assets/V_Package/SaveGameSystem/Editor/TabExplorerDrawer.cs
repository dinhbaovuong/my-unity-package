using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VPackage.SaveGameSystem.EditorNS
{
    public class TabExplorerDrawer
    {
        public SaveGameEditorWindow window;
        public SGEditorSettingDM editorSetting;

        private string ipfDatabaseIdText = SaveGame.DefaultDatabaseId;
        private bool isExploring = false;

        private Database currentDatabase;
        public Dictionary<SGNode, NodeDrawer> dictNodeDrawer = new Dictionary<SGNode, NodeDrawer>();

        
        Vector2 scrollPosition = new Vector2();
        
        
        public TabExplorerDrawer(SaveGameEditorWindow window, SGEditorSettingDM editorSetting)
        {
            this.window = window;
            this.editorSetting = editorSetting;
        }

        public void OnEnable()
        {
            
        }

        public void OnGUI()
        {
            GUILayout.Space(20);
            GUILayout.BeginVertical();
            DrawDatabase();

            if (isExploring)
            {
                GUILayout.Space(20);
                DrawToolBar();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                DrawNodeTree();
                EditorGUILayout.EndScrollView();
            }
                
            
            GUILayout.EndVertical();
        }

        void DrawDatabase()
        {
            GUILayout.BeginHorizontal();
            
            GUILayout.Label("Database Id", EditorStyles.boldLabel);
            ipfDatabaseIdText = GUILayout.TextField(ipfDatabaseIdText);
            ipfDatabaseIdText = ipfDatabaseIdText.Replace(" ", "_");
            GUILayout.EndHorizontal();

            bool enableButton = string.IsNullOrEmpty(ipfDatabaseIdText) == false;

            bool oldEnable = GUI.enabled;
            GUI.enabled = enableButton;
            bool clicked = GUILayout.Button("Explore Database");
            GUI.enabled = oldEnable;
            
            if (clicked)
            {
                editorSetting.databaseIdExploring = ipfDatabaseIdText;
                SGEditorSettingDI.Save(editorSetting);
                isExploring = true;
                dictNodeDrawer.Clear();
                currentDatabase = null;
                // if(SaveGame.IsLoggedIn(editorSetting.databaseIdExploring))
                //     SaveGame.Logout(editorSetting.databaseIdExploring);
            }
        }

        void DrawNodeTree()
        {
            if(string.IsNullOrEmpty(editorSetting.databaseIdExploring))
                return;
            
            if (SaveGame.IsLoggedIn(editorSetting.databaseIdExploring) == false)
                SaveGame.Login(editorSetting.databaseIdExploring);

            if (currentDatabase == null)
                currentDatabase = SaveGame.GetDatabaseLoggedIn(editorSetting.databaseIdExploring);

            var listChildNodeExactOrder = currentDatabase.GetEnumerableExactOrder().ToList();
            foreach (var sgNode in listChildNodeExactOrder)
            {
                if (dictNodeDrawer.TryGetValue(sgNode, out var nodeDrawer))
                    nodeDrawer.OnGUI();
                else
                {
                    nodeDrawer = new NodeDrawer(dictNodeDrawer, sgNode);
                    dictNodeDrawer.Add(sgNode, nodeDrawer);
                    nodeDrawer.OnGUI();
                }

                GUILayout.Space(10);
            }
        }

        void DrawToolBar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawAddChild();
            DrawDelete();
            
            GUILayout.EndHorizontal();
        }

        void DrawAddChild()
        {
            if(GUILayout.Button("Add child", GUILayout.Width(75)))
            {
                Rect rect = new Rect();
                rect.position = Event.current.mousePosition;
                PopupWindow.Show(rect, new PopupAddChild(currentDatabase));
            }
        }

        void DrawDelete()
        {
            Color oldBgColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
            if(GUILayout.Button("X", GUILayout.Width(20)))
            {
                Rect rect = new Rect();
                rect.position = Event.current.mousePosition;
                PopupWindow.Show(rect, new PopupAskDeleteNode(currentDatabase));
            }

            GUI.backgroundColor = oldBgColor;
        }
    }
}