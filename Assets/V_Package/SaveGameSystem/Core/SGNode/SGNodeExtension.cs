/*using System;

namespace VPackage.SaveGameSystem
{
    public static class SGNodeExtension
    {
        #region Try Get Child String Value

        public static string TryGetChild_String(this SGNode node, string childKey, string defaultValue)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetString(defaultValue);
            else
                return defaultValue;
        }
        
        public static string TryGetChild_String(this SGNode node, string childKey)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetString("");
            else
                return "";
        }

        #endregion
        
        #region Try Get Child Int Value

        public static int TryGetChild_Int(this SGNode node, string childKey, int defaultValue)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetInt(defaultValue);
            else
                return defaultValue;
        }
        
        public static int TryGetChild_Int(this SGNode node, string childKey)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetInt(0);
            else
                return 0;
        }

        #endregion
        
        #region Try Get Child Float Value

        public static float TryGetChild_Float(this SGNode node, string childKey, float defaultValue)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetFloat(defaultValue);
            else
                return defaultValue;
        }
        
        public static float TryGetChild_Float(this SGNode node, string childKey)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetFloat(0f);
            else
                return 0f;
        }

        #endregion
        
        #region Try Get Child Long Value

        public static long TryGetChild_Long(this SGNode node, string childKey, long defaultValue)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetLong(defaultValue);
            else
                return defaultValue;
        }
        
        public static long TryGetChild_Long(this SGNode node, string childKey)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetLong(0L);
            else
                return 0L;
        }

        #endregion
        
        #region Try Get Child Bool Value

        public static bool TryGetChild_Bool(this SGNode node, string childKey, bool defaultValue)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetBool(defaultValue);
            else
                return defaultValue;
        }
        
        public static bool TryGetChild_Bool(this SGNode node, string childKey)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetBool(false);
            else
                return false;
        }

        #endregion
        
        #region Try Get Child DateTime

        public static DateTime TryGetChild_DateTime(this SGNode node, string childKey, DateTime defaultValue)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetDateTime(defaultValue);
            else
                return defaultValue;
        }
        
        public static DateTime TryGetChild_DateTime(this SGNode node, string childKey)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetDateTime(DateTime.MinValue);
            else
                return DateTime.MinValue;
        }

        #endregion
        
        #region Try Get Child Date

        public static DateTime TryGetChild_Date(this SGNode node, string childKey, DateTime defaultValue)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetDate(defaultValue);
            else
                return defaultValue;
        }
        
        public static DateTime TryGetChild_Date(this SGNode node, string childKey)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetDate(DateTime.MinValue);
            else
                return DateTime.MinValue;
        }

        #endregion
        
        #region Try Get Child Time

        public static DateTime TryGetChild_Time(this SGNode node, string childKey, DateTime defaultValue)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetTime(defaultValue);
            else
                return defaultValue;
        }
        
        public static DateTime TryGetChild_Time(this SGNode node, string childKey)
        {
            if (node.TryGetChild(childKey, out var childNode))
                return childNode.GetTime(DateTime.MinValue);
            else
                return DateTime.MinValue;
        }

        #endregion
    }
}*/