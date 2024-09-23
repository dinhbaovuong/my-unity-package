using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VPackage.RxSystem;
using VPackage.SaveGameSystem.Security;

namespace VPackage.SaveGameSystem.StorageServices
{
    public class StorageService_FileMerge2 : StorageService
    {
        //Don't change this!!!
        private static readonly string rootFolderPathFormat = "SaveGame/FileMerge2/db.{0}";
        private static readonly string dbFileName = "db.dat";
        private static readonly string dbPassFileName = "dbp.dat";
        
        private static readonly string dbFileNameNoExtension = "db";
        private static readonly string nodeFileNameFormatNoExtension = "n.{0}";
        
        
        private byte[] dbPassBytes;
        private string rootFolderPath;
        private string filePath_Database;
        private string filePath_DatabasePass;


        private Dictionary<string, string> dictFileContent;
        

        private bool isServiceClosed = false;
        private bool databaseChanged = false;
        private Database database;
        readonly HashSet<SGNode> hashSetNodeChanged = new HashSet<SGNode>();
        

        //Task Field
        private bool m_continueThreadUpdateToDisk = true; //flag
        private Task m_taskUpdateToDisk;
        public bool IsTaskUpdateToDiskFinished => m_taskUpdateToDisk.IsCompleted;
        public bool IsTaskUpdateToDiskFaulted => m_taskUpdateToDisk.IsFaulted;
        
        
        //Event
        readonly Disposables disposables = new Disposables();

        
        
        //Temp
        private ulong currentMilisecond = 0;
        private ulong lastTimeSetChanges = 0;
        private ulong lastTimeUpdateAllChangesToDisk = 0;
        
        
        
        
        
        #region Implement

        public override void OpenService(Database database)
        {
            this.database = database;
            
            //Các path cần thiết
            rootFolderPath = Path.Combine(Application.persistentDataPath,  string.Format(rootFolderPathFormat, database.DatabaseId));
            filePath_Database = Path.Combine(rootFolderPath, dbFileName);
            filePath_DatabasePass = Path.Combine(rootFolderPath, dbPassFileName);

            //Tạo folder nếu chưa có
            if (Directory.Exists(rootFolderPath) == false)
                Directory.CreateDirectory(rootFolderPath);
            
            
            //Pass
            string dbPass = SgPassManager.GetOrCreateDbPassInFile(filePath_DatabasePass, database.DatabaseId);
            dbPassBytes = SGAesEncrypt.GetKeyBytes(dbPass);
            
            
            //Read file
            if (File.Exists(filePath_Database))
            {
                string text = File_ReadAllText(filePath_Database);
                dictFileContent = ParseTextToDictionary(text);
            }
            else
            {
                dictFileContent = new Dictionary<string, string>();
            }
            
            
            //Thread
            StartThreadUpdateToDisk();
            
            //Event
            Rx.onApplicationPause.Subscribe(OnApplicationPause).AddTo(disposables);
            Rx.onApplicationQuit.Subscribe(OnApplicationQuit).AddTo(disposables);
        }

        public override void CloseService()
        {
            lock (database)
            {
                if(isServiceClosed)
                    return;
                
                isServiceClosed = true;
            
                disposables.Dispose();
                StopThreadUpdateToDisk();
                SaveAllChangesImmediately();
            
                // while (IsTaskUpdateToDiskFinished == false)
                // {
                //     Thread.Sleep(6);
                //     //Đợi Thread khác hoàn thành việc lưu hết changed rồi mới cho đi tiếp
                // }
            
                if(IsTaskUpdateToDiskFaulted)
                    Debug.LogException(m_taskUpdateToDisk.Exception);
            }
        }

        public override void SaveAllChangesImmediately()
        {
            UpdateDatabaseToDisk();
            UpdateNodeToDisk();
        }

        public override SGJsonNode LoadDatabaseDataJson(Database database)
        {
            if(dictFileContent.ContainsKey(dbFileNameNoExtension) == false)
                return null;
            else
            {
                return SGJson.Parse(SGAesEncrypt.DecryptString(dictFileContent[dbFileNameNoExtension], dbPassBytes));
            }
        }

        public override void SetDatabaseChanged(Database database)
        {
            lock (database)
            {
                databaseChanged = true;
                lastTimeSetChanges = currentMilisecond;
            }
        }

        public override SGJsonNode LoadNodeDataJson(SGNode node)
        {
            lock (database)
            {
                if (node.HasNodeId == false)
                    throw new Exception("Không thể load datajson cho node không có nodeId");

                
                string fileName = string.Format(nodeFileNameFormatNoExtension, node.NodeId);
                if (dictFileContent.ContainsKey(fileName) == false)
                    return null;
                else
                {
                    return SGJsonNode.Parse(SGAesEncrypt.DecryptString(dictFileContent[fileName], dbPassBytes));
                }
            }
        }

        public override void SetNodeChanged(SGNode node)
        {
            lock (database)
            {
                if (node.HasNodeId == false)
                    throw new Exception("Không thể set changed cho node không có nodeId");

                hashSetNodeChanged.Add(node);
                lastTimeSetChanges = currentMilisecond;
            }
        }

        #endregion


        Dictionary<string, string> ParseTextToDictionary(string text)
        {
            if (string.IsNullOrEmpty(text))
                return dictFileContent;

            var list = text.Split(',');
            if (list.Length % 2 != 0)
            {
                throw new Exception("Số lượng phần tử không chẵn -> check lại");
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();
            for (int i = 0; i < list.Length; i+=2)
            {
                dict.Add(list[i], list[i + 1]);
            }

            return dict;
        }

        string ParseDictToString(Dictionary<string, string> dict)
        {
            StringBuilder sb = new StringBuilder();
            int index = 0;
            foreach (var kv in dict)
            {
                sb.Append(kv.Key);
                sb.Append(',');
                sb.Append(kv.Value);

                if (index < dict.Count - 1)
                    sb.Append(',');

                index++;
            }

            return sb.ToString();
        }
        

        string File_ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        void File_WriteAllText(string filePath, string contents)
        {
            File.WriteAllText(filePath, contents);
        }


        void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveAllChangesImmediately();
            }
        }

        void OnApplicationQuit()
        {
            SaveAllChangesImmediately();
        }
        
        


        #region Thread Update To Disk
        
        public void StartThreadUpdateToDisk()
        {
            m_continueThreadUpdateToDisk = true;
            m_taskUpdateToDisk = new Task(ThreadUpdateToDisk);
            m_taskUpdateToDisk.ContinueWith((t) =>
            {
                if (t.IsFaulted && isServiceClosed == false)
                {
                    Rx.RunAction(() => Debug.LogException(t.Exception));
                    Rx.RunAction(() =>
                    {
                        Debug.Log("Thread Update To Disk error -> restart...");
                        StartThreadUpdateToDisk();
                        Debug.Log("Thread Update To Disk restart success");
                    });
                }
            });
            m_taskUpdateToDisk.Start();
        }
        
        public void StopThreadUpdateToDisk()
        {
            m_continueThreadUpdateToDisk = false;
        }
        
        void ThreadUpdateToDisk()
        {
            while (m_continueThreadUpdateToDisk)
            {
                //Nếu đang set change liên tục thì nên từ từ rồi save
                //Cơ mà nếu đợi quá lâu rồi thì nên save luôn
                if (currentMilisecond - lastTimeSetChanges > 15 || currentMilisecond - lastTimeUpdateAllChangesToDisk > 159)
                {
                    bool hasChanged1 = UpdateDatabaseToDisk();
                    bool hasChanged2 = UpdateNodeToDisk();
                    
                    //Vì merge dùng chung 1 file vì vậy chỉ nên write 1 lần
                    if(hasChanged1 || hasChanged2)
                        File_WriteAllText(filePath_Database, ParseDictToString(dictFileContent));
                    
                    lastTimeUpdateAllChangesToDisk = currentMilisecond;
                }
                else
                {
                    //Rx.ExecuteInMainThread(() => Debug.Log("Đợi 1 chút"));
                }

                Thread.Sleep(16);
                currentMilisecond += 16;
            }
        }

        bool UpdateDatabaseToDisk()
        {
            SGJsonNode jNode;
            lock (database)
            {
                if(databaseChanged == false)
                    return false;

                try
                {
                    jNode = database.ToJsonNode();
                    databaseChanged = false;
                }
                catch (Exception e)
                {
                    Rx.RunAction(() => Debug.LogException(e));
                    return false;
                }
            }

            try
            {
                dictFileContent[dbFileNameNoExtension] = SGAesEncrypt.EncryptString(jNode.ToString(), dbPassBytes) ;
                return true;
                //File_WriteAllText(filePath_Database, dictFileContent.ToString());
            }
            catch (Exception e)
            {
                Rx.RunAction(() => Debug.LogException(e));
                return false;
            }
        }

        bool UpdateNodeToDisk()
        {
            Dictionary<long, SGJsonNode> dictJson;
            List<long> listNodeIdDelete = null;
            lock (database)
            {
                if(hashSetNodeChanged.Count == 0)
                    return false;
                
                dictJson = new Dictionary<long, SGJsonNode>();
                foreach (var node in hashSetNodeChanged)
                {
                    try
                    {
                        if(node.IsDeleted == false)
                            dictJson.Add(node.NodeId, node.ToJsonNode());
                        else
                        {
                            if(listNodeIdDelete == null)
                                listNodeIdDelete = new List<long>();
                            
                            listNodeIdDelete.Add(node.NodeId);
                        }
                    }
                    catch (Exception e)
                    {
                        Rx.RunAction(() => Debug.LogException(e));
                    }
                }
                
                hashSetNodeChanged.Clear();
            }

            foreach (var kv in dictJson)
            {
                try
                {
                    string fileName = string.Format(nodeFileNameFormatNoExtension, kv.Key);
                    dictFileContent[fileName] = SGAesEncrypt.EncryptString(kv.Value.ToString(), dbPassBytes) ;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            if (listNodeIdDelete != null)
            {
                foreach (var nodeId in listNodeIdDelete)
                {
                    try
                    {
                        string fileName = string.Format(nodeFileNameFormatNoExtension, nodeId);
                        dictFileContent.Remove(fileName);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            
            //File_WriteAllText(filePath_Database, dictFileContent.ToString());
            return true;
        }
        
        #endregion
    }
}