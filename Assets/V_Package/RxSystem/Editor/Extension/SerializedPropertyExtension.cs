using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

namespace VPackage.RxSystem.Editors
{
    public static class SerializedPropertyExtension
    {
        static BindingFlags bindingFlags = BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        public static object GetObject(this SerializedProperty serializeProperty)
        {
            var listField = serializeProperty.propertyPath.Split('.');
            Type targetObjectType = serializeProperty.serializedObject.targetObject.GetType();
            
            Type parentType = targetObjectType;
            object parentObject = serializeProperty.serializedObject.targetObject;

            for (int i = 0; i < listField.Length; i++)
            {
                string fieldName = listField[i];
                bool isList = i + 1 < listField.Length && listField[i] == "Array" && listField[i + 1].StartsWith("data[");
                
                if (isList) //có nghĩa fieldInfo hiện tại là list
                {
                    var data = listField[i + 1]; //data[xxx];
                    int index = int.Parse(data.Substring(5, data.Length - 6));

                    IList list = (IList)parentObject;

                    parentObject = list[index];

                    if (parentObject == null)
                        return null;
                    
                    parentType = parentObject.GetType();
                    i++;
                }
                else
                {
                    var fi = parentType.GetField(fieldName, bindingFlags);
                    var obj = fi.GetValue(parentObject);

                    parentType = fi.FieldType;
                    parentObject = obj;
                }
            }

            return parentObject;
        }

    }
}