using System;
using System.Collections.Generic;
using UnityEngine;

namespace VPackage.SaveGameSystem
{
    public partial class SGNode
    {
        #region string
        
        public bool HasChildValue(string value)
        {
            lock (Database)
            {
                InitializeIfNot();
                
                foreach (var v in dictChildNode.Values)
                {
                    if (v.StringValue == value)
                        return true;
                }

                return false;
            }
        }

        public SGNode AddChildValue(string value)
        {
            lock (Database)
            {
                ChildKeySeq++;
                while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                {
                    ChildKeySeq++;
                }
            
                var node = this[ChildKeySeq.ToString()];
                node.SetString(value);
                return node;
            }
        }
        
        public void AddChildValueRange(IEnumerable<string> values)
        {
            lock (Database)
            {
                foreach (var value in values)
                {
                    ChildKeySeq++;
                    while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                    {
                        ChildKeySeq++;
                    }
            
                    var node = this[ChildKeySeq.ToString()];
                    node.SetString(value);
                }
            }
        }

        public bool DeleteChildValue(string value)
        {
            lock (Database)
            {
                SGNode nodeToDelete = null;
                foreach (var childNode in dictChildNode.Values)
                {
                    if(childNode.NodeType != NodeType.String)
                        continue;

                    if (childNode.StringValue == value)
                    {
                        nodeToDelete = childNode;
                        break;
                    }
                }

                if (nodeToDelete != null)
                {
                    nodeToDelete.Delete();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int DeleteAllChildValue(Predicate<string> match)
        {
            return DeleteAllChild(childNode =>
            {
                if (childNode.NodeType != NodeType.String)
                    return false;

                return match(childNode.StringValue);
            });
        }
        
        
        
        public bool TryGetChildValue(string childKey, string defaultValue, out string value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetString(defaultValue);
                return true;
            }
            else
            {
                value = defaultValue;
                return false;
            }
        }
        
        public bool TryGetChildValue(string childKey, out string value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetString("");
                return true;
            }
            else
            {
                value = "";
                return false;
            }
        }
        
        #endregion


        #region int
        
        public bool HasChildValue(int value)
        {
            lock (Database)
            {
                InitializeIfNot();
                
                foreach (var v in dictChildNode.Values)
                {
                    if (v.IntValue == value)
                        return true;
                }

                return false;
            }
        }

        public SGNode AddChildValue(int value)
        {
            lock (Database)
            {
                ChildKeySeq++;
                while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                {
                    ChildKeySeq++;
                }
            
                var node = this[ChildKeySeq.ToString()];
                node.SetInt(value);
                return node;
            }
        }
        
        public void AddChildValueRange(IEnumerable<int> values)
        {
            lock (Database)
            {
                foreach (var value in values)
                {
                    ChildKeySeq++;
                    while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                    {
                        ChildKeySeq++;
                    }
            
                    var node = this[ChildKeySeq.ToString()];
                    node.SetInt(value);
                }
            }
        }
        
        public bool DeleteChildValue(int value)
        {
            lock (Database)
            {
                SGNode nodeToDelete = null;
                foreach (var childNode in dictChildNode.Values)
                {
                    if (childNode.IntValue == value)
                    {
                        nodeToDelete = childNode;
                        break;
                    }
                }

                if (nodeToDelete != null)
                {
                    nodeToDelete.Delete();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int DeleteAllChildValue(Predicate<int> match)
        {
            return DeleteAllChild(childNode => match(childNode.IntValue));
        }
        
        
        
        public bool TryGetChildValue(string childKey, int defaultValue, out int value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetInt(defaultValue);
                return true;
            }
            else
            {
                value = defaultValue;
                return false;
            }
        }
        
        public bool TryGetChildValue(string childKey, out int value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetInt(0);
                return true;
            }
            else
            {
                value = 0;
                return false;
            }
        }

        #endregion


        #region float

        public bool HasChildValue(float value)
        {
            lock (Database)
            {
                InitializeIfNot();
                
                foreach (var v in dictChildNode.Values)
                {
                    if (Mathf.Approximately(v.FloatValue, value))
                        return true;
                }

                return false;
            }
        }
        
        public SGNode AddChildValue(float value)
        {
            lock (Database)
            {
                ChildKeySeq++;
                while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                {
                    ChildKeySeq++;
                }
            
                var node = this[ChildKeySeq.ToString()];
                node.SetFloat(value);
                return node;
            }
        }
        
        public void AddChildValueRange(IEnumerable<float> values)
        {
            lock (Database)
            {
                foreach (var value in values)
                {
                    ChildKeySeq++;
                    while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                    {
                        ChildKeySeq++;
                    }
            
                    var node = this[ChildKeySeq.ToString()];
                    node.SetFloat(value);
                }
            }
        }
        
        public bool DeleteChildValue(float value)
        {
            lock (Database)
            {
                SGNode nodeToDelete = null;
                foreach (var childNode in dictChildNode.Values)
                {
                    if (Mathf.Approximately(childNode.FloatValue, value))
                    {
                        nodeToDelete = childNode;
                        break;
                    }
                }

                if (nodeToDelete != null)
                {
                    nodeToDelete.Delete();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int DeleteAllChildValue(Predicate<float> match)
        {
            return DeleteAllChild(childNode => match(childNode.FloatValue));
        }
        
        
        
        public bool TryGetChildValue(string childKey, float defaultValue, out float value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetFloat(defaultValue);
                return true;
            }
            else
            {
                value = defaultValue;
                return false;
            }
        }
        
        public bool TryGetChildValue(string childKey, out float value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetFloat(0f);
                return true;
            }
            else
            {
                value = 0f;
                return false;
            }
        }

        #endregion


        #region long
        
        public bool HasChildValue(long value)
        {
            lock (Database)
            {
                InitializeIfNot();
                
                foreach (var v in dictChildNode.Values)
                {
                    if (v.LongValue == value)
                        return true;
                }

                return false;
            }
        }

        public SGNode AddChildValue(long value)
        {
            lock (Database)
            {
                ChildKeySeq++;
                while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                {
                    ChildKeySeq++;
                }
            
                var node = this[ChildKeySeq.ToString()];
                node.SetLong(value);
                return node;
            }
        }
        
        public void AddChildValueRange(IEnumerable<long> values)
        {
            lock (Database)
            {
                foreach (var value in values)
                {
                    ChildKeySeq++;
                    while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                    {
                        ChildKeySeq++;
                    }
            
                    var node = this[ChildKeySeq.ToString()];
                    node.SetLong(value);
                }
            }
        }
        
        public bool DeleteChildValue(long value)
        {
            lock (Database)
            {
                SGNode nodeToDelete = null;
                foreach (var childNode in dictChildNode.Values)
                {
                    if (childNode.LongValue == value)
                    {
                        nodeToDelete = childNode;
                        break;
                    }
                }

                if (nodeToDelete != null)
                {
                    nodeToDelete.Delete();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int DeleteAllChildValue(Predicate<long> match)
        {
            return DeleteAllChild(childNode => match(childNode.LongValue));
        }
        
        
        
        public bool TryGetChildValue(string childKey, long defaultValue, out long value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetLong(defaultValue);
                return true;
            }
            else
            {
                value = defaultValue;
                return false;
            }
        }
        
        public bool TryGetChildValue(string childKey, out long value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetLong(0L);
                return true;
            }
            else
            {
                value = 0L;
                return false;
            }
        }

        #endregion

        
        #region decimal
        
        public bool HasChildValue(decimal value)
        {
            lock (Database)
            {
                InitializeIfNot();
                
                foreach (var v in dictChildNode.Values)
                {
                    if (v.DecimalValue == value)
                        return true;
                }

                return false;
            }
        }

        public SGNode AddChildValue(decimal value)
        {
            lock (Database)
            {
                ChildKeySeq++;
                while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                {
                    ChildKeySeq++;
                }
            
                var node = this[ChildKeySeq.ToString()];
                node.SetDecimal(value);
                return node;
            }
        }
        
        public void AddChildValueRange(IEnumerable<decimal> values)
        {
            lock (Database)
            {
                foreach (var value in values)
                {
                    ChildKeySeq++;
                    while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                    {
                        ChildKeySeq++;
                    }
            
                    var node = this[ChildKeySeq.ToString()];
                    node.SetDecimal(value);
                }
            }
        }
        
        public bool DeleteChildValue(decimal value)
        {
            lock (Database)
            {
                SGNode nodeToDelete = null;
                foreach (var childNode in dictChildNode.Values)
                {
                    if (childNode.DecimalValue == value)
                    {
                        nodeToDelete = childNode;
                        break;
                    }
                }

                if (nodeToDelete != null)
                {
                    nodeToDelete.Delete();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int DeleteAllChildValue(Predicate<decimal> match)
        {
            return DeleteAllChild(childNode => match(childNode.DecimalValue));
        }
        
        
        
        public bool TryGetChildValue(string childKey, decimal defaultValue, out decimal value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetDecimal(defaultValue);
                return true;
            }
            else
            {
                value = defaultValue;
                return false;
            }
        }
        
        public bool TryGetChildValue(string childKey, out decimal value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetDecimal(0m);
                return true;
            }
            else
            {
                value = 0m;
                return false;
            }
        }
        
        #endregion
        

        #region bool

        public bool HasChildValue(bool value)
        {
            lock (Database)
            {
                InitializeIfNot();
                
                foreach (var v in dictChildNode.Values)
                {
                    if (v.BoolValue == value)
                        return true;
                }

                return false;
            }
        }
        
        public SGNode AddChildValue(bool value)
        {
            return AddChildValue(value ? 1 : 0);
        }
        
        public void AddChildValueRange(IEnumerable<bool> values)
        {
            foreach (var value in values)
            {
                AddChildValue(value ? 1 : 0);
            }
        }
        
        public bool DeleteChildValue(bool value)
        {
            lock (Database)
            {
                SGNode nodeToDelete = null;
                foreach (var childNode in dictChildNode.Values)
                {
                    if (childNode.BoolValue == value)
                    {
                        nodeToDelete = childNode;
                        break;
                    }
                }

                if (nodeToDelete != null)
                {
                    nodeToDelete.Delete();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int DeleteAllChildValue(Predicate<bool> match)
        {
            return DeleteAllChild(childNode => match(childNode.BoolValue));
        }
        
        
        
        public bool TryGetChildValue(string childKey, bool defaultValue, out bool value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetBool(defaultValue);
                return true;
            }
            else
            {
                value = defaultValue;
                return false;
            }
        }

        public bool TryGetChildValue(string childKey, out bool value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetBool(false);
                return true;
            }
            else
            {
                value = false;
                return false;
            }
        }
        
        #endregion
        
        
        #region DateTime
        
        public bool HasChildValue_DateTime(DateTime value)
        {
            lock (Database)
            {
                InitializeIfNot();
                
                foreach (var v in dictChildNode.Values)
                {
                    if (v.DateTimeValue == value)
                        return true;
                }

                return false;
            }
        }

        public SGNode AddChildValue_DateTime(DateTime value)
        {
            lock (Database)
            {
                ChildKeySeq++;
                while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                {
                    ChildKeySeq++;
                }
            
                var node = this[ChildKeySeq.ToString()];
                node.SetDateTime(value);
                return node;
            }
        }
        
        public void AddChildValueRange_DateTime(IEnumerable<DateTime> values)
        {
            lock (Database)
            {
                foreach (var value in values)
                {
                    ChildKeySeq++;
                    while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                    {
                        ChildKeySeq++;
                    }
            
                    var node = this[ChildKeySeq.ToString()];
                    node.SetDateTime(value);
                }
            }
        }

        public bool DeleteChildValue_DateTime(DateTime value)
        {
            lock (Database)
            {
                SGNode nodeToDelete = null;
                foreach (var childNode in dictChildNode.Values)
                {
                    if(childNode.NodeType != NodeType.String)
                        continue;

                    if (childNode.DateTimeValue == value)
                    {
                        nodeToDelete = childNode;
                        break;
                    }
                }

                if (nodeToDelete != null)
                {
                    nodeToDelete.Delete();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int DeleteAllChildValue_DateTime(Predicate<DateTime> match)
        {
            return DeleteAllChild(childNode =>
            {
                if (childNode.NodeType != NodeType.String)
                    return false;

                return match(childNode.DateTimeValue);
            });
        }
        
        
        
        public bool TryGetChildValue_DateTime(string childKey, DateTime defaultValue, out DateTime value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetDateTime(defaultValue);
                return true;
            }
            else
            {
                value = defaultValue;
                return false;
            }
        }
        
        public bool TryGetChildValue_DateTime(string childKey, out DateTime value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetDateTime(DefaultDateTime);
                return true;
            }
            else
            {
                value = DefaultDateTime;
                return false;
            }
        }
        
        #endregion
        
        
        #region Date
        
        public bool HasChildValue_Date(DateTime value)
        {
            lock (Database)
            {
                InitializeIfNot();
                
                foreach (var v in dictChildNode.Values)
                {
                    if (v.DateValue == value)
                        return true;
                }

                return false;
            }
        }

        public SGNode AddChildValue_Date(DateTime value)
        {
            lock (Database)
            {
                ChildKeySeq++;
                while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                {
                    ChildKeySeq++;
                }
            
                var node = this[ChildKeySeq.ToString()];
                node.SetDate(value);
                return node;
            }
        }
        
        public void AddChildValueRange_Date(IEnumerable<DateTime> values)
        {
            lock (Database)
            {
                foreach (var value in values)
                {
                    ChildKeySeq++;
                    while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                    {
                        ChildKeySeq++;
                    }
            
                    var node = this[ChildKeySeq.ToString()];
                    node.SetDate(value);
                }
            }
        }

        public bool DeleteChildValue_Date(DateTime value)
        {
            lock (Database)
            {
                SGNode nodeToDelete = null;
                foreach (var childNode in dictChildNode.Values)
                {
                    if(childNode.NodeType != NodeType.String)
                        continue;

                    if (childNode.DateValue == value.Date)
                    {
                        nodeToDelete = childNode;
                        break;
                    }
                }

                if (nodeToDelete != null)
                {
                    nodeToDelete.Delete();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int DeleteAllChildValue_Date(Predicate<DateTime> match)
        {
            return DeleteAllChild(childNode =>
            {
                if (childNode.NodeType != NodeType.String)
                    return false;

                return match(childNode.DateValue);
            });
        }
        
        
        
        public bool TryGetChildValue_Date(string childKey, DateTime defaultValue, out DateTime value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetDate(defaultValue);
                return true;
            }
            else
            {
                value = defaultValue;
                return false;
            }
        }
        
        public bool TryGetChildValue_Date(string childKey, out DateTime value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetDate(DefaultDate);
                return true;
            }
            else
            {
                value = DefaultDate;
                return false;
            }
        }
        
        #endregion
        
        
        #region Time
        
        public bool HasChildValue_Time(DateTime value)
        {
            lock (Database)
            {
                InitializeIfNot();
                
                foreach (var v in dictChildNode.Values)
                {
                    if (v.EqualsTime(value))
                        return true;
                }

                return false;
            }
        }

        public SGNode AddChildValue_Time(DateTime value)
        {
            lock (Database)
            {
                ChildKeySeq++;
                while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                {
                    ChildKeySeq++;
                }
            
                var node = this[ChildKeySeq.ToString()];
                node.SetTime(value);
                return node;
            }
        }
        
        public void AddChildValueRange_Time(IEnumerable<DateTime> values)
        {
            lock (Database)
            {
                foreach (var value in values)
                {
                    ChildKeySeq++;
                    while (dictChildNode.ContainsKey(ChildKeySeq.ToString()))
                    {
                        ChildKeySeq++;
                    }
            
                    var node = this[ChildKeySeq.ToString()];
                    node.SetTime(value);
                }
            }
        }

        public bool DeleteChildValue_Time(DateTime value)
        {
            lock (Database)
            {
                SGNode nodeToDelete = null;
                foreach (var childNode in dictChildNode.Values)
                {
                    if(childNode.NodeType != NodeType.String)
                        continue;

                    if (childNode.EqualsTime(value))
                    {
                        nodeToDelete = childNode;
                        break;
                    }
                }

                if (nodeToDelete != null)
                {
                    nodeToDelete.Delete();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int DeleteAllChildValue_Time(Predicate<DateTime> match)
        {
            return DeleteAllChild(childNode =>
            {
                if (childNode.NodeType != NodeType.String)
                    return false;

                return match(childNode.TimeValue);
            });
        }
        
        
        
        public bool TryGetChildValue_Time(string childKey, DateTime defaultValue, out DateTime value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetTime(defaultValue);
                return true;
            }
            else
            {
                value = defaultValue;
                return false;
            }
        }
        
        public bool TryGetChildValue_Time(string childKey, out DateTime value)
        {
            if (TryGetChild(childKey, out var childNode))
            {
                value = childNode.GetTime(DefaultTime);
                return true;
            }
            else
            {
                value = DefaultTime;
                return false;
            }
        }
        
        #endregion
    }
}