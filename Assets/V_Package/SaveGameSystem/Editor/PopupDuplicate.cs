using UnityEditor;
using UnityEngine;

namespace VPackage.SaveGameSystem.EditorNS
{
    public class PopupDuplicate : PopupWindowContent
    {
        private string ipfKeyText = "";
        private SGNode node;
        
        public PopupDuplicate(SGNode node)
        {
            this.node = node;
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
            GUILayout.Label("Duplicate", headerStyle);
            GUILayout.Space(5);
            
            Color oldBgColor = GUI.backgroundColor;

            if (node.Parent == null)
            {
                if (node.Database.HasChild(ipfKeyText))
                    GUI.backgroundColor = Color.red;
            }
            else
            {
                if (node.Parent.HasChild(ipfKeyText))
                    GUI.backgroundColor = Color.red;
            }
            
            
            GUI.SetNextControlName("text field new child key");
            ipfKeyText = GUILayout.TextField(ipfKeyText);
            ipfKeyText = ipfKeyText.Replace(" ", "_");
            EditorGUI.FocusTextInControl("text field new child key");

            GUI.backgroundColor = oldBgColor;

            bool oldEnable = GUI.enabled;
            if (node.Parent == null)
                GUI.enabled = node.Database.HasChild(ipfKeyText) == false && string.IsNullOrEmpty(ipfKeyText) == false;
            else
                GUI.enabled = node.Parent.HasChild(ipfKeyText) == false && string.IsNullOrEmpty(ipfKeyText) == false;

            bool clicked = GUILayout.Button("Duplicate");
            GUI.enabled = oldEnable;
            
            GUILayout.EndVertical();


            if (clicked)
            {
                editorWindow.Close();
                
                if (node.Parent == null)
                {
                    SGNode newNode = node.Database[ipfKeyText];
                    CopyNode(node, newNode);
                }
                else
                {
                    SGNode newNode = node.Parent[ipfKeyText];
                    CopyNode(node, newNode);
                }
            }
        }

        void CopyNode(SGNode fromNode, SGNode toNode)
        {
            if (fromNode.ChildCount > 0)
            {
                foreach (var fromChildNode in fromNode)
                {
                    var toChildNode = toNode[fromChildNode.Key];
                    CopyNode(fromChildNode, toChildNode);
                }
            }
            
            toNode.ChangeNodeType_Editor(fromNode.NodeType);
            NodeTypeHandler.CopyNodeValue(fromNode, toNode);
        }
    }
}