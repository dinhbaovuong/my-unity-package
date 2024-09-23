using System;
using System.Collections.Generic;
using VPackage.SaveGameSystem.Core;

namespace VPackage.SaveGameSystem
{
    public partial class SGNode
    {
        //Fill by parent
        public string Key { get;}
        public Database Database { get;}
        public SGNode Root { get; }
        public SGNode Parent { get;}
        public NodeType NodeType { get; private set; }
        public long NodeId { get; private set; } = SGConstant.defaultNodeId;
        
        
        
        //No Fill By Parent
        public bool IsRoot => Root == this;
        public bool HasNodeId => NodeId > SGConstant.defaultNodeId;
        internal bool IsInitialized { get; private set; }
        
        public bool IsPrimitive => NodeType != NodeType.Object;
        
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

        public int ChildCount
        {
            get
            {
                InitializeIfNot();
                return dictChildNode.Count;
            }
        }
        
        public bool IsDeleted { get; internal set; }

        
        
        #region Constructor

        /// <summary>
        /// Only call by database
        /// </summary>
        internal SGNode(string key, Database database, SGNode parent, NodeType nodeType, long nodeId)
        {
            this.Key = key;
            this.Database = database;
            this.Root = this;
            this.Parent = parent;
            this.NodeType = nodeType;
            this.NodeId = nodeId;
        }
        
        internal SGNode(string key, Database database, SGNode parent, NodeType nodeType)
        {
            this.Key = key;
            this.Database = database;
            this.Root = this;
            this.Parent = parent;
            this.NodeType = nodeType;
        }

        internal SGNode(string key, Database database, SGNode root, SGNode parent, NodeType nodeType, long nodeId)
        {
            this.Key = key;
            this.Database = database;
            this.Root = root;
            this.Parent = parent;
            this.NodeType = nodeType;
            this.NodeId = nodeId;
        }
        
        internal SGNode(string key, Database database, SGNode root, SGNode parent, NodeType nodeType)
        {
            this.Key = key;
            this.Database = database;
            this.Root = root;
            this.Parent = parent;
            this.NodeType = nodeType;
        }
        #endregion

        #if UNITY_EDITOR
        /// <summary>
        /// Don't use this method, only for editor
        /// </summary>
        public void ChangeNodeType_Editor(NodeType newNodeType)
        {
            InitializeIfNot();

            if (newNodeType == NodeType.Object) //Nếu muốn chuyển sang Object
            {
                if (HasNodeId) //Nếu đã có nodeId
                {
                    NodeType = NodeType.Object;
                    
                    //Save
                    if(Parent == null)
                        Database.StorageService.SetDatabaseChanged(Database);
                    else
                        Database.StorageService.SetNodeChanged(Parent);
                }
                else
                {
                    NodeType = NodeType.Object;
                    NodeId = Database.NodeIdSeq + 1;
                    
                    //Save
                    Database.IncreaseNodeIdSeq();
                    if(Parent == null)
                        Database.StorageService.SetDatabaseChanged(Database);
                    else
                        Database.StorageService.SetNodeChanged(Parent);
                    
                    Database.StorageService.SetNodeChanged(this); 
                }
            }
            else //Nếu muốn chuyển ngược lại
            {
                if (newNodeType == NodeType.Undefined || newNodeType == NodeType.String)
                {
                    NodeType = newNodeType;
                    
                    //Save
                    if(Parent == null)
                        Database.StorageService.SetDatabaseChanged(Database);
                    else
                        Database.StorageService.SetNodeChanged(Parent);
                }
                else
                {
                    if(numberValue == null)
                        numberValue = NodeTypeHandler.NodeTypeToEType(newNodeType, 0);
                    else
                    {
                        decimal value = NodeTypeHandler.GetNumberValue(numberValue);
                        numberValue = NodeTypeHandler.NodeTypeToEType(newNodeType, value);
                    }
                    
                    NodeType = newNodeType;
                    
                    //Save
                    if(Parent == null)
                        Database.StorageService.SetDatabaseChanged(Database);
                    else
                        Database.StorageService.SetNodeChanged(Parent);
                }
            }
        }
        #endif

        private void InitializeIfNot()
        {
            lock (Database)
            {
                if (IsInitialized)
                    return;

                if (HasNodeId == false) //Không có id thì lấy gì mà load
                {
                    SetupDefault();
                    IsInitialized = true;
                    return;
                }
            
                SGJsonNode jsonNode = Database.StorageService.LoadNodeDataJson(this);
                FromJsonNodeInternal(jsonNode, false);
            
                IsInitialized = true;
            }
        }

        private void SetupDefault()
        {
            ChildKeySeq = SGConstant.defaultChildKeySeq;
            dictChildNode = new Dictionary<string, SGNode>();
            listChildKey = new List<string>();
            //dictChildKeyToIndex = new Dictionary<string, int>();
        }

        public void Delete()
        {
            if(Parent == null)
                Database.DeleteChild(Key);
            else
            {
                Parent.DeleteChild(Key);
            }
        }

        #region Transaction

        /// <summary>
        /// Begins a new transaction. Call <see cref="Commit" /> to end the transaction.
        /// </summary>
        public void BeginTransaction()
        {
            //Chưa làm :)))
            //Trong khi transaction sẽ chưa SetChanges cho đến khi commit
        }

        /// <summary>
        /// Commits the transaction that was begun by <see cref="BeginTransaction" />.
        /// </summary>
        public void Commit()
        {
            //Chưa làm :)))
            //Khi commit sẽ check xem có changes không để set Changes
            //Node không nên gọi thẳng đến Storage Service để set changes nữa mà nên thông qua 1 hàm
        }

        #endregion
    }
}