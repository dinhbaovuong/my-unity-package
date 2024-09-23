using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VPackage.RxSystem.Editors
{
    [CustomPropertyDrawer(typeof(VariableRx<>), true)]
    public class VariableRxEditor : PropertyDrawer
    {
        private bool needEmit = false;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (needEmit)
            {
                needEmit = false;
                
                if(EditorApplication.isPlaying)
                    EmitValueChange(property);
            }
            
            var valueSp = property.FindPropertyRelative("value");
            
            EditorGUI.BeginChangeCheck();
            
            EditorGUI.PropertyField(position, valueSp, label);
            
            if (EditorGUI.EndChangeCheck())
            {
                needEmit = true;
                // if(EditorApplication.isPlaying)
                //     EmitValueChange(property);
            }
        }

        void EmitValueChange(SerializedProperty serializedProperty)
        {
            var variableRxObject = serializedProperty.GetObject();
            if (variableRxObject == null)
            {
                Debug.LogError("Ủa sao get object của serializedProperty lại null nhỉ");
                return;
            }
            
            BindingFlags bindingFlags = BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var variableRxType = variableRxObject.GetType();
            
            //Get variable value
            var valueFi = variableRxType.BaseType.GetField("value", bindingFlags);
            var value = valueFi.GetValue(variableRxObject);
            
            //Get Subject Object
            var subjectFi = variableRxType.BaseType.GetField("subject", bindingFlags);
            var subject = subjectFi.GetValue(variableRxObject);
            
            //Emit
            var subjectType = subject.GetType();
            var emitMi = subjectType.GetMethod("Emit", bindingFlags);
            emitMi.Invoke(subject, new[] {value});
        }
        
        //Value bị chậm :)))
    }
}