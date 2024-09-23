using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VPackage.DataTableSystem.EditorNS
{
    public class ImportSOTableEditorWindow : EditorWindow
    {
        private static readonly string soTableNameSpace = "VPackage.DataTableSystem.SoTableSystem";
        private static readonly string soTableName = "SoTable`2";
        
        
        private ScriptableObject tableAsset;
        private string filePath = "";
        private Editor tableAssetEditor;
        private bool showTableAssetEditor = false;
        private Vector2 scrollPos = Vector2.zero;

        private readonly string[] listFieldSeparator = new string[]
        {
            ",", ";", "|"
        };
        
        private readonly string[] listQuoteCharacter = new string[]
        {
            "\"", "'"
        };

        private int fieldSeparatorIndexSelected = 0;
        private int quoteCharacterIndexSelected;
        private bool trimField = true;

        private BindingFlags bindingFlags = BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        //Data Analyzed
        private List<string> listCsvColumn = new List<string>();
        private List<string> listField = new List<string>();
        private List<List<string>> listCsvRow = new List<List<string>>();
        
        //Convert Setting
        private bool removeWordSeparator = true;
        private bool upperCaseAfterWordSeparator = true;


        [MenuItem("Tools/VPackage/Data Table System/Import SO Table")]
        public static ImportSOTableEditorWindow OpenWindow()
        {
            var window = GetWindow<ImportSOTableEditorWindow>();
            window.titleContent = new GUIContent("Import SO Table");
            return window;
        }

        bool IsInheritFromSOTable(Type type)
        {
            if (type == null)
                return false;

            var soType = typeof(ScriptableObject);

            if (type.IsSubclassOf(soType) == false)
                return false;
            
            Type currentType = type;
            while (true)
            {
                var baseType = currentType.BaseType;
                
                if (baseType == null)
                    return false;

                if (baseType == soType)
                {
                    if (currentType.Namespace == soTableNameSpace && currentType.Name == soTableName)
                        return true;
                    else
                        return false;
                }

                currentType = baseType;
            }
        }

        Type GetSOTableType(Type type)
        {
            if (type == null)
                return null;

            var soType = typeof(ScriptableObject);

            if (type.IsSubclassOf(soType) == false)
                return null;
            
            Type currentType = type;
            while (true)
            {
                var baseType = currentType.BaseType;
                
                if (baseType == null)
                    return null;

                if (baseType == soType)
                {
                    if (currentType.Namespace == soTableNameSpace && currentType.Name == soTableName)
                        return currentType;
                    else
                        return null;
                }

                currentType = baseType;
            }
        }

        private void OnGUI()
        {
            //GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            //titleStyle.alignment = TextAnchor.MiddleLeft;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
            GUILayout.Space(15);
            DrawChooseTableAsset();

            if (tableAsset == null)
            {
                EditorGUILayout.EndScrollView();
                return;
            }
                
            
            DrawChooseFile();
            if (string.IsNullOrEmpty(filePath) == false)
            {
                GUILayout.Space(15);
                DrawSetting();
                
                GUILayout.Space(15);
                
                DrawButtonAnalytics();
                DrawButtonImport();
                
                if (listCsvColumn.Count > 0)
                {
                    GUILayout.Space(15);
                    DrawColumnAndField();
                }
            }
            
            GUILayout.Space(15);
            DrawTableAssetEditor();
            
            EditorGUILayout.EndScrollView();
        }

        void DrawChooseTableAsset()
        {
            var newTableAsset = (ScriptableObject)EditorGUILayout.ObjectField("Table Asset", tableAsset, typeof(ScriptableObject), false);
            if (newTableAsset != tableAsset)
            {
                ClearDataAnalyzed();
                
                if (newTableAsset == null)
                {
                    tableAsset = null;
                    tableAssetEditor = null;
                }
                else
                {
                    if (IsInheritFromSOTable(newTableAsset.GetType()))
                    {
                        tableAsset = newTableAsset;
                        tableAssetEditor = Editor.CreateEditor(tableAsset);
                    }
                }
            }
        }

        void DrawChooseFile()
        {
            GUILayout.BeginHorizontal();
            filePath = EditorGUILayout.TextField("File CSV", filePath);

            bool clicked = GUILayout.Button("Choose CSV File", GUILayout.Width(120));
            GUILayout.EndHorizontal();

            if (clicked)
            {
                string lastFolder = "";
                if (string.IsNullOrWhiteSpace(filePath) == false)
                {
                    string directory = Path.GetDirectoryName(filePath);
                    if (string.IsNullOrWhiteSpace(directory) == false)
                    {
                        if (Directory.Exists(directory))
                            lastFolder = directory;
                    }
                }
                
                string newFilePath = EditorUtility.OpenFilePanel("Choose file csv", lastFolder, "csv");
                if (string.IsNullOrEmpty(newFilePath) == false && newFilePath != filePath)
                {
                    filePath = newFilePath;
                    ClearDataAnalyzed();
                }
                    
            }
        }

        void ClearDataAnalyzed()
        {
            listCsvColumn.Clear();
            listCsvRow.Clear();
            listField.Clear();
        }

        void DrawSetting()
        {
            GUILayout.Label("Import Settings", EditorStyles.boldLabel);
            fieldSeparatorIndexSelected = EditorGUILayout.Popup("Field Separator", fieldSeparatorIndexSelected, listFieldSeparator);
            quoteCharacterIndexSelected = EditorGUILayout.Popup("Quote Character", quoteCharacterIndexSelected, listQuoteCharacter);
            trimField = EditorGUILayout.Toggle("Trim Field", trimField);
        }

        void DrawButtonAnalytics()
        {
            if (GUILayout.Button("Analytics"))
            {
                ClearDataAnalyzed();
                
                if (File.Exists(filePath) == false)
                {
                    Debug.LogError("File csv not exist!");
                    return;
                }

                char fieldSeparator = listFieldSeparator[fieldSeparatorIndexSelected][0];
                char quoteCharacter = listQuoteCharacter[quoteCharacterIndexSelected][0];
                var listRow = CsvUtility.ImportFromCSV(File.ReadAllText(filePath), fieldSeparator, quoteCharacter, trimField);
                if (listRow.Count == 0)
                {
                    Debug.Log("Csv file empty");
                }
                else
                {
                    listCsvColumn = listRow[0];
                    
                    listRow.RemoveAt(0);
                    listCsvRow = listRow;
                    
                    ConvertListCsvColumnToListField();
                }
            }
        }

        void ConvertListCsvColumnToListField()
        {
            if(listCsvColumn == null || listCsvColumn.Count == 0)
                return;

            listField.Clear();
            foreach (var col in listCsvColumn)
            {
                StringBuilder sb = new StringBuilder();
                bool needUpperCase = false;
                for (int i = 0; i < col.Length; i++)
                {
                    char c = col[i];
                    if (c == '_' || c == ' ')
                    {
                        if (upperCaseAfterWordSeparator)
                            needUpperCase = true;

                        if (removeWordSeparator == false)
                            sb.Append(c);
                        
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
                
                listField.Add(sb.ToString());
            }
        }

        void DrawButtonImport()
        {
            bool oldEnable = GUI.enabled;
            GUI.enabled = listCsvRow.Count > 0 && listField.Count > 0;
            if (GUILayout.Button("Import") == false)
            {
                GUI.enabled = oldEnable;
                return;
            }
            GUI.enabled = oldEnable;

            var soTableType = GetSOTableType(tableAsset.GetType());
            var methodInfo = soTableType.GetMethod("ImportFromCsvData", bindingFlags);
            methodInfo.Invoke(tableAsset, new object[] {listField, listCsvRow});
            Debug.Log("Import success");
        }

        
        void DrawColumnAndField()
        {
            GUILayout.BeginHorizontal();
            
            //Column
            GUILayout.BeginVertical();
            GUILayout.Label("Columns", EditorStyles.boldLabel);
            foreach (var col in listCsvColumn)
            {
                GUILayout.Label(col);
            }
            GUILayout.EndVertical();
            
            
            //Convert Setting
            GUILayout.BeginVertical();
            GUILayout.Label("Convert Setting", EditorStyles.boldLabel);
            removeWordSeparator = GUILayout.Toggle(removeWordSeparator, "Remove word separator");
            upperCaseAfterWordSeparator = GUILayout.Toggle(upperCaseAfterWordSeparator, "Upper case after word separator");
            if (GUILayout.Button("Convert", GUILayout.Width(70)))
            {
                ConvertListCsvColumnToListField();
            }
            
            GUILayout.EndVertical();
            
            
            //Field
            GUILayout.BeginVertical();
            GUILayout.Label("Fields", EditorStyles.boldLabel);
            for (int i = 0; i < listField.Count; i++)
            {
                listField[i] = GUILayout.TextField(listField[i]);
            }
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
        }

        void DrawTableAssetEditor()
        {
            if(tableAsset == null)
                return;

            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            foldoutStyle.fontStyle = FontStyle.Bold;
            showTableAssetEditor = EditorGUILayout.Foldout(showTableAssetEditor, "Table Asset Inspector", true, foldoutStyle);

            if (showTableAssetEditor)
            {
                if (tableAssetEditor == null)
                {
                    tableAssetEditor = Editor.CreateEditor(tableAsset);
                }
                tableAssetEditor.OnInspectorGUI();
            }
        }
    }
}