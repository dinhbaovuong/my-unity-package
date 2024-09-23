using System.Collections.Generic;
using UnityEngine;

namespace VPackage.DataTableSystem.SoTableSystem
{
    public abstract class SoTableOneId<TTable, TRow, TId> : SoTable<TTable, TRow>
        where TTable : SoTableOneId<TTable, TRow, TId>, new()
        where TRow : new()
    {
        protected Dictionary<TId, TRow> m_dictRow;
        
        public static Dictionary<TId, TRow> DictRow
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
        
        protected abstract TId GetRowId(TRow row);
        
        protected override void SetListRow(List<TRow> rows)
        {
            listRow = rows;
            m_dictRow = new Dictionary<TId, TRow>();
            foreach (var row in listRow)
            {
                m_dictRow.Add(GetRowId(row), row);
            }
        }
        
        

        public static TRow GetRowById(TId id)
        {
            DictRow.TryGetValue(id, out TRow row);
            return row;
        }
        
        public static TRow GetRowByIdWithLog(TId id)
        {
            if (DictRow.TryGetValue(id, out TRow row))
                return row;
            else
            {
                Debug.LogError("Cannot find row with id: " + id);
                return default;
            }
        }

        public static bool PatchTable_ReplaceRow(TId id, TRow newRow)
        {
            if (DictRow.TryGetValue(id, out TRow row))
            {
                int index = Instance.listRow.IndexOf(row);
                if (index == -1)
                {
                    Debug.LogError("WTF, trong dict có mà trong list không có!");
                    return false;
                }

                Instance.listRow[index] = newRow;
                DictRow[id] = newRow;
                return true;
            }

            return false;
        }

        public static bool PatchTable_AddRow(TRow newRow)
        {
            TId id = Instance.GetRowId(newRow);
            if (DictRow.ContainsKey(id))
            {
                Debug.Log("Đã tồn tại row với id này trong table: " + id);
                return false;
            }
            
            DictRow.Add(id, newRow);
            Instance.listRow.Add(newRow);
            return true;
        }
    }
}