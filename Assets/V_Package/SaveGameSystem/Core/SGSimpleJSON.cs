using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace VPackage.SaveGameSystem
{
    public enum SGJsonNodeType
    {
        Array = 1,
        Object = 2,
        String = 3,
        Number = 4,
        NullValue = 5,
        Boolean = 6,
        None = 7,
        Custom = 0xFF,
    }
    public enum SGJsonTextMode
    {
        Compact,
        Indent
    }

    public abstract partial class SGJsonNode
    {
        #region Enumerators
        public struct Enumerator
        {
            private enum Type { None, Array, Object }
            private Type type;
            private Dictionary<string, SGJsonNode>.Enumerator m_Object;
            private List<SGJsonNode>.Enumerator m_Array;
            public bool IsValid { get { return type != Type.None; } }
            public Enumerator(List<SGJsonNode>.Enumerator aArrayEnum)
            {
                type = Type.Array;
                m_Object = default(Dictionary<string, SGJsonNode>.Enumerator);
                m_Array = aArrayEnum;
            }
            public Enumerator(Dictionary<string, SGJsonNode>.Enumerator aDictEnum)
            {
                type = Type.Object;
                m_Object = aDictEnum;
                m_Array = default(List<SGJsonNode>.Enumerator);
            }
            public KeyValuePair<string, SGJsonNode> Current
            {
                get
                {
                    if (type == Type.Array)
                        return new KeyValuePair<string, SGJsonNode>(string.Empty, m_Array.Current);
                    else if (type == Type.Object)
                        return m_Object.Current;
                    return new KeyValuePair<string, SGJsonNode>(string.Empty, null);
                }
            }
            public bool MoveNext()
            {
                if (type == Type.Array)
                    return m_Array.MoveNext();
                else if (type == Type.Object)
                    return m_Object.MoveNext();
                return false;
            }
        }
        public struct ValueEnumerator
        {
            private Enumerator m_Enumerator;
            public ValueEnumerator(List<SGJsonNode>.Enumerator aArrayEnum) : this(new Enumerator(aArrayEnum)) { }
            public ValueEnumerator(Dictionary<string, SGJsonNode>.Enumerator aDictEnum) : this(new Enumerator(aDictEnum)) { }
            public ValueEnumerator(Enumerator aEnumerator) { m_Enumerator = aEnumerator; }
            public SGJsonNode Current { get { return m_Enumerator.Current.Value; } }
            public bool MoveNext() { return m_Enumerator.MoveNext(); }
            public ValueEnumerator GetEnumerator() { return this; }
        }
        public struct KeyEnumerator
        {
            private Enumerator m_Enumerator;
            public KeyEnumerator(List<SGJsonNode>.Enumerator aArrayEnum) : this(new Enumerator(aArrayEnum)) { }
            public KeyEnumerator(Dictionary<string, SGJsonNode>.Enumerator aDictEnum) : this(new Enumerator(aDictEnum)) { }
            public KeyEnumerator(Enumerator aEnumerator) { m_Enumerator = aEnumerator; }
            public string Current { get { return m_Enumerator.Current.Key; } }
            public bool MoveNext() { return m_Enumerator.MoveNext(); }
            public KeyEnumerator GetEnumerator() { return this; }
        }

        public class LinqEnumerator : IEnumerator<KeyValuePair<string, SGJsonNode>>, IEnumerable<KeyValuePair<string, SGJsonNode>>
        {
            private SGJsonNode m_Node;
            private Enumerator m_Enumerator;
            internal LinqEnumerator(SGJsonNode aNode)
            {
                m_Node = aNode;
                if (m_Node != null)
                    m_Enumerator = m_Node.GetEnumerator();
            }
            public KeyValuePair<string, SGJsonNode> Current { get { return m_Enumerator.Current; } }
            object IEnumerator.Current { get { return m_Enumerator.Current; } }
            public bool MoveNext() { return m_Enumerator.MoveNext(); }

            public void Dispose()
            {
                m_Node = null;
                m_Enumerator = new Enumerator();
            }

            public IEnumerator<KeyValuePair<string, SGJsonNode>> GetEnumerator()
            {
                return new LinqEnumerator(m_Node);
            }

            public void Reset()
            {
                if (m_Node != null)
                    m_Enumerator = m_Node.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new LinqEnumerator(m_Node);
            }
        }

        #endregion Enumerators

        #region common interface

        public static bool forceASCII = false; // Use Unicode by default
        public static bool longAsString = false; // lazy creator creates a JSONString instead of JSONNumber
        public static bool allowLineComments = true; // allow "//"-style comments at the end of a line

        public abstract SGJsonNodeType Tag { get; }

        public virtual SGJsonNode this[int aIndex] { get { return null; } set { } }

        public virtual SGJsonNode this[string aKey] { get { return null; } set { } }

        public virtual string Value { get { return ""; } set { } }

        public virtual int Count { get { return 0; } }

        public virtual bool IsNumber { get { return false; } }
        public virtual bool IsString { get { return false; } }
        public virtual bool IsBoolean { get { return false; } }
        public virtual bool IsNull { get { return false; } }
        public virtual bool IsArray { get { return false; } }
        public virtual bool IsObject { get { return false; } }

        public virtual bool Inline { get { return false; } set { } }

        public virtual void Add(string aKey, SGJsonNode aItem)
        {
        }
        public virtual void Add(SGJsonNode aItem)
        {
            Add("", aItem);
        }

        public virtual SGJsonNode Remove(string aKey)
        {
            return null;
        }

        public virtual SGJsonNode Remove(int aIndex)
        {
            return null;
        }

        public virtual SGJsonNode Remove(SGJsonNode aNode)
        {
            return aNode;
        }
        public virtual void Clear() { }

        public virtual SGJsonNode Clone()
        {
            return null;
        }

        public virtual IEnumerable<SGJsonNode> Children
        {
            get
            {
                yield break;
            }
        }

        public IEnumerable<SGJsonNode> DeepChildren
        {
            get
            {
                foreach (var C in Children)
                    foreach (var D in C.DeepChildren)
                        yield return D;
            }
        }

        public virtual bool HasKey(string aKey)
        {
            return false;
        }

        public virtual SGJsonNode GetValueOrDefault(string aKey, SGJsonNode aDefault)
        {
            return aDefault;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            WriteToStringBuilder(sb, 0, 0, SGJsonTextMode.Compact);
            return sb.ToString();
        }

        public virtual string ToString(int aIndent)
        {
            StringBuilder sb = new StringBuilder();
            WriteToStringBuilder(sb, 0, aIndent, SGJsonTextMode.Indent);
            return sb.ToString();
        }
        internal abstract void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, SGJsonTextMode aMode);

        public abstract Enumerator GetEnumerator();
        public IEnumerable<KeyValuePair<string, SGJsonNode>> Linq { get { return new LinqEnumerator(this); } }
        public KeyEnumerator Keys { get { return new KeyEnumerator(GetEnumerator()); } }
        public ValueEnumerator Values { get { return new ValueEnumerator(GetEnumerator()); } }

        #endregion common interface

        #region typecasting properties


        public virtual double AsDouble
        {
            get
            {
                double v = 0.0;
                if (double.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                    return v;
                return 0.0;
            }
            set
            {
                Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public virtual int AsInt
        {
            get { return (int)AsDouble; }
            set { AsDouble = value; }
        }

        public virtual float AsFloat
        {
            get { return (float)AsDouble; }
            set { AsDouble = value; }
        }

        public virtual bool AsBool
        {
            get
            {
                bool v = false;
                if (bool.TryParse(Value, out v))
                    return v;
                return !string.IsNullOrEmpty(Value);
            }
            set
            {
                Value = (value) ? "true" : "false";
            }
        }

        public virtual long AsLong
        {
            get
            {
                long val = 0;
                if (long.TryParse(Value, out val))
                    return val;
                return 0L;
            }
            set
            {
                Value = value.ToString();
            }
        }

        public virtual ulong AsULong
        {
            get
            {
                ulong val = 0;
                if (ulong.TryParse(Value, out val))
                    return val;
                return 0;
            }
            set
            {
                Value = value.ToString();
            }
        }
        
        #region Decimal
        public virtual decimal AsDecimal
        {
            get
            {
                decimal result;
                if (!decimal.TryParse(Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result))
                    result = 0;
                return result;
            }
            set
            {
                Value = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public static implicit operator SGJsonNode(decimal aDecimal)
        {
            return new SGJsonString(aDecimal.ToString(CultureInfo.InvariantCulture));
        }

        public static implicit operator decimal(SGJsonNode aNode)
        {
            return aNode.AsDecimal;
        }
        #endregion Decimal

        public virtual SGJsonArray AsArray
        {
            get
            {
                return this as SGJsonArray;
            }
        }

        public virtual SGJsonObject AsObject
        {
            get
            {
                return this as SGJsonObject;
            }
        }


        #endregion typecasting properties

        #region operators

        public static implicit operator SGJsonNode(string s)
        {
            return (s == null) ? (SGJsonNode) SGJsonNull.CreateOrGet() : new SGJsonString(s);
        }
        public static implicit operator string(SGJsonNode d)
        {
            return (d == null) ? null : d.Value;
        }

        public static implicit operator SGJsonNode(double n)
        {
            return new SGJsonNumber(n);
        }
        public static implicit operator double(SGJsonNode d)
        {
            return (d == null) ? 0 : d.AsDouble;
        }

        public static implicit operator SGJsonNode(float n)
        {
            return new SGJsonNumber(n);
        }
        public static implicit operator float(SGJsonNode d)
        {
            return (d == null) ? 0 : d.AsFloat;
        }

        public static implicit operator SGJsonNode(int n)
        {
            return new SGJsonNumber(n);
        }
        public static implicit operator int(SGJsonNode d)
        {
            return (d == null) ? 0 : d.AsInt;
        }

        public static implicit operator SGJsonNode(long n)
        {
            if (longAsString)
                return new SGJsonString(n.ToString());
            return new SGJsonNumber(n);
        }
        public static implicit operator long(SGJsonNode d)
        {
            return (d == null) ? 0L : d.AsLong;
        }

        public static implicit operator SGJsonNode(ulong n)
        {
            if (longAsString)
                return new SGJsonString(n.ToString());
            return new SGJsonNumber(n);
        }
        public static implicit operator ulong(SGJsonNode d)
        {
            return (d == null) ? 0 : d.AsULong;
        }

        public static implicit operator SGJsonNode(bool b)
        {
            return new SGJsonBool(b);
        }
        public static implicit operator bool(SGJsonNode d)
        {
            return (d == null) ? false : d.AsBool;
        }

        public static implicit operator SGJsonNode(KeyValuePair<string, SGJsonNode> aKeyValue)
        {
            return aKeyValue.Value;
        }

        public static bool operator ==(SGJsonNode a, object b)
        {
            if (ReferenceEquals(a, b))
                return true;
            bool aIsNull = a is SGJsonNull || ReferenceEquals(a, null) || a is SGJsonLazyCreator;
            bool bIsNull = b is SGJsonNull || ReferenceEquals(b, null) || b is SGJsonLazyCreator;
            if (aIsNull && bIsNull)
                return true;
            return !aIsNull && a.Equals(b);
        }

        public static bool operator !=(SGJsonNode a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion operators

        [ThreadStatic]
        private static StringBuilder m_EscapeBuilder;
        internal static StringBuilder EscapeBuilder
        {
            get
            {
                if (m_EscapeBuilder == null)
                    m_EscapeBuilder = new StringBuilder();
                return m_EscapeBuilder;
            }
        }
        internal static string Escape(string aText)
        {
            var sb = EscapeBuilder;
            sb.Length = 0;
            if (sb.Capacity < aText.Length + aText.Length / 10)
                sb.Capacity = aText.Length + aText.Length / 10;
            foreach (char c in aText)
            {
                switch (c)
                {
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    default:
                        if (c < ' ' || (forceASCII && c > 127))
                        {
                            ushort val = c;
                            sb.Append("\\u").Append(val.ToString("X4"));
                        }
                        else
                            sb.Append(c);
                        break;
                }
            }
            string result = sb.ToString();
            sb.Length = 0;
            return result;
        }

        private static SGJsonNode ParseElement(string token, bool quoted)
        {
            if (quoted)
                return token;
            if (token.Length <= 5)
            {
                string tmp = token.ToLower();
                if (tmp == "false" || tmp == "true")
                    return tmp == "true";
                if (tmp == "null")
                    return SGJsonNull.CreateOrGet();
            }
            double val;
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                return val;
            else
                return token;
        }

        public static SGJsonNode Parse(string aJSON)
        {
            Stack<SGJsonNode> stack = new Stack<SGJsonNode>();
            SGJsonNode ctx = null;
            int i = 0;
            StringBuilder Token = new StringBuilder();
            string TokenName = "";
            bool QuoteMode = false;
            bool TokenIsQuoted = false;
            bool HasNewlineChar = false;
            while (i < aJSON.Length)
            {
                switch (aJSON[i])
                {
                    case '{':
                        if (QuoteMode)
                        {
                            Token.Append(aJSON[i]);
                            break;
                        }
                        stack.Push(new SGJsonObject());
                        if (ctx != null)
                        {
                            ctx.Add(TokenName, stack.Peek());
                        }
                        TokenName = "";
                        Token.Length = 0;
                        ctx = stack.Peek();
                        HasNewlineChar = false;
                        break;

                    case '[':
                        if (QuoteMode)
                        {
                            Token.Append(aJSON[i]);
                            break;
                        }

                        stack.Push(new SGJsonArray());
                        if (ctx != null)
                        {
                            ctx.Add(TokenName, stack.Peek());
                        }
                        TokenName = "";
                        Token.Length = 0;
                        ctx = stack.Peek();
                        HasNewlineChar = false;
                        break;

                    case '}':
                    case ']':
                        if (QuoteMode)
                        {

                            Token.Append(aJSON[i]);
                            break;
                        }
                        if (stack.Count == 0)
                            throw new Exception("JSON Parse: Too many closing brackets");

                        stack.Pop();
                        if (Token.Length > 0 || TokenIsQuoted)
                            ctx.Add(TokenName, ParseElement(Token.ToString(), TokenIsQuoted));
                        if (ctx != null)
                            ctx.Inline = !HasNewlineChar;
                        TokenIsQuoted = false;
                        TokenName = "";
                        Token.Length = 0;
                        if (stack.Count > 0)
                            ctx = stack.Peek();
                        break;

                    case ':':
                        if (QuoteMode)
                        {
                            Token.Append(aJSON[i]);
                            break;
                        }
                        TokenName = Token.ToString();
                        Token.Length = 0;
                        TokenIsQuoted = false;
                        break;

                    case '"':
                        QuoteMode ^= true;
                        TokenIsQuoted |= QuoteMode;
                        break;

                    case ',':
                        if (QuoteMode)
                        {
                            Token.Append(aJSON[i]);
                            break;
                        }
                        if (Token.Length > 0 || TokenIsQuoted)
                            ctx.Add(TokenName, ParseElement(Token.ToString(), TokenIsQuoted));
                        TokenIsQuoted = false;
                        TokenName = "";
                        Token.Length = 0;
                        TokenIsQuoted = false;
                        break;

                    case '\r':
                    case '\n':
                        HasNewlineChar = true;
                        break;

                    case ' ':
                    case '\t':
                        if (QuoteMode)
                            Token.Append(aJSON[i]);
                        break;

                    case '\\':
                        ++i;
                        if (QuoteMode)
                        {
                            char C = aJSON[i];
                            switch (C)
                            {
                                case 't':
                                    Token.Append('\t');
                                    break;
                                case 'r':
                                    Token.Append('\r');
                                    break;
                                case 'n':
                                    Token.Append('\n');
                                    break;
                                case 'b':
                                    Token.Append('\b');
                                    break;
                                case 'f':
                                    Token.Append('\f');
                                    break;
                                case 'u':
                                    {
                                        string s = aJSON.Substring(i + 1, 4);
                                        Token.Append((char)int.Parse(
                                            s,
                                            System.Globalization.NumberStyles.AllowHexSpecifier));
                                        i += 4;
                                        break;
                                    }
                                default:
                                    Token.Append(C);
                                    break;
                            }
                        }
                        break;
                    case '/':
                        if (allowLineComments && !QuoteMode && i + 1 < aJSON.Length && aJSON[i + 1] == '/')
                        {
                            while (++i < aJSON.Length && aJSON[i] != '\n' && aJSON[i] != '\r') ;
                            break;
                        }
                        Token.Append(aJSON[i]);
                        break;
                    case '\uFEFF': // remove / ignore BOM (Byte Order Mark)
                        break;

                    default:
                        Token.Append(aJSON[i]);
                        break;
                }
                ++i;
            }
            if (QuoteMode)
            {
                throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
            }
            if (ctx == null)
                return ParseElement(Token.ToString(), TokenIsQuoted);
            return ctx;
        }

    }
    // End of JSONNode

    public partial class SGJsonArray : SGJsonNode
    {
        private List<SGJsonNode> m_List = new List<SGJsonNode>();
        private bool inline = false;
        public override bool Inline
        {
            get { return inline; }
            set { inline = value; }
        }

        public override SGJsonNodeType Tag { get { return SGJsonNodeType.Array; } }
        public override bool IsArray { get { return true; } }
        public override Enumerator GetEnumerator() { return new Enumerator(m_List.GetEnumerator()); }

        public override SGJsonNode this[int aIndex]
        {
            get
            {
                if (aIndex < 0 || aIndex >= m_List.Count)
                    return new SGJsonLazyCreator(this);
                return m_List[aIndex];
            }
            set
            {
                if (value == null)
                    value = SGJsonNull.CreateOrGet();
                if (aIndex < 0 || aIndex >= m_List.Count)
                    m_List.Add(value);
                else
                    m_List[aIndex] = value;
            }
        }

        public override SGJsonNode this[string aKey]
        {
            get { return new SGJsonLazyCreator(this); }
            set
            {
                if (value == null)
                    value = SGJsonNull.CreateOrGet();
                m_List.Add(value);
            }
        }

        public override int Count
        {
            get { return m_List.Count; }
        }

        public override void Add(string aKey, SGJsonNode aItem)
        {
            if (aItem == null)
                aItem = SGJsonNull.CreateOrGet();
            m_List.Add(aItem);
        }

        public override SGJsonNode Remove(int aIndex)
        {
            if (aIndex < 0 || aIndex >= m_List.Count)
                return null;
            SGJsonNode tmp = m_List[aIndex];
            m_List.RemoveAt(aIndex);
            return tmp;
        }

        public override SGJsonNode Remove(SGJsonNode aNode)
        {
            m_List.Remove(aNode);
            return aNode;
        }

        public override void Clear()
        {
            m_List.Clear();
        }

        public override SGJsonNode Clone()
        {
            var node = new SGJsonArray();
            node.m_List.Capacity = m_List.Capacity;
            foreach(var n in m_List)
            {
                if (n != null)
                    node.Add(n.Clone());
                else
                    node.Add(null);
            }
            return node;
        }

        public override IEnumerable<SGJsonNode> Children
        {
            get
            {
                foreach (SGJsonNode N in m_List)
                    yield return N;
            }
        }


        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, SGJsonTextMode aMode)
        {
            aSB.Append('[');
            int count = m_List.Count;
            if (inline)
                aMode = SGJsonTextMode.Compact;
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                    aSB.Append(',');
                if (aMode == SGJsonTextMode.Indent)
                    aSB.AppendLine();

                if (aMode == SGJsonTextMode.Indent)
                    aSB.Append(' ', aIndent + aIndentInc);
                m_List[i].WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
            }
            if (aMode == SGJsonTextMode.Indent)
                aSB.AppendLine().Append(' ', aIndent);
            aSB.Append(']');
        }
    }
    // End of JSONArray

    public partial class SGJsonObject : SGJsonNode
    {
        private Dictionary<string, SGJsonNode> m_Dict = new Dictionary<string, SGJsonNode>();

        private bool inline = false;
        public override bool Inline
        {
            get { return inline; }
            set { inline = value; }
        }

        public override SGJsonNodeType Tag { get { return SGJsonNodeType.Object; } }
        public override bool IsObject { get { return true; } }

        public override Enumerator GetEnumerator() { return new Enumerator(m_Dict.GetEnumerator()); }


        public override SGJsonNode this[string aKey]
        {
            get
            {
                if (m_Dict.ContainsKey(aKey))
                    return m_Dict[aKey];
                else
                    return new SGJsonLazyCreator(this, aKey);
            }
            set
            {
                if (value == null)
                    value = SGJsonNull.CreateOrGet();
                if (m_Dict.ContainsKey(aKey))
                    m_Dict[aKey] = value;
                else
                    m_Dict.Add(aKey, value);
            }
        }

        public override SGJsonNode this[int aIndex]
        {
            get
            {
                if (aIndex < 0 || aIndex >= m_Dict.Count)
                    return null;
                return m_Dict.ElementAt(aIndex).Value;
            }
            set
            {
                if (value == null)
                    value = SGJsonNull.CreateOrGet();
                if (aIndex < 0 || aIndex >= m_Dict.Count)
                    return;
                string key = m_Dict.ElementAt(aIndex).Key;
                m_Dict[key] = value;
            }
        }

        public override int Count
        {
            get { return m_Dict.Count; }
        }

        public override void Add(string aKey, SGJsonNode aItem)
        {
            if (aItem == null)
                aItem = SGJsonNull.CreateOrGet();

            if (aKey != null)
            {
                if (m_Dict.ContainsKey(aKey))
                    m_Dict[aKey] = aItem;
                else
                    m_Dict.Add(aKey, aItem);
            }
            else
                m_Dict.Add(Guid.NewGuid().ToString(), aItem);
        }

        public override SGJsonNode Remove(string aKey)
        {
            if (!m_Dict.ContainsKey(aKey))
                return null;
            SGJsonNode tmp = m_Dict[aKey];
            m_Dict.Remove(aKey);
            return tmp;
        }

        public override SGJsonNode Remove(int aIndex)
        {
            if (aIndex < 0 || aIndex >= m_Dict.Count)
                return null;
            var item = m_Dict.ElementAt(aIndex);
            m_Dict.Remove(item.Key);
            return item.Value;
        }

        public override SGJsonNode Remove(SGJsonNode aNode)
        {
            try
            {
                var item = m_Dict.Where(k => k.Value == aNode).First();
                m_Dict.Remove(item.Key);
                return aNode;
            }
            catch
            {
                return null;
            }
        }

        public override void Clear()
        {
            m_Dict.Clear();
        }

        public override SGJsonNode Clone()
        {
            var node = new SGJsonObject();
            foreach (var n in m_Dict)
            {
                node.Add(n.Key, n.Value.Clone());
            }
            return node;
        }

        public override bool HasKey(string aKey)
        {
            return m_Dict.ContainsKey(aKey);
        }

        public override SGJsonNode GetValueOrDefault(string aKey, SGJsonNode aDefault)
        {
            SGJsonNode res;
            if (m_Dict.TryGetValue(aKey, out res))
                return res;
            return aDefault;
        }

        public override IEnumerable<SGJsonNode> Children
        {
            get
            {
                foreach (KeyValuePair<string, SGJsonNode> N in m_Dict)
                    yield return N.Value;
            }
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, SGJsonTextMode aMode)
        {
            aSB.Append('{');
            bool first = true;
            if (inline)
                aMode = SGJsonTextMode.Compact;
            foreach (var k in m_Dict)
            {
                if (!first)
                    aSB.Append(',');
                first = false;
                if (aMode == SGJsonTextMode.Indent)
                    aSB.AppendLine();
                if (aMode == SGJsonTextMode.Indent)
                    aSB.Append(' ', aIndent + aIndentInc);
                aSB.Append('\"').Append(Escape(k.Key)).Append('\"');
                if (aMode == SGJsonTextMode.Compact)
                    aSB.Append(':');
                else
                    aSB.Append(" : ");
                k.Value.WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
            }
            if (aMode == SGJsonTextMode.Indent)
                aSB.AppendLine().Append(' ', aIndent);
            aSB.Append('}');
        }

    }
    // End of JSONObject

    public partial class SGJsonString : SGJsonNode
    {
        private string m_Data;

        public override SGJsonNodeType Tag { get { return SGJsonNodeType.String; } }
        public override bool IsString { get { return true; } }

        public override Enumerator GetEnumerator() { return new Enumerator(); }


        public override string Value
        {
            get { return m_Data; }
            set
            {
                m_Data = value;
            }
        }

        public SGJsonString(string aData)
        {
            m_Data = aData;
        }
        public override SGJsonNode Clone()
        {
            return new SGJsonString(m_Data);
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, SGJsonTextMode aMode)
        {
            aSB.Append('\"').Append(Escape(m_Data)).Append('\"');
        }
        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
                return true;
            string s = obj as string;
            if (s != null)
                return m_Data == s;
            SGJsonString s2 = obj as SGJsonString;
            if (s2 != null)
                return m_Data == s2.m_Data;
            return false;
        }
        public override int GetHashCode()
        {
            return m_Data.GetHashCode();
        }
        public override void Clear()
        {
            m_Data = "";
        }
    }
    // End of JSONString

    public partial class SGJsonNumber : SGJsonNode
    {
        private double m_Data;

        public override SGJsonNodeType Tag { get { return SGJsonNodeType.Number; } }
        public override bool IsNumber { get { return true; } }
        public override Enumerator GetEnumerator() { return new Enumerator(); }

        public override string Value
        {
            get { return m_Data.ToString(CultureInfo.InvariantCulture); }
            set
            {
                double v;
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                    m_Data = v;
            }
        }

        public override double AsDouble
        {
            get { return m_Data; }
            set { m_Data = value; }
        }
        public override long AsLong
        {
            get { return (long)m_Data; }
            set { m_Data = value; }
        }
        public override ulong AsULong
        {
            get { return (ulong)m_Data; }
            set { m_Data = value; }
        }

        public SGJsonNumber(double aData)
        {
            m_Data = aData;
        }

        public SGJsonNumber(string aData)
        {
            Value = aData;
        }

        public override SGJsonNode Clone()
        {
            return new SGJsonNumber(m_Data);
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, SGJsonTextMode aMode)
        {
            aSB.Append(Value);
        }
        private static bool IsNumeric(object value)
        {
            return value is int || value is uint
                || value is float || value is double
                || value is decimal
                || value is long || value is ulong
                || value is short || value is ushort
                || value is sbyte || value is byte;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (base.Equals(obj))
                return true;
            SGJsonNumber s2 = obj as SGJsonNumber;
            if (s2 != null)
                return m_Data == s2.m_Data;
            if (IsNumeric(obj))
                return Convert.ToDouble(obj) == m_Data;
            return false;
        }
        public override int GetHashCode()
        {
            return m_Data.GetHashCode();
        }
        public override void Clear()
        {
            m_Data = 0;
        }
    }
    // End of JSONNumber

    public partial class SGJsonBool : SGJsonNode
    {
        private bool m_Data;

        public override SGJsonNodeType Tag { get { return SGJsonNodeType.Boolean; } }
        public override bool IsBoolean { get { return true; } }
        public override Enumerator GetEnumerator() { return new Enumerator(); }

        public override string Value
        {
            get { return m_Data.ToString(); }
            set
            {
                bool v;
                if (bool.TryParse(value, out v))
                    m_Data = v;
            }
        }
        public override bool AsBool
        {
            get { return m_Data; }
            set { m_Data = value; }
        }

        public SGJsonBool(bool aData)
        {
            m_Data = aData;
        }

        public SGJsonBool(string aData)
        {
            Value = aData;
        }

        public override SGJsonNode Clone()
        {
            return new SGJsonBool(m_Data);
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, SGJsonTextMode aMode)
        {
            aSB.Append((m_Data) ? "true" : "false");
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is bool)
                return m_Data == (bool)obj;
            return false;
        }
        public override int GetHashCode()
        {
            return m_Data.GetHashCode();
        }
        public override void Clear()
        {
            m_Data = false;
        }
    }
    // End of JSONBool

    public partial class SGJsonNull : SGJsonNode
    {
        static SGJsonNull m_StaticInstance = new SGJsonNull();
        public static bool reuseSameInstance = true;
        public static SGJsonNull CreateOrGet()
        {
            if (reuseSameInstance)
                return m_StaticInstance;
            return new SGJsonNull();
        }
        private SGJsonNull() { }

        public override SGJsonNodeType Tag { get { return SGJsonNodeType.NullValue; } }
        public override bool IsNull { get { return true; } }
        public override Enumerator GetEnumerator() { return new Enumerator(); }

        public override string Value
        {
            get { return "null"; }
            set { }
        }
        public override bool AsBool
        {
            get { return false; }
            set { }
        }

        public override SGJsonNode Clone()
        {
            return CreateOrGet();
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;
            return (obj is SGJsonNull);
        }
        public override int GetHashCode()
        {
            return 0;
        }

        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, SGJsonTextMode aMode)
        {
            aSB.Append("null");
        }
    }
    // End of JSONNull

    internal partial class SGJsonLazyCreator : SGJsonNode
    {
        private SGJsonNode m_Node = null;
        private string m_Key = null;
        public override SGJsonNodeType Tag { get { return SGJsonNodeType.None; } }
        public override Enumerator GetEnumerator() { return new Enumerator(); }

        public SGJsonLazyCreator(SGJsonNode aNode)
        {
            m_Node = aNode;
            m_Key = null;
        }

        public SGJsonLazyCreator(SGJsonNode aNode, string aKey)
        {
            m_Node = aNode;
            m_Key = aKey;
        }

        private T Set<T>(T aVal) where T : SGJsonNode
        {
            if (m_Key == null)
                m_Node.Add(aVal);
            else
                m_Node.Add(m_Key, aVal);
            m_Node = null; // Be GC friendly.
            return aVal;
        }

        public override SGJsonNode this[int aIndex]
        {
            get { return new SGJsonLazyCreator(this); }
            set { Set(new SGJsonArray()).Add(value); }
        }

        public override SGJsonNode this[string aKey]
        {
            get { return new SGJsonLazyCreator(this, aKey); }
            set { Set(new SGJsonObject()).Add(aKey, value); }
        }

        public override void Add(SGJsonNode aItem)
        {
            Set(new SGJsonArray()).Add(aItem);
        }

        public override void Add(string aKey, SGJsonNode aItem)
        {
            Set(new SGJsonObject()).Add(aKey, aItem);
        }

        public static bool operator ==(SGJsonLazyCreator a, object b)
        {
            if (b == null)
                return true;
            return System.Object.ReferenceEquals(a, b);
        }

        public static bool operator !=(SGJsonLazyCreator a, object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return true;
            return System.Object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override int AsInt
        {
            get { Set(new SGJsonNumber(0)); return 0; }
            set { Set(new SGJsonNumber(value)); }
        }

        public override float AsFloat
        {
            get { Set(new SGJsonNumber(0.0f)); return 0.0f; }
            set { Set(new SGJsonNumber(value)); }
        }

        public override double AsDouble
        {
            get { Set(new SGJsonNumber(0.0)); return 0.0; }
            set { Set(new SGJsonNumber(value)); }
        }

        public override long AsLong
        {
            get
            {
                if (longAsString)
                    Set(new SGJsonString("0"));
                else
                    Set(new SGJsonNumber(0.0));
                return 0L;
            }
            set
            {
                if (longAsString)
                    Set(new SGJsonString(value.ToString()));
                else
                    Set(new SGJsonNumber(value));
            }
        }

        public override ulong AsULong
        {
            get
            {
                if (longAsString)
                    Set(new SGJsonString("0"));
                else
                    Set(new SGJsonNumber(0.0));
                return 0L;
            }
            set
            {
                if (longAsString)
                    Set(new SGJsonString(value.ToString()));
                else
                    Set(new SGJsonNumber(value));
            }
        }

        public override bool AsBool
        {
            get { Set(new SGJsonBool(false)); return false; }
            set { Set(new SGJsonBool(value)); }
        }

        public override SGJsonArray AsArray
        {
            get { return Set(new SGJsonArray()); }
        }

        public override SGJsonObject AsObject
        {
            get { return Set(new SGJsonObject()); }
        }
        internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, SGJsonTextMode aMode)
        {
            aSB.Append("null");
        }
    }
    // End of JSONLazyCreator

    public static class SGJson
    {
        public static SGJsonNode Parse(string aJSON)
        {
            return SGJsonNode.Parse(aJSON);
        }
    }
}
