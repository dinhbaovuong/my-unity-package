using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VPackage.DataTableSystem.SoTableSystem
{
    [System.Serializable]
    public abstract class SoTable<TTable, TRow> : ScriptableObject
        where TTable : SoTable<TTable, TRow>, new()
        where TRow : new()
    {
        private static TTable m_instance;
        [SerializeField] private List<TRow> m_listRow = new List<TRow>();
        
#if UNITY_EDITOR
        [System.NonSerialized] private List<TRow> m_listRowEditor;
#endif

#if UNITY_EDITOR
        protected List<TRow> listRow
        {
            get => m_listRowEditor;
            set => m_listRowEditor = value;
        }
#else
        protected List<TRow> listRow
        {
            get => m_listRow;
            set => m_listRow = value;
        }
#endif
        
        protected static TTable Instance
        {
            get
            {
                if (m_instance == null)
                {
                    TTable tableTemp = ScriptableObject.CreateInstance<TTable>();
                    m_instance = tableTemp.LoadInstance();
                    Destroy(tableTemp);
                    if(m_instance == null)
                        Debug.LogError("Get Instance failed!");
                }

                return m_instance;
            }
        }

        protected abstract string InstancePath { get; }
        
        protected virtual TTable LoadInstance()
        {
            return Resources.Load<TTable>(InstancePath);
        }
        
        public static List<TRow> Rows
        {
            get
            {
                if (Instance.listRow == null)
                {
                    //Init table
                    var listRow = Instance.LoadListRow();
                    Instance.SetListRow(listRow);
                    
                    if (Instance.listRow == null)
                    {
                        Debug.LogError("InitTable failed, table not inited!");
                        return null;
                    }
                }

                return Instance.listRow;
            }
        }

        protected virtual void SetListRow(List<TRow> rows)
        {
            listRow = rows;
        }

        protected virtual List<TRow> LoadListRow()
        {
#if UNITY_EDITOR
            return Instance.m_listRow.ToList(); //nếu lấy từ listrow thì trên editor sẽ là static
#else
            return Instance.m_listRow;
#endif
        }
        
        /// <summary>
        /// Load list row after set listrow
        /// </summary>
        // protected void InitTable()
        // {
        //     var listRow = LoadListRow();
        //     SetListRow(listRow);
        // }

        public static void InitTable(List<TRow> rows)
        {
            Instance.SetListRow(rows);
        }
        
        public static void CacheTable()
        {
            var r = Rows;
        }

        #if UNITY_EDITOR
        private void ImportFromCsvData(List<string> listField, List<List<string>> listRowData)
        {
            var rowType = typeof(TRow);
            if(rowType.IsSubclassOf(typeof(ScriptableObject)) == false)
                ImportFromCsvDataWithRowNonSO(listField, listRowData);
            else
            {
                ImportFromCsvDataWithRowSO(listField, listRowData);
            }
        }

        private void ImportFromCsvDataWithRowNonSO(List<string> listField, List<List<string>> listRowData)
        {
            UnityEditor.Undo.RecordObject(this, "ImportFromCsvData");
            List<TRow> newListRow = new List<TRow>();
            var rowType = typeof(TRow);
            BindingFlags bindingFlags = BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;


            //Check các field cái đã
            foreach (var field in listField)
            {
                var fieldInfo = rowType.GetField(field, bindingFlags);
                if (fieldInfo == null)
                {
                    Debug.LogErrorFormat("Field {0} not exist", field);
                }
            }
            
            
            for (int i = 0; i < listRowData.Count; i++)
            {
                var rowData = listRowData[i];
                TRow row = new TRow();
                newListRow.Add(row);
                
                for (int j = 0; j < rowData.Count; j++)
                {
                    string cellValue = rowData[j];
                    if(string.IsNullOrEmpty(cellValue))
                        continue;
                    
                    var fieldInfo = rowType.GetField(listField[j], bindingFlags);
                    if (fieldInfo == null)
                        continue;

                    SetFieldInfoValue(fieldInfo, row, cellValue, i + 2);
                }
            }
            
            m_listRow.Clear();
            m_listRow.AddRange(newListRow);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        private void ImportFromCsvDataWithRowSO(List<string> listField, List<List<string>> listRowData)
        {
            UnityEditor.Undo.RecordObject(this, "ImportFromCsvData");
            List<TRow> newListRow = new List<TRow>();
            var rowType = typeof(TRow);
            BindingFlags bindingFlags = BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;


            //Check các field cái đã
            foreach (var field in listField)
            {
                var fieldInfo = rowType.GetField(field, bindingFlags);
                if (fieldInfo == null)
                {
                    Debug.LogErrorFormat("Field {0} not exist", field);
                }
            }

            string tableAssetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            string tableAssetFolderPath = Path.GetDirectoryName(tableAssetPath);
            string tableAssetName = Path.GetFileName(tableAssetPath);

            string rowAssetFolderName = tableAssetName.Replace(".asset", "").Replace("Table", "Row").Replace("table", "row");
            if (rowAssetFolderName.Contains("Row") == false && rowAssetFolderName.Contains("row") == false)
                rowAssetFolderName += " Row";

            string rowAssetFolderPath = Path.Combine(tableAssetFolderPath, rowAssetFolderName);
            
            if (UnityEditor.AssetDatabase.IsValidFolder(rowAssetFolderPath) == false)
                UnityEditor.AssetDatabase.CreateFolder(tableAssetFolderPath, rowAssetFolderName);
            
            for (int i = 0; i < listRowData.Count; i++)
            {
                var rowData = listRowData[i];
                
                string rowAssetPath = Path.Combine(rowAssetFolderPath, "row_" + i + ".asset");
                TRow row = (TRow)(object)UnityEditor.AssetDatabase.LoadAssetAtPath(rowAssetPath, rowType);
                if (row == null)
                {
                    row = (TRow)(object)ScriptableObject.CreateInstance(typeof(TRow));
                    UnityEditor.AssetDatabase.CreateAsset((Object)(object)row, rowAssetPath);
                }
                
                newListRow.Add(row);
                
                for (int j = 0; j < rowData.Count; j++)
                {
                    string cellValue = rowData[j];
                    if(string.IsNullOrEmpty(cellValue))
                        continue;
                    
                    var fieldInfo = rowType.GetField(listField[j], bindingFlags);
                    if (fieldInfo == null)
                        continue;

                    SetFieldInfoValue(fieldInfo, row, cellValue, i + 2);
                }
            }
            
            m_listRow.Clear();
            m_listRow.AddRange(newListRow);

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        void SetFieldInfoEnumValue(FieldInfo fieldInfo, object obj, string cellValue, int rowInSheet)
        {
            var fieldType = fieldInfo.FieldType;
            
            //Thử vỡi chữ trước
            try
            {
                var enumObj = Enum.Parse(fieldType, cellValue, true);
                fieldInfo.SetValue(obj, enumObj);
                return;
            }
            catch (Exception e)
            { }

            //Sau đó thử với số
            try
            {
                int number = int.Parse(cellValue);
                string nameToParse = Enum.GetName(fieldType, number);
                var enumObj = Enum.Parse(fieldType, nameToParse, true);
                fieldInfo.SetValue(obj, enumObj);
                return;
            }
            catch (Exception e)
            { }

            
            //Định dạng lại chữ
            if (string.IsNullOrEmpty(cellValue) == false)
            {
                //Định dạng lại chữ và thử lại với chữ
                StringBuilder sb = new StringBuilder();

                bool needUpperCase = true; //Để upper case ký tự đầu
                for (int i = 0; i < cellValue.Length; i++)
                {
                    char c = cellValue[i];
                    if (c == '_' || c == ' ')
                    {
                        needUpperCase = true;
                        continue;
                    }
                    
                    if (needUpperCase)
                    {
                        sb.Append(c.ToString().ToUpper());
                        needUpperCase = false;
                    }
                    else
                        sb.Append(c);
                }

                cellValue = sb.ToString();
            }
            
            
            //Thử lại với chữ đã đc định dạng
            try
            {
                var enumObj = Enum.Parse(fieldType, cellValue, true);
                fieldInfo.SetValue(obj, enumObj);
                return;
            }
            catch (Exception e)
            { }
            
            
            Debug.LogErrorFormat("Field {0} có kiểu là {1} nhưng data để import lại không phải - rowInSheet: {2}", fieldInfo.Name, fieldType.Name, rowInSheet);
        }
        
        void SetFieldInfoValue(FieldInfo fieldInfo, object obj, string cellValue, int rowInSheet)
        {
            var fieldType = fieldInfo.FieldType;
            
            if (fieldType == typeof(string))
            {
                fieldInfo.SetValue(obj, cellValue);
                return;
            }
            
            if (fieldType == typeof(char))
            {
                fieldInfo.SetValue(obj, cellValue[0]);
                return;
            }

            if (fieldType.IsEnum)
            {
                SetFieldInfoEnumValue(fieldInfo, obj, cellValue, rowInSheet);
                return;
            }
            
            

            if (fieldType == typeof(bool))
            {
                if(cellValue.Equals("true", StringComparison.OrdinalIgnoreCase))
                    fieldInfo.SetValue(obj, true);
                else if(cellValue.Equals("false", StringComparison.OrdinalIgnoreCase))
                    fieldInfo.SetValue(obj, false);

                if (int.TryParse(cellValue, out int number) == false)
                {
                    Debug.LogErrorFormat("Field {0} có kiểu là bool nhưng data để import lại không phải - rowInSheet: {1}", fieldInfo.Name, rowInSheet);
                }
                else if(number != 0)
                    fieldInfo.SetValue(obj, true);
                else
                    fieldInfo.SetValue(obj, false);
                
                return;
            }
            

            //Kiểu số nguyên
            if (fieldType == typeof(byte))
            {
                if(byte.TryParse(cellValue, out var number) == false)
                    Debug.LogErrorFormat("Field {0} có kiểu là byte nhưng data để import lại không phải - rowInSheet: {1}", fieldInfo.Name, rowInSheet);
                else
                    fieldInfo.SetValue(obj, number);
                
                return;
            }
            
            if (fieldType == typeof(sbyte))
            {
                if(sbyte.TryParse(cellValue, out var number) == false)
                    Debug.LogErrorFormat("Field {0} có kiểu là sbyte nhưng data để import lại không phải - rowInSheet: {1}", fieldInfo.Name, rowInSheet);
                else
                    fieldInfo.SetValue(obj, number);
                
                return;
            }
            
            if (fieldType == typeof(short))
            {
                if(short.TryParse(cellValue, out var number) == false)
                    Debug.LogErrorFormat("Field {0} có kiểu là short nhưng data để import lại không phải - rowInSheet: {1}", fieldInfo.Name, rowInSheet);
                else
                    fieldInfo.SetValue(obj, number);
                
                return;
            }
            
            if (fieldType == typeof(ushort))
            {
                if(ushort.TryParse(cellValue, out var number) == false)
                    Debug.LogErrorFormat("Field {0} có kiểu là ushort nhưng data để import lại không phải - rowInSheet: {1}", fieldInfo.Name, rowInSheet);
                else
                    fieldInfo.SetValue(obj, number);
                
                return;
            }
            
            if (fieldType == typeof(int))
            {
                if(int.TryParse(cellValue, out var number) == false)
                    Debug.LogErrorFormat("Field {0} có kiểu là int nhưng data để import lại không phải - rowInSheet: {1}", fieldInfo.Name, rowInSheet);
                else
                    fieldInfo.SetValue(obj, number);
                
                return;
            }
            
            if (fieldType == typeof(uint))
            {
                if(uint.TryParse(cellValue, out var number) == false)
                    Debug.LogErrorFormat("Field {0} có kiểu là uint nhưng data để import lại không phải - rowInSheet: {1}", fieldInfo.Name, rowInSheet);
                else
                    fieldInfo.SetValue(obj, number);
                
                return;
            }
            
            if (fieldType == typeof(long))
            {
                if(long.TryParse(cellValue, out var number) == false)
                    Debug.LogErrorFormat("Field {0} có kiểu là long nhưng data để import lại không phải - rowInSheet: {1}", fieldInfo.Name, rowInSheet);
                else
                    fieldInfo.SetValue(obj, number);
                
                return;
            }
            
            if (fieldType == typeof(ulong))
            {
                if(ulong.TryParse(cellValue, out var number) == false)
                    Debug.LogErrorFormat("Field {0} có kiểu là ulong nhưng data để import lại không phải - rowInSheet: {1}", fieldInfo.Name, rowInSheet);
                else
                    fieldInfo.SetValue(obj, number);
                
                return;
            }
            
            //Kiểu số thực
            if (fieldType == typeof(float))
            {
                try
                {
                    var number = float.Parse(cellValue, CultureInfo.InvariantCulture.NumberFormat);
                    fieldInfo.SetValue(obj, number);
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Field {0} có kiểu là float nhưng data để import lại không phải - rowInSheet: {1}", fieldInfo.Name, rowInSheet);
                }
                
                return;
            }
            
            if (fieldType == typeof(double))
            {
                try
                {
                    var number = double.Parse(cellValue, CultureInfo.InvariantCulture.NumberFormat);
                    fieldInfo.SetValue(obj, number);
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Field {0} có kiểu là double nhưng data để import lại không phải - rowInSheet: {1}", fieldInfo.Name, rowInSheet);
                }
                
                return;
            }
            
            if (fieldType == typeof(decimal))
            {
                try
                {
                    var number = decimal.Parse(cellValue, CultureInfo.InvariantCulture.NumberFormat);
                    fieldInfo.SetValue(obj, number);
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Field {0} có kiểu là decimal nhưng data để import lại không phải - rowInSheet: {1}", fieldInfo.Name, rowInSheet);
                }
                
                return;
            }
            
            //Kiểu Object
            try
            {
                object objFromJson = JsonConvert.DeserializeObject(cellValue, fieldType);
                fieldInfo.SetValue(obj, objFromJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"Json sai định dạng ở field {fieldInfo.Name}, rowInSheet: {rowInSheet}");
            }
            return;
            //Debug.LogError("Không hỗ trợ import field có kiểu là: " + fieldInfo.Name);
        }
#endif
    }
}