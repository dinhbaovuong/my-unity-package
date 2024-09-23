using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using VPackage.SaveGameSystem.Core;
using Debug = UnityEngine.Debug;

namespace VPackage.SaveGameSystem
{
    //Cần check lại FromJson/ToJson khi thêm các thuộc tính mới cho database
    public partial class Database : IEnumerable<SGNode>
    {
        public string DatabaseId { get; private set; }
        internal StorageService StorageService { get; private set; }
        public int ChildCount
        {
            get
            {
                InitializeIfNot();
                return dictChildNode.Count;
            }
        }
        protected bool IsInitialized { get; private set; }
        
        public bool IsDatabaseClosed { get; private set; }

        private long nodeIdSeq;
        public long NodeIdSeq
        {
            get
            {
                InitializeIfNot();
                return nodeIdSeq;
            }
            set => nodeIdSeq = value;
        }

        private long childKeySeq;
        public long ChildKeySeq
        {
            get
            {
                InitializeIfNot();
                return childKeySeq;
            }
            set => childKeySeq = value;
        }

        //Child
        private List<string> listChildKey;
        private Dictionary<string, SGNode> dictChildNode;

        // protected Dictionary<string, SGNode> DictChildNode
        // {
        //     get
        //     {
        //         InitializeIfNot();
        //         return dictChildNode;
        //     }
        // }

        void InitializeIfNot()
        {
            lock (this)
            {
                if(IsInitialized)
                    return;
            
                SGJsonNode jsonNode = StorageService.LoadDatabaseDataJson(this);
                FromJsonNodeInternal(jsonNode, false);
                
                IsInitialized = true;
            }
        }

        void SetupDefault()
        {
            NodeIdSeq = SGConstant.defaultNodeIdSeq;
            ChildKeySeq = SGConstant.defaultChildKeySeq;
            dictChildNode = new Dictionary<string, SGNode>();
            listChildKey = new List<string>();
        }
        
        private void FromJsonNodeInternal(SGJsonNode jDb, bool setChanged)
        {
            lock (this)
            {
                if (IsInitialized)
                {
                    Debug.LogError("Do thời gian có hạn nên tính năng này hiện tại chỉ ở mức dùng để init");
                    return;
                }

                
                if (jDb == null || jDb.Count == 0)
                {
                    SetupDefault();
                    IsInitialized = true;
                    if(setChanged)
                        StorageService.SetDatabaseChanged(this);
                    return;
                }


                
                //Import các thuộc tính của database
                NodeIdSeq = jDb.HasKey(KeyDefine.nodeIdSequence) ? jDb[KeyDefine.nodeIdSequence].AsLong : SGConstant.defaultNodeIdSeq;
                ChildKeySeq = jDb.HasKey(KeyDefine.childKeySeq) ? jDb[KeyDefine.childKeySeq].AsLong : SGConstant.defaultChildKeySeq;


                //Import Child
                var jListChild = jDb.HasKey(KeyDefine.listChild) ? jDb[KeyDefine.listChild] : new SGJsonObject();
                dictChildNode = new Dictionary<string, SGNode>();
                listChildKey = new List<string>();
                
                foreach (var kv in jListChild)
                {
                    string key = kv.Key;
                    var jChild = kv.Value;
                    
                    NodeType nodeType = jChild.HasKey(KeyDefine.nodeType)? (NodeType) jChild[KeyDefine.nodeType].AsInt : NodeType.Undefined;
                    long nodeId = jChild.HasKey(KeyDefine.nodeId) ? jChild[KeyDefine.nodeId].AsLong : 0;

                    SGNode childNode = new SGNode(key, this, null, nodeType, nodeId);
                    
                    if (jChild.HasKey(KeyDefine.value))
                        NodeTypeHandler.ImportJChildValueIntoNode(jChild[KeyDefine.value], childNode, nodeType);


                    dictChildNode.Add(key, childNode);
                    listChildKey.Add(key);
                }
                
                
                IsInitialized = true;
                
                if(setChanged)
                    StorageService.SetDatabaseChanged(this);
            }
        }

        public void FromJsonNode(SGJsonNode jDb)
        {
            FromJsonNodeInternal(jDb, true);
        }

        private void FromJsonInternal(string json, bool setChanged)
        {
            if(string.IsNullOrEmpty(json))
                FromJsonNodeInternal(null, setChanged);
            else
            {
                var jNode = SGJson.Parse(json);
                FromJsonNodeInternal(jNode, setChanged);
            }
        }

        public void FromJson(string json)
        {
            var jNode = SGJson.Parse(json);
            FromJsonNodeInternal(jNode, true);
        }
        
        public SGJsonNode ToJsonNode()
        {
            lock (this)
            {
                if (IsInitialized == false)
                {
                    Debug.LogError("Chưa initialized, to json sẽ lấy từ disk");
                    return StorageService.LoadDatabaseDataJson(this);
                }

                SGJsonObject jDb = new SGJsonObject();
                //Các thuộc tính của database
                jDb.Add(KeyDefine.nodeIdSequence, NodeIdSeq);
                jDb.Add(KeyDefine.childKeySeq, ChildKeySeq);
                
                
                //Child
                SGJsonObject jListChild = new SGJsonObject();
                foreach (var key in listChildKey)
                {
                    SGNode childNode = dictChildNode[key];
                    
                    SGJsonObject jChild = new SGJsonObject();
                    jChild[KeyDefine.nodeType] = (int)childNode.NodeType;
                    
                    if (childNode.HasNodeId)
                        jChild[KeyDefine.nodeId] = childNode.NodeId;
                    
                    if (childNode.NodeType != NodeType.Undefined && childNode.IsPrimitive)
                        NodeTypeHandler.ImportNodeIntoJChild(childNode, jChild);

                    jListChild.Add(key, jChild);
                }
                
                jDb.Add(KeyDefine.listChild, jListChild);
                return jDb;
            }
        }

        public string ToJson()
        {
            return ToJsonNode().ToString();
        }

        #region Constructor

        public Database(string databaseId, StorageService storageService)
        {
            this.DatabaseId = databaseId;
            this.StorageService = storageService;
        }

        #endregion

        public void OpenDatabase()
        {
            lock (this)
            {
                StorageService.OpenService(this);
                InitializeIfNot();
            }
        }

        public void CloseDatabase()
        {
            if(IsDatabaseClosed)
                return;
            
            IsDatabaseClosed = true;
            StorageService.CloseService();
        }
        
        public bool IsStorageServiceHasRunningProcess()
        {
            return StorageService.HasRunningProcess();
        }

        public bool HasChangesNeedWriteToStorage()
        {
            return StorageService.HasChangesNeedWriteToStorage();
        }

        public void SaveAllChangesImmediately()
        {
            if (IsDatabaseClosed)
            {
                Debug.LogError("Cannot save because database closed");
                return;
            }
            
            StorageService.SaveAllChangesImmediately();
        }

        internal void IncreaseNodeIdSeq()
        {
            lock (this)
            {
                NodeIdSeq++;
                StorageService.SetDatabaseChanged(this);
            }
        }
        
        public bool HasChild(string childKey)
        {
            if (string.IsNullOrEmpty(childKey))
            {
                return false;
            }
            
            lock (this)
            {
                InitializeIfNot();
                return dictChildNode.ContainsKey(childKey);
            }
        }

        #region Get Child

        /// <summary>
        /// If node with childKey not exist -> create new node and return
        /// </summary>
        public SGNode this[string childKey]
        {
            get
            {
                if (string.IsNullOrEmpty(childKey))
                {
                    Debug.LogError("Child key cannot empty");
                    return null;
                }
                
                lock (this)
                {
                    InitializeIfNot();
                    
                    if (dictChildNode.TryGetValue(childKey, out var childNode))
                        return childNode;
                
                    childNode = new SGNode(childKey, this, null, NodeType.Undefined);
                    dictChildNode.Add(childKey, childNode);
                    listChildKey.Add(childKey);

                    StorageService.SetDatabaseChanged(this);
                
                    return childNode;
                }
            }
        }
        
        /// <summary>
        /// childKey will convert to string, not index
        /// </summary>
        public SGNode this[object childKey]
        {
            get
            {
                if (childKey == null)
                {
                    Debug.LogError("Child key cannot empty");
                    return null;
                }

                return this[childKey.ToString()];
            }
        }

        public SGNode GetChildAt(int childIndex)
        {
            lock (this)
            {
                //Gọi đến ChildCount thì đã InitializeIfNot rồi
                if (childIndex < 0 || childIndex >= ChildCount)
                    throw new IndexOutOfRangeException();

                string key = listChildKey[childIndex];
                return this[key];
            }
        }
        
        /// <summary>
        /// If child not exist -> return null instead of creating a new node
        /// </summary>
        public bool TryGetChild(string childKey, out SGNode childNode)
        {
            if (string.IsNullOrEmpty(childKey))
            {
                Debug.LogError("Child key cannot empty");
                childNode = null;
                return false;
            }

            lock (this)
            {
                InitializeIfNot();
                
                if (dictChildNode.ContainsKey(childKey))
                {
                    childNode = this[childKey];
                    return true;
                }
                else
                {
                    childNode = null;
                    return false;
                }
            }
        }
        
        /// <summary>
        /// If child not exist -> return false instead of creating a new node
        /// </summary>
        public bool TryGetChild(object childKey, out SGNode childNode)
        {
            if(childKey == null)
            {
                Debug.LogError("Child key cannot empty");
                childNode = null;
                return false;
            }
            
            string childKeyString = childKey.ToString();
            if (string.IsNullOrEmpty(childKeyString))
            {
                Debug.LogError("Child key cannot empty");
                childNode = null;
                return false;
            }

            lock (this)
            {
                InitializeIfNot();
                
                if (dictChildNode.ContainsKey(childKeyString))
                {
                    childNode = this[childKeyString];
                    return true;
                }
                else
                {
                    childNode = null;
                    return false;
                }
            }
        }
        
        
        /// <summary>
        /// Get recursively
        /// <para>Example childKeys is [inventory, sword_1, damage] will return node inventory/sword_1/damage if exists</para>
        /// </summary>
        public bool TryGetChild(out SGNode childNode, params string[] childKeys)
        {
            if (childKeys == null || childKeys.Length == 0)
            {
                childNode = null;
                return false;
            }

            lock (this)
            {
                InitializeIfNot();
                
                if (string.IsNullOrEmpty(childKeys[0]))
                {
                    Debug.LogError("Child key cannot empty");
                    childNode = null;
                    return false;
                }

                SGNode childNodeTemp;
                if (dictChildNode.ContainsKey(childKeys[0]) == false)
                {
                    childNode = null;
                    return false;
                }
                else
                {
                    childNodeTemp = this[childKeys[0]];
                }

                for (int i = 1; i < childKeys.Length; i++)
                {
                    string childKey = childKeys[i];
                    if (string.IsNullOrEmpty(childKey))
                    {
                        Debug.LogError("Child key cannot empty");
                        childNode = null;
                        return false;
                    }

                    if (childNodeTemp.HasChild(childKey) == false)
                    {
                        childNode = null;
                        return false;
                    }

                    childNodeTemp = childNodeTemp[childKey];
                }

                childNode = childNodeTemp;
                return true;
            }
        }
        
        /// <summary>
        /// Get recursively
        /// <para>Example childKeys is [inventory, sword_1, damage] will return node inventory/sword_1/damage if exists</para>
        /// </summary>
        public bool TryGetChild(out SGNode childNode, params object[] childKeys)
        {
            if (childKeys == null || childKeys.Length == 0)
            {
                Debug.LogError("Child keys cannot empty");
                childNode = null;
                return false;
            }
            
            lock (this)
            {
                InitializeIfNot();
                
                if (childKeys[0] == null)
                {
                    Debug.LogError("Child key cannot empty");
                    childNode = null;
                    return false;
                }

                string childKeyString = childKeys[0].ToString();
                if (string.IsNullOrEmpty(childKeyString))
                {
                    Debug.LogError("Child key cannot empty");
                    childNode = null;
                    return false;
                }

                SGNode childNodeTemp;
                if (dictChildNode.ContainsKey(childKeyString) == false)
                {
                    childNode = null;
                    return false;
                }
                else
                {
                    childNodeTemp = this[childKeyString];
                }

                for (int i = 1; i < childKeys.Length; i++)
                {
                    if (childKeys[i] == null)
                    {
                        Debug.LogError("Child key cannot empty");
                    }
                    
                    childKeyString = childKeys[i].ToString();
                    if (string.IsNullOrEmpty(childKeyString))
                    {
                        Debug.LogError("Child key cannot empty");
                        childNode = null;
                        return false;
                    }

                    if (childNodeTemp.HasChild(childKeyString) == false)
                    {
                        childNode = null;
                        return false;
                    }

                    childNodeTemp = childNodeTemp[childKeyString];
                }

                childNode = childNodeTemp;
                return true;
            }
        }

        #endregion
        
        #region Delete Child

        public void DeleteAllChild()
        {
            lock (this)
            {
                InitializeIfNot();
                
                if(listChildKey.Count == 0)
                    return;
                
                foreach (var childNode in dictChildNode.Values)
                {
                    childNode.DeleteInternal();
                }
                
                listChildKey.Clear();
                dictChildNode.Clear();

                //Save
                StorageService.SetDatabaseChanged(this);
            }
        }
        
        public void DeleteChild(string childKey)
        {
            if (string.IsNullOrEmpty(childKey))
            {
                Debug.LogError("Child key cannot empty");
                return;
            }
            
            lock (this)
            {
                InitializeIfNot();

                if (dictChildNode.TryGetValue(childKey, out var childNode) == false)
                {
                    Debug.Log("Has no child with this key to delete: " + childKey);
                    return;
                }
                
                childNode.DeleteInternal();

                dictChildNode.Remove(childKey);
                listChildKey.Remove(childKey);
                
                StorageService.SetDatabaseChanged(this);
            }
        }
        
        public void DeleteChildAt(int childIndex)
        {
            lock (this)
            {
                if (childIndex < 0 || childIndex >= ChildCount)
                    throw new IndexOutOfRangeException();
                
                string key = listChildKey[childIndex];
                DeleteChild(key);
            }
        }
        
        public int DeleteAllChild(Predicate<SGNode> match)
        {
            lock (this)
            {
                InitializeIfNot();
                
                if (ChildCount == 0)
                    return 0;
                
                HashSet<string> listChildKeyRemove = new HashSet<string>();
                foreach (var kv in dictChildNode)
                {
                    if(match(kv.Value))
                        listChildKeyRemove.Add(kv.Key);
                }

                foreach (var childKey in listChildKeyRemove)
                {
                    var childNode = dictChildNode[childKey];
                    childNode.DeleteInternal();
                    
                    dictChildNode.Remove(childKey);
                }

                listChildKey.RemoveAll(k => listChildKeyRemove.Contains(k));

                //Save
                StorageService.SetDatabaseChanged(this);
                
                return listChildKeyRemove.Count;
            }
        }

        #endregion

        
        #region Add Child

        public SGNode AddChild(string key)
        {
            return this[key];
        }
        
        /// <summary>
        /// Auto generate key number
        /// </summary>
        public SGNode AddChild()
        {
            lock (this)
            {
                ChildKeySeq++;
                while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                {
                    ChildKeySeq++;
                }
            
                return this[ChildKeySeq.ToString()];
            }
        }
        
        #endregion
        
        
        public IEnumerator<SGNode> GetEnumerator()
        {
            lock (this)
            {
                InitializeIfNot();
                
                foreach (var childNode in dictChildNode.Values)
                {
                    yield return childNode;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<SGNode> GetEnumerableExactOrder()
        {
            lock (this)
            {
                InitializeIfNot();
                
                foreach (var childKey in listChildKey)
                {
                    yield return dictChildNode[childKey];
                }
            }
        }
        
        public void InitializeChildRange(int fromIndex, int toIndex)
        {
            InitializeIfNot();
            
            if(fromIndex < 0)
                throw new IndexOutOfRangeException();
            
            if(fromIndex > toIndex)
                throw new Exception("FromIndex cannot larger toIndex");
            
            if(toIndex >= listChildKey.Count)
                throw new IndexOutOfRangeException();
            
            Parallel.For(fromIndex, toIndex, index =>
            {
                string key = listChildKey[index];
                var childNode = dictChildNode[key];
                if (childNode.HasNodeId && childNode.IsInitialized == false)
                {
                    var jNode = StorageService.LoadNodeDataJsonNoLock(childNode.NodeId);
                    childNode.FromJsonNodeInternal(jNode, false);
                }
            });
        }
        
        public void InitializeAllChild()
        {
            InitializeIfNot();

            Parallel.ForEach(dictChildNode.Values, (childNode, loopState, index) =>
            {
                if (childNode.HasNodeId && childNode.IsInitialized == false)
                {
                    var jNode = StorageService.LoadNodeDataJsonNoLock(childNode.NodeId);
                    childNode.FromJsonNodeInternal(jNode, false);
                }
            });
        }
        
        
        #region Transaction

        /// <summary>
        /// Begins a new transaction. Call <see cref="Commit" /> to end the transaction.
        /// </summary>
        public void BeginTransaction()
        {
            //Chưa làm :)))
        }

        /// <summary>
        /// Commits the transaction that was begun by <see cref="BeginTransaction" />.
        /// </summary>
        public void Commit()
        {
            //Chưa làm :)))
        }

        #endregion
    }
}