using UnityEditor;
using UnityEngine;

namespace VPackage.SaveGameSystem.EditorNS
{
    public class PopupAddChild : PopupWindowContent
    {
        private string ipfKeyText = "";
        private SGNode node;
        private Database database;

        public PopupAddChild(SGNode node)
        {
            this.node = node;
        }
        
        public PopupAddChild(Database database)
        {
            this.database = database;
        }
        
        public override Vector2 GetWindowSize()
        {
            return new Vector2(200, 75);
        }
        
        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginVertical();
            var headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Space(5);
            GUILayout.Label("Add new child", headerStyle);
            GUILayout.Space(5);
            
            Color oldBgColor = GUI.backgroundColor;

            if (node != null)
            {
                if (node.HasChild(ipfKeyText))
                    GUI.backgroundColor = Color.red;
            }
            else if (database != null)
            {
                if (database.HasChild(ipfKeyText))
                    GUI.backgroundColor = Color.red;
            }
            
            GUI.SetNextControlName("text field new child key");
            ipfKeyText = GUILayout.TextField(ipfKeyText);
            ipfKeyText = ipfKeyText.Replace(" ", "_");
            EditorGUI.FocusTextInControl("text field new child key");

            GUI.backgroundColor = oldBgColor;
            
            
            bool clicked = false;
            if (string.IsNullOrEmpty(ipfKeyText) == false)
            {
                bool oldEnabled = GUI.enabled;

                if (node != null)
                {
                    if (node.HasChild(ipfKeyText))
                        GUI.enabled = false;
                }
                else if (database != null)
                {
                    if (database.HasChild(ipfKeyText))
                        GUI.enabled = false;
                }
                
                
                clicked = GUILayout.Button("Add");

                GUI.enabled = oldEnabled;
            }
            else
            {
                clicked = GUILayout.Button("Add with auto key");
            }
            
            
            
            GUILayout.EndVertical();
            
            
            if (clicked)
            {
                ConfirmAdd();
            }

            bool pressEnter = Event.current.isKey && Event.current.keyCode == KeyCode.Return;
            if (pressEnter)
            {
                //Debug.Log("Press Enter");
                ConfirmAdd();
            }
        }

        void ConfirmAdd()
        {
            if (string.IsNullOrEmpty(ipfKeyText))
            {
                if(node != null)
                    node.AddChildValue(0);
                else if (database != null)
                    database.AddChildValue(0);
            }
            else
            {
                if (node != null)
                {
                    node.AddChild(ipfKeyText);
                }
                else if (database != null)
                    database.AddChild(ipfKeyText);
                        
            }

            ipfKeyText = "";
                
            editorWindow.Close();
        }
    }
}