using UnityEditor;
using UnityEngine;

namespace VPackage.SaveGameSystem.EditorNS
{
    [System.Serializable]
    public class SGEditorSettingDM
    {
        public int tabIndexSelected = 0;
        public string databaseIdExploring = SaveGame.DefaultDatabaseId;
    }
    
    public static class SGEditorSettingDI
    {
        private static string editorPrefsKey = "sg.editor_setting";
        
        public static SGEditorSettingDM Load()
        {
            if (EditorPrefs.HasKey(editorPrefsKey) == false)
            {
                SGEditorSettingDM dm = new SGEditorSettingDM();
                EditorPrefs.SetString(editorPrefsKey, JsonUtility.ToJson(dm));
            }

            string json = EditorPrefs.GetString(editorPrefsKey);
            if (string.IsNullOrEmpty(json))
            {
                SGEditorSettingDM dm = new SGEditorSettingDM();
                EditorPrefs.SetString(editorPrefsKey, JsonUtility.ToJson(dm));
            }

            return JsonUtility.FromJson<SGEditorSettingDM>(json);
        }

        public static void Save(SGEditorSettingDM dm)
        {
            string json = JsonUtility.ToJson(dm);
            EditorPrefs.SetString(editorPrefsKey, json);
        }
        
        public static void SetTabIndexSelected(int index)
        {
            var dm = Load();
            dm.tabIndexSelected = index;
            Save(dm);
        }

        public static int GetTabIndexSelected()
        {
            var dm = Load();
            return dm.tabIndexSelected;
        }
    }
}