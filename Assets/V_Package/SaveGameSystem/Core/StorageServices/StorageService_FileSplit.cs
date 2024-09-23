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
    public class StorageService_FileSplit : StorageService
    {
        //Don't change this!!!
        private static readonly string rootFolderPathFormat = "SaveGame/FileSplit/db.{0}";
        private static readonly string dbFileName = "db.dat";
        private static readonly string dbPassFileName = "dbp.dat";
        private static readonly string nodeFileNameFormat = "n.{0}.dat";
        
        //Cache
        private byte[] dbPassBytes;
        private string rootFolderPath;
        private string filePath_Database;
        private string filePath_DatabasePass;
        

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

        
        
        //Time Config
        private const ulong checkPerMillisecond = 20;
        private const ulong maxMillisecondIgnoreSave = 100;
        private ulong currentMillisecond = 0;
        private ulong lastTimeSetChanges = 0;
        private ulong lastTimeUpdateAllChangesToDisk = 0;
        
        //Process
        private bool isUpdatingAllChangesToDisk = false;
        
        
        
        #region Implement

        public override void OpenService(Database database)
        {
            this.database = database;
            
            //Các path cần thiết
            rootFolderPath = Path.Combine(SaveGame.persistentDataPath,  string.Format(rootFolderPathFormat, database.DatabaseId));
            filePath_Database = Path.Combine(rootFolderPath, dbFileName);
            filePath_DatabasePass = Path.Combine(rootFolderPath, dbPassFileName);

            //Tạo folder nếu chưa có
            if (Directory.Exists(rootFolderPath) == false)
                Directory.CreateDirectory(rootFolderPath);
            
            
            //Db Pass
            try
            {
                string dbPass = SgPassManager.GetOrCreateDbPassInFile(filePath_DatabasePass, database.DatabaseId);
                dbPassBytes = SGAesEncrypt.GetKeyBytes(dbPass);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("Get DB Pass Error -> Delete database and re-create");
                Directory.Delete(rootFolderPath, true);
                Directory.CreateDirectory(rootFolderPath);
                
                string dbPass = SgPassManager.GetOrCreateDbPassInFile(filePath_DatabasePass, database.DatabaseId);
                dbPassBytes = SGAesEncrypt.GetKeyBytes(dbPass);
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
            lock (database)
            {
                if (File.Exists(filePath_Database) == false)
                    return null;
                else
                {
                    return SGJson.Parse(File_ReadAllText(filePath_Database));
                }
            }
        }

        public override void SetDatabaseChanged(Database database)
        {
            lock (database)
            {
                databaseChanged = true;
                lastTimeSetChanges = currentMillisecond;
            }
        }

        public override SGJsonNode LoadNodeDataJson(SGNode node)
        {
            lock (database)
            {
                if (node.HasNodeId == false)
                    throw new Exception("Không thể load dataJson cho node không có nodeId");


                string filePath = GetFilePath_Node(node.NodeId);
                if (File.Exists(filePath) == false)
                    return null;
                else
                {
                    return SGJson.Parse(File_ReadAllText(filePath));
                }
            }
        }

        public override SGJsonNode LoadNodeDataJsonNoLock(long nodeId)
        {
            string filePath = GetFilePath_Node(nodeId);
            if (File.Exists(filePath) == false)
                return null;
            else
            {
                return SGJson.Parse(File_ReadAllText(filePath));
            }
        }

        public override void SetNodeChanged(SGNode node)
        {
            lock (database)
            {
                if (node.HasNodeId == false)
                    throw new Exception("Không thể set changed cho node không có nodeId");

                hashSetNodeChanged.Add(node);
                lastTimeSetChanges = currentMillisecond;
            }
        }

        public override bool HasRunningProcess()
        {
            return isUpdatingAllChangesToDisk;
        }

        public override bool HasChangesNeedWriteToStorage()
        {
            lock (database)
            {
                return databaseChanged || hashSetNodeChanged.Count > 0;
            }
        }

        #endregion
        
        string GetFilePath_Node(long nodeId)
        {
            return Path.Combine(rootFolderPath, string.Format(nodeFileNameFormat, nodeId));
        }
        
        string File_ReadAllText(string filePath)
        {
#if UNITY_EDITOR
            return File.ReadAllText(filePath);
#else
            var cipherBytes = File.ReadAllBytes(filePath);
            if (cipherBytes == null || cipherBytes.Length == 0)
                return "";
            
            try
            {
                byte[] plainBytes = SGAesEncrypt.DecryptBytes(cipherBytes, dbPassBytes);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return "";
            }
#endif
        }

        void File_WriteAllText(string filePath, string contents)
        {
#if UNITY_EDITOR
            File.WriteAllText(filePath, contents);
#else
            var cipherBytes = SGAesEncrypt.EncryptStringToBytes(contents, dbPassBytes);
            try
            {
                File.WriteAllBytes(filePath, cipherBytes);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#endif
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
            //SaveAllChangesImmediately();
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
                if (currentMillisecond - lastTimeSetChanges > checkPerMillisecond ||
                    currentMillisecond - lastTimeUpdateAllChangesToDisk > maxMillisecondIgnoreSave)
                {
                    isUpdatingAllChangesToDisk = true;
                    
                    UpdateDatabaseToDisk();
                    UpdateNodeToDisk();
                    
                    lastTimeUpdateAllChangesToDisk = currentMillisecond;
                    isUpdatingAllChangesToDisk = false;
                }
                else
                {
                    //Rx.ExecuteInMainThread(() => Debug.Log("Đợi 1 chút"));
                }

                Thread.Sleep((int)checkPerMillisecond);
                currentMillisecond += checkPerMillisecond;
            }
        }

        void UpdateDatabaseToDisk()
        {
            SGJsonNode jNode;
            lock (database)
            {
                if(databaseChanged == false)
                    return;

                try
                {
                    jNode = database.ToJsonNode();
                    databaseChanged = false;
                }
                catch (Exception e)
                {
                    Rx.RunAction(() => Debug.LogException(e));
                    return;
                }
            }

            try
            {
                File_WriteAllText(filePath_Database, jNode.ToString());
            }
            catch (Exception e)
            {
                Rx.RunAction(() => Debug.LogException(e));
            }
        }

        void UpdateNodeToDisk()
        {
            Dictionary<long, SGJsonNode> dictJson;
            List<long> listNodeIdDelete = null;
            lock (database)
            {
                if(hashSetNodeChanged.Count == 0)
                    return;
                
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
                    // if(kv.Value == null)
                    //     continue;
                    
                    string filePath = GetFilePath_Node(kv.Key);
                    File_WriteAllText(filePath, kv.Value.ToString());
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
                        string filePath = GetFilePath_Node(nodeId);
                        if(File.Exists(filePath))
                            File.Delete(filePath);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            
        }
        
        #endregion
    }
}