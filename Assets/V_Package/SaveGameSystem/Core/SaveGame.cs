using System.Collections.Generic;
using UnityEngine;

namespace VPackage.SaveGameSystem
{
    public static class SaveGame
    {
        private static string persistentDataPathInternal;
        private static string deviceUniqueIdentifierInternal;
        
#if !UNITY_EDITOR
        public static string persistentDataPath => persistentDataPathInternal;
        public static string deviceUniqueIdentifier => deviceUniqueIdentifierInternal;
#else

        
        public static string persistentDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(persistentDataPathInternal))
                    return Application.persistentDataPath;

                return persistentDataPathInternal;
            }
        }
        
        public static string deviceUniqueIdentifier
        {
            get
            {
                if (string.IsNullOrEmpty(deviceUniqueIdentifierInternal))
                    return SystemInfo.deviceUniqueIdentifier;

                return deviceUniqueIdentifierInternal;
            }
        }
        
        #endif
        
        
        private static readonly string defaultDatabaseId = "0";
        public static string DefaultDatabaseId => defaultDatabaseId;
        
        private static readonly string sharedDatabaseId = "shared";
        public static string SharedDatabaseId => sharedDatabaseId;
        
        
        static Dictionary<string, Database> dictDatabase = new Dictionary<string, Database>();
        
        private static Database defaultDatabase;
        private static Database sharedDatabase;
        
        
        
        [RuntimeInitializeOnLoadMethod]
        static void InitializeOnLoad()
        {
            persistentDataPathInternal = Application.persistentDataPath;
            deviceUniqueIdentifierInternal = SystemInfo.deviceUniqueIdentifier;
        }
        
        static Database OpenDatabase(string databaseId)
        {
            StorageService storageService = StorageServiceProvider.Generate();
            Database database = new Database(databaseId, storageService);
            database.OpenDatabase();
            
            return database;
        }
        
        public static Database Login(string databaseId)
        {
            databaseId = databaseId.ToLower();
            if (dictDatabase.TryGetValue(databaseId, out var database))
            {
                if (database.IsDatabaseClosed == false)
                {
                    Debug.Log("<color=green>Đã login trước đó rồi</color>");
                    return database;
                }
                else
                {
                    dictDatabase.Remove(databaseId);
                    if (defaultDatabase != null)
                    {
                        if (databaseId == defaultDatabase.DatabaseId)
                            defaultDatabase = null;
                    }
                    
                    return Login(databaseId);
                }
            }

            database = OpenDatabase(databaseId);
            dictDatabase.Add(databaseId, database);
            Debug.LogFormat("<color=green>Login thành công: {0}</color>", databaseId);
            return database;
        }

        /// <summary>
        /// Login default database. Use database id is "0"
        /// </summary>
        public static Database Login()
        {
            defaultDatabase = Login(defaultDatabaseId);
            return defaultDatabase;
        }

        public static void Logout(string databaseId)
        {
            if (dictDatabase.TryGetValue(databaseId, out var database) == false)
            {
                Debug.Log("Database chưa login nên không cần logout: " + databaseId);
                return;
            }
            
            database.CloseDatabase();
            dictDatabase.Remove(databaseId);
            if (defaultDatabase != null)
            {
                if (databaseId == defaultDatabase.DatabaseId)
                    defaultDatabase = null;
            }
            
            Debug.LogFormat("<color=green>Logout thành công: {0}</color>", databaseId);
        }

        public static void Logout()
        {
            Logout(defaultDatabaseId);
        }

        public static bool IsLoggedIn(string databaseId)
        {
            return dictDatabase.ContainsKey(databaseId);
        }

        public static Database GetDatabaseLoggedIn(string databaseId)
        {
            if (dictDatabase.TryGetValue(databaseId, out var database))
                return database;
            else
                return null;
        }

        public static void SetDefaultDatabase(string databaseId)
        {
            if (dictDatabase.TryGetValue(databaseId, out var database))
            {
                defaultDatabase = database;
            }
            else
            {
                Debug.LogError("Chưa login database với id này nên không thể set làm default: " + database);
            }
        }

        public static Database Default
        {
            get
            {
                if (defaultDatabase == null)
                {
                    Login();
                }

                return defaultDatabase;
            }
        }

        public static Database Shared
        {
            get
            {
                if (sharedDatabase == null)
                {
                    sharedDatabase = OpenDatabase(sharedDatabaseId);
                }

                return sharedDatabase;
            }
        }
    }
}