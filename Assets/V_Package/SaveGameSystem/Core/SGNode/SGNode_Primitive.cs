using System;
using System.Globalization;
using UnityEngine;
using VPackage.SaveGameSystem.Security;

namespace VPackage.SaveGameSystem
{
    public partial class SGNode
    {
         //If node is primitive

        internal EString stringValue;
        internal object numberValue;


        public static readonly DateTime DefaultDateTime = DateTime.MinValue;
        public static readonly DateTime DefaultDate = DateTime.MinValue.Date;
        public static readonly DateTime DefaultTime = DateTime.MinValue.Date;
        

        #region String
        
        public string GetString(string defaultValue)
        {
            if (NodeType == NodeType.Undefined)
                return defaultValue;
            
            if(NodeType != NodeType.String)
            {
                throw new Exception("Node value is not string");
                //return null;
            }

            return stringValue.Value;
        }

        public string GetString()
        {
            return GetString("");
        }

        public void SetString(string value)
        {
            lock (Database)
            {
                NodeType = NodeType.String;
                stringValue.Value = value;
            
                if(Parent == null)
                    Database.StorageService.SetDatabaseChanged(Database);
                else
                    Database.StorageService.SetNodeChanged(Parent);
            }
        }

        public string StringValue
        {
            get => GetString("");
            set => SetString(value);
        }
        
        #endregion
        
        #region Int

        public int GetInt(int defaultValue)
        {
            if (NodeType == NodeType.Undefined)
                return defaultValue;
            
            if(IsPrimitive == false)
                throw new Exception("Node value is not primitive");

            if(NodeType == NodeType.String)
                throw new Exception("Node value is not number");
            

            
            if (NodeType == NodeType.Int)
                return ((EInt)numberValue).Value;

            if (NodeType == NodeType.Float)
                return (int)((EFloat)numberValue).Value;
            
            if (NodeType == NodeType.Long)
                return (int)((ELong)numberValue).Value;
            
            if (NodeType == NodeType.Decimal)
                return (int)((EDecimal)numberValue).Value;
            
            if(NodeType == NodeType.Bool)
                return ((EInt)numberValue).Value;
            
            
            
            throw new Exception("Not programmed for this value type yet: " + NodeType);
        }

        public int GetInt()
        {
            return GetInt(0);
        }

        public void SetInt(int value)
        {
            lock (Database)
            {
                NodeType = NodeType.Int;
                numberValue = new EInt(value);
            
                if(Parent == null)
                    Database.StorageService.SetDatabaseChanged(Database);
                else
                    Database.StorageService.SetNodeChanged(Parent);
            }
        }
        
        public int IntValue
        {
            get => GetInt(0);
            set => SetInt(value);
        }
        
        #endregion
        
        #region Float

        public float GetFloat(float defaultValue)
        {
            if (NodeType == NodeType.Undefined)
                return defaultValue;
            
            if(IsPrimitive == false)
                throw new Exception("Node value is not primitive");

            if(NodeType == NodeType.String)
                throw new Exception("Node value is not number");

            

            if (NodeType == NodeType.Float)
                return ((EFloat)numberValue).Value;

            if (NodeType == NodeType.Int)
                return ((EInt)numberValue).Value;
            
            if (NodeType == NodeType.Long)
                return ((ELong)numberValue).Value;
            
            if (NodeType == NodeType.Decimal)
                return (float)((EDecimal)numberValue).Value;
            
            if(NodeType == NodeType.Bool)
                return ((EInt)numberValue).Value;
            
            
            
            throw new Exception("Not programmed for this value type yet: " + NodeType);
        }

        public float GetFloat()
        {
            return GetFloat(0f);
        }
        
        public void SetFloat(float value)
        {
            lock (Database)
            {
                NodeType = NodeType.Float;
                numberValue = new EFloat(value);
            
                if(Parent == null)
                    Database.StorageService.SetDatabaseChanged(Database);
                else
                    Database.StorageService.SetNodeChanged(Parent);
            }
        }
        
        public float FloatValue
        {
            get => GetFloat(0);
            set => SetFloat(value);
        }
        
        #endregion
        
        #region Long
        
        public long GetLong(long defaultValue)
        {
            if (NodeType == NodeType.Undefined)
                return defaultValue;
            
            if(IsPrimitive == false)
                throw new Exception("Node value is not primitive");

            if(NodeType == NodeType.String)
                throw new Exception("Node value is not number");


            
            if (NodeType == NodeType.Long)
                return ((ELong)numberValue).Value;
            
            if (NodeType == NodeType.Int)
                return ((EInt)numberValue).Value;
            
            if (NodeType == NodeType.Float)
                return (long)((EFloat)numberValue).Value;
            
            if (NodeType == NodeType.Decimal)
                return (long)((EDecimal)numberValue).Value;
            
            if(NodeType == NodeType.Bool)
                return ((EInt)numberValue).Value;
            
            

            throw new Exception("Not programmed for this value type yet: " + NodeType);
        }

        public long GetLong()
        {
            return GetLong(0);
        }
        
        public void SetLong(long value)
        {
            lock (Database)
            {
                NodeType = NodeType.Long;
                numberValue = new ELong(value);
            
                if(Parent == null)
                    Database.StorageService.SetDatabaseChanged(Database);
                else
                    Database.StorageService.SetNodeChanged(Parent);
            }
        }
        
        public long LongValue
        {
            get => GetLong(0);
            set => SetLong(value);
        }
        
        #endregion
        
        #region Decimal

        public decimal GetDecimal(decimal defaultValue)
        {
            if (NodeType == NodeType.Undefined)
                return defaultValue;
            
            if(IsPrimitive == false)
                throw new Exception("Node value is not primitive");

            if(NodeType == NodeType.String)
                throw new Exception("Node value is not number");

            
            
            if (NodeType == NodeType.Decimal)
                return ((EDecimal)numberValue).Value;
            
            if (NodeType == NodeType.Int)
                return ((EInt)numberValue).Value;
            
            if (NodeType == NodeType.Float)
                return (decimal)((EFloat)numberValue).Value;
            
            if (NodeType == NodeType.Long)
                return ((ELong)numberValue).Value;
            
            
            
            if(NodeType == NodeType.Bool)
                return ((EInt)numberValue).Value;
            
            
            
            throw new Exception("Not programmed for this value type yet: " + NodeType);
        }

        public decimal GetDecimal()
        {
            return GetDecimal(0m);
        }
        
        public void SetDecimal(decimal value)
        {
            lock (Database)
            {
                NodeType = NodeType.Decimal;
                numberValue = new EDecimal(value);
            
                if(Parent == null)
                    Database.StorageService.SetDatabaseChanged(Database);
                else
                    Database.StorageService.SetNodeChanged(Parent);
            }
        }
        
        public decimal DecimalValue
        {
            get => GetDecimal(0m);
            set => SetDecimal(value);
        }
        
        #endregion

        #region Bool

        public bool GetBool(bool defaultValue)
        {
            if (NodeType == NodeType.Undefined)
                return defaultValue;
            
            if(IsPrimitive == false)
                throw new Exception("Node value is not primitive");

            if(NodeType == NodeType.String)
                throw new Exception("Node value is not number");
            

            
            if(NodeType == NodeType.Bool)
                return ((EInt)numberValue).Value != 0;
            
            if (NodeType == NodeType.Int)
                return ((EInt)numberValue).Value != 0;

            if (NodeType == NodeType.Float)
                return (int)((EFloat)numberValue).Value != 0;
            
            if (NodeType == NodeType.Long)
                return (int)((ELong)numberValue).Value != 0;
            
            if (NodeType == NodeType.Decimal)
                return (int)((EDecimal)numberValue).Value != 0;
            
            
            
            throw new Exception("Not programmed for this value type yet: " + NodeType);
        }

        public bool GetBool()
        {
            return GetBool(false);
        }

        public void SetBool(bool value)
        {
            lock (Database)
            {
                NodeType = NodeType.Bool;
                numberValue = new EInt(value ? 1 : 0);
            
                if(Parent == null)
                    Database.StorageService.SetDatabaseChanged(Database);
                else
                    Database.StorageService.SetNodeChanged(Parent);
            }
        }

        public bool BoolValue
        {
            get => GetBool(false);
            set => SetBool(value);
        }

        #endregion

        #region DateTime

        public DateTime GetDateTime(DateTime defaultValue)
        {
            string s = GetString("");
            if (string.IsNullOrEmpty(s))
                return defaultValue;

            if (DateTime.TryParseExact(s, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result1))
                return result1;
            
            if (DateTime.TryParseExact(s, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result2))
                return result2;
            
            if (DateTime.TryParseExact(s, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result3))
                return result3;
            
            throw new FormatException("String was not recognized as a valid DateTime.");
        }

        /// <summary>
        /// If empty, will return DateTime.MinValue
        /// </summary>
        public DateTime GetDateTime()
        {
            return GetDateTime(DefaultDateTime);
        }

        public void SetDateTime(DateTime value)
        {
            string s = value.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            SetString(s);
        }

        public DateTime DateTimeValue
        {
            get => GetDateTime(DefaultDateTime);
            set => SetDateTime(value);
        }
        
        #endregion

        #region Date

        public DateTime GetDate(DateTime defaultValue)
        {
            string s = GetString("");
            if (string.IsNullOrEmpty(s))
                return defaultValue;
            
            if (DateTime.TryParseExact(s, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result1))
                return result1;
            
            if (DateTime.TryParseExact(s, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result2))
                return result2.Date;

            if (DateTime.TryParseExact(s, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result3))
                return result3.Date;
            
            throw new FormatException("String was not recognized as a valid DateTime.");
        }

        /// <summary>
        /// If empty, will return DateTime.MinValue
        /// </summary>
        public DateTime GetDate()
        {
            return GetDate(DefaultDate);
        }

        public void SetDate(DateTime value)
        {
            string s = value.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
            SetString(s);
        }

        public DateTime DateValue
        {
            get => GetDate(DefaultDate);
            set => SetDate(value);
        }

        #endregion

        #region Time

        public DateTime GetTime(DateTime defaultValue)
        {
            string s = GetString("");
            if (string.IsNullOrEmpty(s))
                return defaultValue;
            
            if (DateTime.TryParseExact(s, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result1))
                return result1;
            
            if (DateTime.TryParseExact(s, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result2))
                return result2;
            
            if (DateTime.TryParseExact(s, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result3))
                return result3;

            throw new FormatException("String was not recognized as a valid DateTime.");
        }

        /// <summary>
        /// If empty, will return DateTime.MinValue
        /// </summary>
        public DateTime GetTime()
        {
            return GetTime(DefaultTime);
        }

        public void SetTime(DateTime value)
        {
            string s = value.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            SetString(s);
        }

        public DateTime TimeValue
        {
            get => GetTime(DefaultTime);
            set => SetTime(value);
        }

        public bool EqualsTime(DateTime otherTimeValue)
        {
            var timeValue = GetTime();
            return timeValue.Hour == otherTimeValue.Hour &&
                   timeValue.Minute == otherTimeValue.Minute &&
                   timeValue.Second == otherTimeValue.Second;
        }
        
        #endregion
    }
}