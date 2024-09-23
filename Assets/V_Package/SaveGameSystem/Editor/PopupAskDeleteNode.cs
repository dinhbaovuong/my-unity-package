using UnityEditor;
using UnityEngine;

namespace VPackage.SaveGameSystem.EditorNS
{
    public class PopupAskDeleteNode : PopupWindowContent
    {
        private SGNode node;
        private Database database;

        public PopupAskDeleteNode(SGNode node)
        {
            this.node = node;
        }
        
        public PopupAskDeleteNode(Database database)
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
            GUILayout.Label("Do you want delete?", headerStyle);
            GUILayout.Space(5);
            
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Yes"))
            {
                editorWindow.Close();
                
                if (node != null)
                {
                    node.Delete();
                }
                else if(database != null)
                {
                    database.DeleteAllChild();
                }
            }

            if (GUILayout.Button("No"))
            {
                editorWindow.Close();
            }

            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }
    }
}