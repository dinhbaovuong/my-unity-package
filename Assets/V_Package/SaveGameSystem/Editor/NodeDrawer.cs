using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VPackage.SaveGameSystem.EditorNS
{
    public class NodeDrawer
    {
        private Dictionary<SGNode, NodeDrawer> dictNodeDrawer;
        public SGNode node;

        private bool showChild = false;
        public bool wantDelete;

        public NodeDrawer(Dictionary<SGNode, NodeDrawer> dictNodeDrawer, SGNode node)
        {
            this.dictNodeDrawer = dictNodeDrawer;
            this.node = node;
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUILayout.BeginHorizontal();
            
            showChild = EditorGUILayout.Foldout(showChild, node.Key, true);
            GUILayout.Space(70);
            DrawNodeType();
            
            if (node.NodeType != NodeType.Object)
                NodeTypeHandler.DrawNodeValueField(node);
            else
            {
                DrawDuplicate();
                DrawAddChild();
            }
                
            
            DrawDelete();
            
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
            


            if (showChild && node.NodeType == NodeType.Object)
            {
                DrawChild();
            }
            
            GUILayout.EndVertical();
        }

        void DrawNodeType()
        {
            NodeType newNodeType = (NodeType)EditorGUILayout.EnumPopup(node.NodeType, GUILayout.Width(85));
            
            if (newNodeType != node.NodeType)
                node.ChangeNodeType_Editor(newNodeType);
        }

        void DrawChild()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical();

            var listChildNodeExactOrder = node.GetEnumerableExactOrder().ToList();
            foreach (var childNode in listChildNodeExactOrder)
            {
                if (dictNodeDrawer.TryGetValue(childNode, out var childNodeDrawer))
                    childNodeDrawer.OnGUI();
                else
                {
                    childNodeDrawer = new NodeDrawer(dictNodeDrawer, childNode);
                    dictNodeDrawer.Add(childNode, childNodeDrawer);
                    childNodeDrawer.OnGUI();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        void DrawAddChild()
        {
            if(GUILayout.Button("+", GUILayout.Width(25)))
            {
                Rect rect = new Rect();
                rect.position = Event.current.mousePosition;
                PopupWindow.Show(rect, new PopupAddChild(node));
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
                PopupWindow.Show(rect, new PopupAskDeleteNode(node));
            }

            GUI.backgroundColor = oldBgColor;
        }

        void DrawDuplicate()
        {
            if(GUILayout.Button("D", GUILayout.Width(20)))
            {
                Rect rect = new Rect();
                rect.position = Event.current.mousePosition;
                PopupWindow.Show(rect, new PopupDuplicate(node));
            }
        }
    }
}