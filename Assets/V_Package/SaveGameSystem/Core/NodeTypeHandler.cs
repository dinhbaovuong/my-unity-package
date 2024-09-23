using UnityEngine;
using VPackage.SaveGameSystem.Security;

namespace VPackage.SaveGameSystem
{
    public static class NodeTypeHandler
    {
        public static void ImportJChildValueIntoNode(SGJsonNode jChildValue, SGNode node, NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.Undefined:
                {
                    break;
                }
                case NodeType.String:
                {
                    node.stringValue.Value = jChildValue.Value;
                    break;
                }
                case NodeType.Int:
                {
                    node.numberValue = new EInt(jChildValue.AsInt);
                    break;
                }
                case NodeType.Float:
                {
                    node.numberValue = new EFloat(jChildValue.AsFloat);
                    break;
                }
                case NodeType.Long:
                {
                    node.numberValue = new ELong(jChildValue.AsLong);
                    break;
                }
                case NodeType.Decimal:
                {
                    node.numberValue = new EDecimal(jChildValue.AsDecimal);
                    break;
                }
                case NodeType.Bool:
                {
                    node.numberValue = new EInt(jChildValue.AsInt);
                    break;
                }
                default:
                {
                    Debug.LogError("Chưa code cho value type này");
                    break;
                }
            }
        }

        public static void ImportNodeIntoJChild(SGNode node, SGJsonNode jChild)
        {
            switch (node.NodeType)
            {
                case NodeType.String:
                {
                    jChild[KeyDefine.value] = node.stringValue.Value;
                    break;
                }
                case NodeType.Int:
                {
                    jChild[KeyDefine.value] = ((EInt)node.numberValue).Value;
                    break;
                }
                case NodeType.Float:
                {
                    jChild[KeyDefine.value] = ((EFloat)node.numberValue).Value;
                    break;
                }
                case NodeType.Long:
                {
                    jChild[KeyDefine.value] = ((ELong)node.numberValue).Value;
                    break;
                }
                case NodeType.Decimal:
                {
                    jChild[KeyDefine.value] = ((EDecimal)node.numberValue).Value;
                    break;
                }
                case NodeType.Bool:
                {
                    jChild[KeyDefine.value] = ((EInt)node.numberValue).Value;
                    break;
                }
                default:
                {
                    Debug.LogError("Chưa code cho value type này: " + node.NodeType);
                    break;
                }
            }
        }


        #region Editor
#if UNITY_EDITOR
        public static decimal GetNumberValue(object numberValue)
        {
            if (numberValue is EInt)
                return ((EInt)numberValue).Value;
            
            if (numberValue is EFloat)
                return (decimal)((EFloat)numberValue).Value;
            
            if (numberValue is ELong)
                return ((ELong)numberValue).Value;
            
            if (numberValue is EDecimal)
                return ((EDecimal)numberValue).Value;
            
            Debug.LogError("Chưa code EType này: " + numberValue.GetType().Name);
            return 0;
        }
        
        public static object NodeTypeToEType(NodeType nodeType, decimal numberValue)
        {
            switch (nodeType)
            {
                case NodeType.Undefined:
                {
                    return null;
                }
                case NodeType.String:
                {
                    return new EString("");
                }
                case NodeType.Int:
                {
                    return new EInt((int)numberValue);
                }
                case NodeType.Float:
                {
                    return new EFloat((float)numberValue);
                }
                case NodeType.Long:
                {
                    return new ELong((long)numberValue);
                }
                case NodeType.Decimal:
                {
                    return new EDecimal((decimal)numberValue);
                }
                case NodeType.Bool:
                {
                    return new EInt((int)numberValue);
                }
                default:
                {
                    Debug.LogError("Chưa code cho value type này");
                    return null;
                }
            }
        }
        
        public static void DrawNodeValueField(SGNode node)
        {
            var nodeType = node.NodeType;
            switch (nodeType)
            {
                case NodeType.Undefined:
                {
                    break;
                }
                case NodeType.Object:
                {
                    break;
                }
                case NodeType.String:
                {
                    var newValue = UnityEditor.EditorGUILayout.TextField(node.StringValue);
                    if (newValue != node.StringValue)
                    {
                        node.StringValue = newValue;
                    }
                    break;
                }
                case NodeType.Int:
                {
                    var newValue = UnityEditor.EditorGUILayout.IntField(node.IntValue);
                    if (newValue != node.IntValue)
                    {
                        node.IntValue = newValue;
                    }
                    break;
                }
                case NodeType.Float:
                {
                    var newValue = UnityEditor.EditorGUILayout.FloatField(node.FloatValue);
                    if (newValue != node.FloatValue)
                    {
                        node.FloatValue = newValue;
                    }
                    break;
                }
                case NodeType.Long:
                {
                    var newValue = UnityEditor.EditorGUILayout.LongField(node.LongValue);
                    if (newValue != node.LongValue)
                    {
                        node.LongValue = newValue;
                    }
                    break;
                }
                case NodeType.Decimal:
                {
                    decimal newValue = (decimal)UnityEditor.EditorGUILayout.FloatField((float)node.DecimalValue);
                    if (newValue != node.DecimalValue)
                    {
                        node.DecimalValue = newValue;
                    }
                    break;
                }
                case NodeType.Bool:
                {
                    int selectedIndex = node.BoolValue ? 1 : 0;
                    int newSelectedIndex = UnityEditor.EditorGUILayout.Popup(selectedIndex, new []{"False", "True"});
                    if (newSelectedIndex != selectedIndex)
                    {
                        node.BoolValue = newSelectedIndex != 0;
                    }
                    break;
                }
                default:
                {
                    UnityEditor.EditorGUILayout.LabelField("No code for this value type yet");
                    break;
                }
            }
        }

        public static void CopyNodeValue(SGNode fromNode, SGNode toNode)
        {
            switch (fromNode.NodeType)
            {
                case NodeType.Undefined:
                case NodeType.Object:
                {
                    break;
                }
                case NodeType.String:
                {
                    toNode.StringValue = fromNode.StringValue;
                    break;
                }
                case NodeType.Int:
                {
                    toNode.IntValue = fromNode.IntValue;
                    break;
                }
                case NodeType.Float:
                {
                    toNode.FloatValue = fromNode.FloatValue;
                    break;
                }
                case NodeType.Long:
                {
                    toNode.LongValue = fromNode.LongValue;
                    break;
                }
                case NodeType.Decimal:
                {
                    toNode.DecimalValue = fromNode.DecimalValue;
                    break;
                }
                case NodeType.Bool:
                {
                    toNode.BoolValue = fromNode.BoolValue;
                    break;
                }
                default:
                {
                    Debug.LogError("Chưa code cho nodeType này: " + fromNode.NodeType);
                    break;
                }
            }
        }
#endif
        
        #endregion
    }
}