using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VPackage.DataTableSystem.SoTableSystem
{
    public abstract class SoTableTwoId<TTable, TRow, TId1, TId2> : SoTable<TTable, TRow>
        where TTable : SoTableTwoId<TTable, TRow, TId1, TId2>, new()
        where TRow : new()
    {
        protected Dictionary<TId1, Dictionary<TId2, TRow>> m_dictRow;
        
        public static Dictionary<TId1, Dictionary<TId2, TRow>> DictRow
        {
            get
            {
                if (Instance.m_dictRow == null)
                {
                    //Init table
                    var listRow = Instance.LoadListRow();
                    Instance.SetListRow(listRow);
                    
                    if (Instance.m_dictRow == null)
                    {
                        Debug.LogError("InitTable failed, table not inited!");
                        return null;
                    }
                }

                return Instance.m_dictRow;
            }
        }
        
        protected abstract void GetRowId(TRow row, out TId1 id1, out TId2 id2);
        
        protected override void SetListRow(List<TRow> rows)
        {
            var dict1 = new Dictionary<TId1, Dictionary<TId2, TRow>>();
            foreach (var row in rows)
            {
                GetRowId(row, out TId1 id1, out TId2 id2);
                
                if (dict1.TryGetValue(id1, out var dict2) == false) //Nếu trong dict chưa có id1
                {
                    dict2 = new Dictionary<TId2, TRow>();
                    dict1.Add(id1, dict2);
                }
                
                dict2.Add(id2, row);
            }
            
            listRow = rows;
            m_dictRow = dict1;
        }
        
        public static TRow GetRowById(TId1 id1, TId2 id2)
        {
            var dict1 = DictRow;
            
            if (dict1.TryGetValue(id1, out var dict2) == false)
                return default;

            if (dict2.TryGetValue(id2, out TRow row) == false)
                return default;

            return row;
        }
        
        public static TRow GetRowByIdWithLog(TId1 id1, TId2 id2)
        {
            var dict1 = DictRow;
            
            if (dict1.TryGetValue(id1, out var dict2) == false)
            {
                Debug.LogError("Id1 not exist: " + id1);
                return default;
            }

            if (dict2.TryGetValue(id2, out TRow row) == false)
            {
                Debug.LogError("Id2 not exist: " + id2);
                return default;
            }
            
            return row;
        }
        
        public static Dictionary<TId2, TRow> GetDictRowById(TId1 id1)
        {
            if (DictRow.TryGetValue(id1, out var dict) == false)
                return null;

            return dict;
        }
        
        public static Dictionary<TId2, TRow> GetDictRowByIdWithLog(TId1 id1)
        {
            if (DictRow.TryGetValue(id1, out var dict) == false)
            {
                Debug.LogError("Id1 not exist: " + id1);
                return null;
            }

            return dict;
        }
        
        public static List<TRow> GetListRowById(TId1 id1)
        {
            var dict2 = GetDictRowById(id1);
            if(dict2 == null)
                return new List<TRow>();

            List<TRow> listRow = dict2.Values.ToList();
            return listRow;
        }
        
        public static List<TRow> GetListRowByIdWithLog(TId1 id1)
        {
            var dict2 = GetDictRowById(id1);
            if (dict2 == null)
            {
                Debug.LogError("Id1 not exist: " + id1);
                return new List<TRow>();
            }

            List<TRow> listRow = dict2.Values.ToList();
            return listRow;
        }
        
        public static bool PatchTable_ReplaceRow(TId1 id1, TId2 id2, TRow newRow)
        {
            if (DictRow.TryGetValue(id1, out var dict) == false)
            {
                Debug.LogError("Id1 not exist: " + id1);
                return false;
            }

            if (dict.TryGetValue(id2, out TRow row))
            {
                int index = Instance.listRow.IndexOf(row);
                if (index == -1)
                {
                    Debug.LogError("WTF, trong dict có mà trong list không có!");
                    return false;
                }
                Instance.listRow[index] = newRow;
                dict[id2] = newRow;
                return true;
            }

            return false;
        }
        
        public static bool PatchTable_AddRow(TRow newRow)
        {
            Instance.GetRowId(newRow, out TId1 id1, out TId2 id2);
            if (DictRow.TryGetValue(id1, out var dict) == false)
            {
                dict = new Dictionary<TId2, TRow>();
                DictRow.Add(id1, dict);
                dict.Add(id2, newRow);
                Instance.listRow.Add(newRow);
                return true;
            }
            else
            {
                if (dict.ContainsKey(id2))
                {
                    Debug.LogErrorFormat("Đã tồn tại row này trong table: {0} - {1}", id1, id2);
                    return false;
                }
                
                dict.Add(id2, newRow);
                Instance.listRow.Add(newRow);
                return true;
            }
        }

        public static bool PathTable_ReplaceDictRowById1(TId1 id1, Dictionary<TId2, TRow> dict)
        {
            if (DictRow.TryGetValue(id1, out var dictOld) == false)
            {
                Debug.LogError("Id1 not exist: " + id1);
                return false;
            }

            //Xóa hết row cũ
            var listRow = Instance.listRow;
            foreach (var row in dictOld.Values)
            {
                int index = listRow.IndexOf(row);
                if (index == -1)
                {
                    Debug.LogError("Wtf, trong list không có row có trong dict");
                    continue;
                }
                
                listRow[index] = row;
            }

            //Set row mới
            foreach (var row in dict.Values)
            {
                listRow.Add(row);
            }
            DictRow[id1] = dict;
            return true;
        }
    }
}