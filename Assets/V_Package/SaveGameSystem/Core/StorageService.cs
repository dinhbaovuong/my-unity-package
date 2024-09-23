using System;

namespace VPackage.SaveGameSystem
{
    public abstract class StorageService
    {
        /// <summary>
        /// Call by database
        /// </summary>
        public abstract void OpenService(Database database);

        public abstract void CloseService();

        public abstract void SaveAllChangesImmediately();
        
        public abstract SGJsonNode LoadDatabaseDataJson(Database database);

        public abstract void SetDatabaseChanged(Database database);

        public abstract SGJsonNode LoadNodeDataJson(SGNode node);

        public virtual SGJsonNode LoadNodeDataJsonNoLock(long nodeId)
        {
            throw new Exception("InitializeAllChild will use LoadNodeDataJsonNoLock, but storage service in use not implement this method");
        }
        
        public abstract void SetNodeChanged(SGNode node);

        public virtual bool HasRunningProcess()
        {
            return false;
        }

        public virtual bool HasChangesNeedWriteToStorage()
        {
            return false;
        }
    }
}