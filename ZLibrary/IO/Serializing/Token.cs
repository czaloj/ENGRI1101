using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace System.IO
{
    public class Token : IEnumerable<string>
    {
        public const char CharBeginArgument = '[';
        public const char CharEndArgument = ']';
        public const char CharSimpleArgSplit = '|';
        public static readonly string StringSimpleArgSplit = new string(CharSimpleArgSplit, 1);
        public const char CharEscape = '%';
        public const char CharComment = '"';
        public const char CharCommentRest = '#';
        public const char CharVariable = '@';
        public const char CharFunction = '$';

        private static Dictionary<string, string> Variables;
        static Token()
        {
            Variables = new Dictionary<string, string>();
            setVariable("CWD", Directory.GetCurrentDirectory());
        }
        public static void setVariable(string name, string value)
        {
            if (Variables.ContainsKey(name))
            {
                Variables[name] = value;
            }
            else
            {
                Variables.Add(name, value);
            }
        }
        public static void setVariable(string name, object value)
        {
            setVariable(name, value.ToString());
        }
        public static bool deleteVariable(string name)
        {
            return Variables.Remove(name);
        }

        public delegate char CharProcessor(char c, string s, int i);
        public static readonly CharProcessor ReturnOriginal = new CharProcessor((c, s, i) => { return c; });
        public static readonly CharProcessor ReturnLower = new CharProcessor((c, s, i) => { return char.ToLower(c); });
        public static readonly CharProcessor ReturnUpper = new CharProcessor((c, s, i) => { return char.ToUpper(c); });

        protected string original;
        public string Original
        {
            get
            {
                return original;
            }
        }

        protected string header;
        public string Header
        {
            get
            {
                return header;
            }
        }
        protected string[] arguments;
        public string this[int arg]
        {
            get
            {
                return arguments[arg];
            }
        }
        public int ArgumentCount
        {
            get
            {
                return arguments != null ? arguments.Length : 0;
            }
        }
        public bool IsViable
        {
            get
            {
                return !string.IsNullOrWhiteSpace(header);
            }
        }

        #region Initialization
        private Token()
        {
            header = "";
            arguments = new string[0];
            original = "";
        }
        public Token(string line)
            : this(line, ReturnOriginal)
        {
        }
        public Token(string line, CharProcessor cpArg)
            : this(line, cpArg, ReturnLower)
        {
        }
        public Token(string line, CharProcessor cpArg, CharProcessor cpHeader)
        {
            original = line;
            parseOriginal(cpArg, cpHeader);
        }

        public static Token fromHeaderArgs(string header, params string[] args)
        {
            Token t = new Token();
            if (!string.IsNullOrWhiteSpace(header))
            {
                t.header = header;
            }
            if (args != null && args.Length > 0)
            {
                t.arguments = new string[args.Length];
                Array.Copy(args, t.arguments, t.arguments.Length);
            }
            t.original = t.ToString();
            return t;
        }
        public static Token fromHeaderArgs(string header, params object[] args)
        {
            Token t = new Token();
            if (!string.IsNullOrWhiteSpace(header))
            {
                t.header = header;
            }
            if (args != null && args.Length > 0)
            {
                t.arguments = new string[args.Length];
                for (int i = 0; i < t.arguments.Length; i++)
                { t.arguments[i] = args[i].ToString(); }
            }
            t.original = t.ToString();
            return t;
        }

        public void parseOriginal(CharProcessor cpArg, CharProcessor cpHeader)
        {
            parseString(original, cpArg, cpHeader);
        }
        public void parseString(string s, CharProcessor cpArg, CharProcessor cpHeader)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;

            //Read Until First Character In Header
            bool readHeader = false;
            while (!readHeader && i < s.Length)
            {
                switch (s[i])
                {
                    case CharComment:
                        if (!readComment(ref s, ref i)) { return; }
                        i++;
                        break;
                    case CharCommentRest:
                        return;
                    default:
                        if (char.IsWhiteSpace(s[i]))
                        {
                            i++;
                        }
                        else { readHeader = true; }
                        break;
                }
            }
            if (i >= s.Length) { header = null; return; }

            //Read Header Until First Whitespace Or Arg
            while (i < s.Length && !char.IsWhiteSpace(s[i]) && s[i] != CharBeginArgument)
            {
                sb.Append(cpHeader(s[i], s, i));
                i++;
            }
            header = sb.ToString();
            sb.Clear();


            //Read All Arguments
            LinkedList<string> args = new LinkedList<string>();
            while (readArg(ref sb, ref s, ref i, ref cpArg))
            {
                args.AddLast(sb.ToString());
                sb.Clear();
            }
            arguments = args.ToArray();

            //Make Sure Token Is Not A Special Token
            if (hasHeader("%set", ReturnLower) && ArgumentCount == 2)
            {
                setVariable(this[0], this[1]);
                header = null;

            }
            else if (hasHeader("%unset", ReturnLower) && ArgumentCount == 1)
            {
                deleteVariable(this[0]);
                header = null;
            }

        }

        private bool readArg(ref StringBuilder sb, ref string s, ref int i, ref CharProcessor cp)
        {
            //Read Until Beginning Of First Argument
            while (i < s.Length && (s[i] != CharBeginArgument && s[i] != CharSimpleArgSplit))
            {
                switch (s[i])
                {
                    case CharEscape:
                        i++;
                        if (i < s.Length)
                        {
                            switch (s[i])
                            {
                                case CharComment:
                                    if (!readComment(ref s, ref i)) { return false; }
                                    break;
                                case CharCommentRest:
                                    return false;
                            }
                        }
                        else { return false; }
                        break;
                    default: break;
                }
                i++;
            }
            if (i >= s.Length) { return false; }
            //Check If Bad
            if (s[i] == CharSimpleArgSplit && i > 0 && s[i - 1] == CharEscape) { return readArg(ref sb, ref s, ref i, ref cp); }


            //Move To First Character In Argument
            i++;
            while (i < s.Length)
            {
                switch (s[i])
                {
                    case CharEscape:
                        i++;
                        if (i < s.Length)
                        {
                            switch (s[i])
                            {
                                case CharEscape:
                                case CharBeginArgument:
                                case CharEndArgument:
                                case CharVariable:
                                case CharSimpleArgSplit:
                                    //Process As Part Of Argument
                                    sb.Append(cp(s[i], s, i));
                                    break;
                                case CharComment:
                                    //Read Until End Of Comment
                                    readComment(ref s, ref i);
                                    break;
                                case CharCommentRest:
                                    //Finished Arg
                                    return true;
                            }
                        }
                        break;
                    case CharVariable:
                        string vn, vv;
                        readVariable(ref s, ref i, out vn, out vv);
                        break;
                    case CharSimpleArgSplit:
                    case CharEndArgument:
                        //Finished Arg
                        return true;
                    case CharComment:
                        if (!readComment(ref s, ref i)) { return sb.Length > 0; }
                        break;
                    case CharCommentRest:
                        i = s.Length;
                        return sb.Length > 0;
                    default:
                        sb.Append(cp(s[i], s, i));
                        break;
                }
                i++;
            }
            return sb.Length > 0;
        }
        private bool readComment(ref string s, ref int i)
        {
            i++;
            while (i < s.Length && s[i] != CharComment)
            {
                if (s[i] == CharEscape) { i++; }
                i++;
            }
            return i < s.Length;
        }
        private bool readVariable(ref string s, ref int i, out string variable, out string value)
        {
            StringBuilder sb = new StringBuilder();
            int startIndex = i;
            i++;
            bool read = false;
            while (!read && i < s.Length && s[i] != CharVariable)
            {
                switch (s[i])
                {
                    case CharEscape:
                        i++;
                        if (i < s.Length)
                        {
                            switch (s[i])
                            {
                                case CharVariable:
                                    string ivn, ivv;
                                    read = !readVariable(ref s, ref i, out ivn, out ivv);
                                    break;
                                case CharEscape:
                                    sb.Append(s[i]);
                                    break;
                                default:
                                    i--;
                                    read = true;
                                    break;
                            }
                        }
                        break;
                    case CharVariable:
                        read = true;
                        break;
                    default:
                        if (char.IsWhiteSpace(s[i])) { i--; read = true; }
                        else { sb.Append(s[i]); }
                        break;
                }
                i++;
            }
            variable = sb.ToString();
            sb.Clear();
            if (!string.IsNullOrEmpty(variable) && Variables.ContainsKey(variable))
            {
                value = Variables[variable];
                sb.Append(s, 0, startIndex);
                sb.Append(value);
                sb.Append(s, i + 1, s.Length - i - 1);
                s = sb.ToString();
                i = startIndex - 1;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
        //private bool readFunction(ref string s, ref int i, out string variable, out string value)
        //{
        //    return false;
        //}
        #endregion

        public void makeHeaderLowerCase()
        {
            if (header != null)
            {
                header = header.ToLower();
            }
        }
        public void makeArgsLowerCase()
        {
            for (int i = 0; i < ArgumentCount; i++)
            {
                if (arguments[i] != null)
                {
                    arguments[i] = arguments[i].ToLower();
                }
            }
        }
        public void makeAllLowerCase()
        {
            makeHeaderLowerCase();
            makeArgsLowerCase();
        }

        public bool hasHeader(string h)
        {
            return header.Equals(h);
        }
        public bool hasHeader(string h, CharProcessor cpHeader)
        {
            if (string.IsNullOrEmpty(h) || string.IsNullOrEmpty(header) || h.Length != header.Length) { return false; }
            for (int i = 0; i < header.Length; i++)
            {
                if (cpHeader(header[i], header, i) != h[i])
                {
                    return false;
                }
            }
            return true;
        }

        public bool getArg<T>(int arg, ref T value)
        {
            string s = this[arg];
            if (s == null) { return false; }
            else
            {
                try
                {
                    value = (T)Convert.ChangeType(this[arg], typeof(T));
                    return value != null;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        public bool getArgVector2(int arg, ref Vector2 v)
        {
            float vX = default(float), vY = default(float);
            if (getArg<float>(arg, ref vX) &&
                getArg<float>(arg + 1, ref vY)
                )
            {
                v = new Vector2(vX, vY);
                return true;
            }
            return false;
        }
        public bool getArgVector3(int arg, ref Vector3 v)
        {
            float vX = default(float), vY = default(float), vZ = default(float);
            if (getArg<float>(arg, ref vX) &&
                getArg<float>(arg + 1, ref vY) &&
                getArg<float>(arg + 2, ref vZ)
                )
            {
                v = new Vector3(vX, vY, vZ);
                return true;
            }
            return false;
        }
        public bool getArgVector4(int arg, ref Vector4 v)
        {
            float vX = default(float), vY = default(float), vZ = default(float), vW = default(float);
            if (getArg<float>(arg, ref vX) &&
                getArg<float>(arg + 1, ref vY) &&
                getArg<float>(arg + 2, ref vZ) &&
                getArg<float>(arg + 3, ref vW)
                )
            {
                v = new Vector4(vX, vY, vZ, vW);
                return true;
            }
            return false;
        }
        public bool getArgColor(int arg, ref Color c)
        {
            int r = default(int), g = default(int), b = default(int), a = default(int);
            if (getArg<int>(arg, ref r) &&
                getArg<int>(arg + 1, ref g) &&
                getArg<int>(arg + 2, ref b) &&
                getArg<int>(arg + 3, ref a)
                )
            {
                c = new Color(r, g, b, a);
                return true;
            }
            return false;
        }
        public bool getArgs(out object[] o, params ConvertedArg[] args)
        {
            o = new object[args.Length];
            for (int i = 0; i < o.Length; i++)
            {
                args[i].getValue(this);
                if (args[i].Successful)
                { o[i] = args[i].Value; }
                else { return false; }
            }
            return true;
        }

        //public void write(StreamWriter s)
        //{
        //    s.WriteLine(ToString());
        //}

        //public static bool read(StreamReader s, out Token t)
        //{
        //    if (s.EndOfStream) { t = null; return false; }
        //    t = new Token(s.ReadLine());
        //    if (t.IsViable)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        //public static bool readUntilNext(StreamReader s, out Token t)
        //{
        //    while (!s.EndOfStream)
        //    {
        //        t = new Token(s.ReadLine());
        //        if (t.IsViable)
        //        {
        //            return true;
        //        }
        //    }
        //    t = null;
        //    return false;
        //}

        //public static bool readUntilHeader(StreamReader s, string header, out Token token)
        //{
        //    while (readUntilNext(s, out token))
        //    {
        //        if (token.Header.Equals(header))
        //        {
        //            return true;
        //        }
        //    }
        //    token = null;
        //    return false;
        //}
        //public static bool readUntilSignature(StreamReader s, out Token t, Signature sig)
        //{
        //    while (readUntilNext(s, out t))
        //    {
        //        if (sig.holdsWith(t))
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        //public static bool readUntilAllSignatures(StreamReader s, out Token[] tokens, params ISignature[] signatures)
        //{
        //    tokens = new Token[signatures.Length];
        //    int count = tokens.Length;
        //    Token t;
        //    while (count > 0 && Token.readUntilNext(s, out t))
        //    {
        //        for (int i = 0; i < signatures.Length; i++)
        //        {
        //            if (tokens[i] == null && signatures[i].holdsWith(t))
        //            {
        //                tokens[i] = t;
        //                count--;
        //                break;
        //            }
        //        }
        //    }
        //    return count == 0;
        //}
        //public static bool readUntilAllSignaturePiles(StreamReader s, out Token[][] tokens, params ISignaturePile[] signatures)
        //{
        //    tokens = new Token[signatures.Length][];
        //    int[] counts = new int[signatures.Length];
        //    int count = 0;
        //    for (int i = 0; i < signatures.Length; i++)
        //    {
        //        tokens[i] = new Token[signatures[i].PileSize];
        //        count += signatures[i].PileSize;
        //    }
        //    Token t;
        //    while (count > 0 && Token.readUntilNext(s, out t))
        //    {
        //        for (int i = 0; i < signatures.Length; i++)
        //        {
        //            if (tokens[i] == null && signatures[i].holdsWith(t) && counts[i] < tokens[i].Length)
        //            {
        //                tokens[i][counts[i]] = t;
        //                counts[i]++;
        //                count--;
        //                break;
        //            }
        //        }
        //    }
        //    return count == 0;
        //}
        //public static bool readUntilAllHeaders(StreamReader s, out Token[] tokens, params string[] headers)
        //{
        //    tokens = new Token[headers.Length];
        //    int count = tokens.Length;
        //    Token t;
        //    while (count > 0 && Token.readUntilNext(s, out t))
        //    {
        //        for (int i = 0; i < headers.Length; i++)
        //        {
        //            if (tokens[i] == null && headers[i].Equals(t.Header))
        //            {
        //                tokens[i] = t;
        //                count--;
        //                break;
        //            }
        //        }
        //    }
        //    return count == 0;
        //}
        //public static bool readUntilAllHeaders(StreamReader s, out Token[] tokens, params HeaderValueBinding[] headers)
        //{
        //    tokens = new Token[headers.Length];
        //    int count = tokens.Length;
        //    Token t;
        //    while (count > 0 && Token.readUntilNext(s, out t))
        //    {
        //        for (int i = 0; i < headers.Length; i++)
        //        {
        //            if (tokens[i] == null && headers[i].Lower.Equals(t.Header))
        //            {
        //                tokens[i] = t;
        //                count--;
        //                break;
        //            }
        //        }
        //    }
        //    return count == 0;
        //}

        //public static bool readUntilAllHeaders(StreamReader s, out Token[] tokens, out object[][] values, params HeaderValueBinding[] headers)
        //{
        //    if (readUntilAllHeaders(s, out tokens, headers))
        //    {
        //        values = new object[headers.Length][];
        //        for (int i = 0; i < headers.Length; i++)
        //        {
        //            if (headers[i].ArgCount > 0)
        //            {
        //                values[i] = new object[headers[i].ArgCount];
        //                for (int ai = 0; ai < values[i].Length; ai++)
        //                {
        //                    headers[i].Args[ai].getValue(tokens[i]);
        //                    if (headers[i].Args[ai].Successful)
        //                    {
        //                        values[i][ai] = headers[i][ai];
        //                    }
        //                    else
        //                    {
        //                        return false;
        //                    }
        //                }
        //            }
        //        }
        //        return true;
        //    }
        //    values = null;
        //    return false;
        //}

        public override string ToString()
        {
            if (ArgumentCount == 0)
            {
                return header;
            }
            else
            {
                return string.Format("{0} [{1}]", header, string.Join(StringSimpleArgSplit, arguments));
            }
        }

        #region Bindings

        public struct HeaderValueBinding
        {
            public string Lower;
            public string Upper;

            public ConvertedArg[] Args;
            public int ArgCount;
            public object this[int argIndex]
            {
                get { return Args[argIndex].Value; }
            }

            public bool ValueConversionSuccessful
            {
                get { foreach (ConvertedArg a in Args) { if (!a.Successful) { return false; } } return true; }
            }

            public HeaderValueBinding(string header, params ConvertedArg[] args)
            {
                Upper = header;
                Lower = header.ToLower();
                Args = args;
                ArgCount = args != null ? args.Length : 0;
            }

            public void getArgs(Token t)
            {
                for (int i = 0; i < ArgCount; i++)
                {
                    Args[i].getValue(t);
                }
            }
        }
        public struct ConvertedArg
        {
            public int ArgIndex;
            public Type VType;
            public object Value;
            public bool Successful { get { return Value != null && Value.GetType() == VType; } }

            public ConvertedArg(int arg, Type argType)
            {
                ArgIndex = arg;
                VType = argType;
                Value = null;
            }

            public void getValue(Token t)
            {
                Value = Convert.ChangeType(t[ArgIndex], VType);
            }
        }
        #endregion

        #region Exceptions
        public void throwBadArg<T>(int position)
        {
            throw new BadArgumentException<T>(this, position);
        }
        public void throwBadArgCount(int desiredCount)
        {
            throw new BadArgumentCountException(this, desiredCount);
        }
        public void throwHeaderMissing(string header)
        {
            throw new HeaderMissingException(this, header);
        }
        public static void throwMissingSignature(Signature sig)
        {
            throw new ArgumentException(string.Format("Missing Token With Signature:\r\n{0}", sig));
        }


        [Serializable]
        public class TokenException : Exception
        {
            private Token token;
            public Token Token { get { return token; } }

            public TokenException() { }
            public TokenException(Token t, string message) : base(t.ToString() + "\r\n" + message) { token = t; }
            public TokenException(Token t, string message, Exception inner) : base(t.ToString() + "\r\n" + message, inner) { token = t; }
            protected TokenException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
        }
        [Serializable]
        public class HeaderMissingException : TokenException
        {
            public HeaderMissingException() { }
            public HeaderMissingException(Token t, string message) : base(t, formatMessage(message)) { }
            public HeaderMissingException(Token t, string message, Exception inner) : base(t, formatMessage(message), inner) { }
            protected HeaderMissingException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }

            private static string formatMessage(string m)
            {
                return string.Format("Expecting Header [{0}]", m);
            }
        }
        [Serializable]
        public class BadArgumentException<T> : TokenException
        {
            public BadArgumentException() { }
            public BadArgumentException(Token t, int message) : base(t, formatMessage(message)) { }
            public BadArgumentException(Token t, int message, Exception inner) : base(t, formatMessage(message), inner) { }
            protected BadArgumentException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }

            private static string formatMessage(int m)
            {
                return string.Format("Expecting Argument Of Type [{0}] At Position [{1}]", typeof(T).Name, m);
            }
        }
        [Serializable]
        public class BadArgumentCountException : TokenException
        {
            public BadArgumentCountException() { }
            public BadArgumentCountException(Token t, int message) : base(t, formatMessage(message)) { }
            public BadArgumentCountException(Token t, int message, Exception inner) : base(t, formatMessage(message), inner) { }
            protected BadArgumentCountException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }

            private static string formatMessage(int m)
            {
                return string.Format("Expecting At Least {0} Arguments", m);
            }
        }
        #endregion

        #region Enumeration
        public IEnumerator<string> GetEnumerator()
        {
            foreach (string s in arguments) { yield return s; }
        }
        Collections.IEnumerator Collections.IEnumerable.GetEnumerator()
        {
            foreach (string s in arguments) { yield return s; }
        }
        #endregion
    }

    public class TokenStream : IDisposable
    {

        private LinkedList<Node> tokenQueue;
        public int TokensRemaining
        {
            get { return tokenQueue.Count; }
        }
        public Token First
        {
            get { return TokensRemaining > 0 ? tokenQueue.First.Value.Token : null; }
        }
        public Token Last
        {
            get { return TokensRemaining > 0 ? tokenQueue.Last.Value.Token : null; }
        }

        private Token last;
        public Token LastAdded
        {
            get { return last; }
        }

        public event Action<Token> OnTokenRead;
        public event Action<Token> OnDesiredTokenRead;
        private void fireRead(Token t) { if (OnTokenRead != null) { OnTokenRead(t); } }

        private StreamReader reader;
        public bool CanRead
        {
            get { return reader != null && reader.BaseStream.CanRead && !reader.EndOfStream; }
        }
        private StreamWriter writer;
        public bool CanWrite
        {
            get { return writer != null && writer.BaseStream.CanWrite; }
        }

        private Token.CharProcessor cpArg, cpHeader;
        public Token.CharProcessor ArgProcessor
        {
            get { return cpArg; }
            set { cpArg = value; }
        }
        public Token.CharProcessor HeaderProcessor
        {
            get { return cpHeader; }
            set { cpHeader = value; }
        }

        private TokenStream()
        {
            cpArg = Token.ReturnOriginal;
            cpHeader = Token.ReturnLower;

            tokenQueue = new LinkedList<Node>();
            OnDesiredTokenRead += fireRead;
        }
        public TokenStream(StreamReader s)
            : this()
        {
            setReader(s);
        }
        public TokenStream(StreamWriter s)
            : this()
        {
            setWriter(s);
        }
        public TokenStream(StreamReader sr, StreamWriter sw)
            : this()
        {
            setReader(sr);
            setWriter(sw);
        }
        public TokenStream(string file, FileMode mode)
            : this()
        {
            switch (mode)
            {
                case FileMode.Open:
                case FileMode.OpenOrCreate:
                    reader = new StreamReader(File.Open(file, mode));
                    break;
                case FileMode.Append:
                case FileMode.Truncate:
                case FileMode.Create:
                case FileMode.CreateNew:
                    writer = new StreamWriter(File.Open(file, mode));
                    break;
            }
        }
        public TokenStream(string file, FileMode mode, Encoding encoding)
            : this()
        {
            switch (mode)
            {
                case FileMode.Open:
                case FileMode.OpenOrCreate:
                    reader = new StreamReader(File.Open(file, mode), encoding);
                    break;
                case FileMode.Append:
                case FileMode.Truncate:
                case FileMode.Create:
                case FileMode.CreateNew:
                    writer = new StreamWriter(File.Open(file, mode), encoding);
                    break;
            }
        }

        public void setReader(StreamReader s)
        {
            reader = s;
        }
        public void closeRead()
        {
            if (reader != null) { reader.Dispose(); }
        }
        public void setWriter(StreamWriter s)
        {
            writer = s;
        }
        public void closeWrite()
        {
            if (writer != null) { writer.Dispose(); }
        }
        ~TokenStream() { dispose(); }
        public void Dispose() { GC.SuppressFinalize(this); dispose(); }
        private void dispose()
        {
            closeRead();
            closeWrite();
        }

        private void addNewNode(Token t)
        {
            tokenQueue.AddLast(new Node(t));
            last = t;
        }

        public bool readNext()
        {
            if (reader.EndOfStream) { return false; }
            Token t = new Token(reader.ReadLine(), cpArg, cpHeader);
            if (t.IsViable)
            {
                addNewNode(t);
                OnDesiredTokenRead(t);
                return true;
            }
            return false;
        }
        public bool readUntilNext()
        {
            Token t;
            while (!reader.EndOfStream)
            {
                t = new Token(reader.ReadLine(), cpArg, cpHeader);
                if (t.IsViable)
                {
                    addNewNode(t);
                    OnDesiredTokenRead(t);
                    return true;
                }
            }
            return false;
        }

        public bool readUntilHeader(string header)
        {
            Token t;
            while (!reader.EndOfStream)
            {
                t = new Token(reader.ReadLine(), cpArg, cpHeader);
                if (t.IsViable)
                {
                    addNewNode(t);
                    if (t.hasHeader(header))
                    {
                        OnDesiredTokenRead(t);
                        return true;
                    }
                    else
                    {
                        fireRead(t);
                    }
                }
            }
            return false;
        }
        public bool readUntilHeader(string header, out Token t)
        {
            if (readUntilHeader(header))
            {
                t = LastAdded;
                return true;
            }
            else
            {
                t = null;
                return false;
            }
        }
        public bool readUntilHeader(string header, Token.CharProcessor cpHeaderTest)
        {
            Token t;
            while (!reader.EndOfStream)
            {
                t = new Token(reader.ReadLine(), cpArg, cpHeader);
                if (t.IsViable)
                {
                    addNewNode(t);
                    if (t.hasHeader(header, cpHeaderTest))
                    {
                        OnDesiredTokenRead(t);
                        return true;
                    }
                    else
                    {
                        fireRead(t);
                    }
                }
            }
            return false;
        }
        public bool readUntilHeader(string header, Token.CharProcessor cpHeaderTest, out Token t)
        {
            if (readUntilHeader(header, cpHeaderTest))
            {
                t = LastAdded;
                return true;
            }
            else
            {
                t = null;
                return false;
            }
        }
        public bool readUntilAllHeaders(LinkedQueue<string> q)
        {
            string test;
            int count; Token t;
            while (q.Count > 0 && !reader.EndOfStream)
            {
                t = new Token(reader.ReadLine(), cpArg, cpHeader);
                if (t.IsViable)
                {
                    addNewNode(t);
                    count = q.Count;
                    while (count > 0)
                    {
                        test = q.dequeue();
                        if (t.hasHeader(test))
                        {
                            OnDesiredTokenRead(t);
                            break;
                        }
                        else
                        {
                            q.enqueue(test);
                            count--;
                        }
                    }
                    if (count == 0) { fireRead(t); }
                }
            }
            return q.Count == 0;
        }
        public bool readUntilAllHeaders(LinkedQueue<string> q, Token.CharProcessor cpHeaderTest)
        {
            string test;
            int count; Token t;
            while (q.Count > 0 && !reader.EndOfStream)
            {
                t = new Token(reader.ReadLine(), cpArg, cpHeader);
                if (t.IsViable)
                {
                    addNewNode(t);
                    count = q.Count;
                    while (count > 0)
                    {
                        test = q.dequeue();
                        if (t.hasHeader(test, cpHeaderTest))
                        {
                            OnDesiredTokenRead(t);
                            break;
                        }
                        else
                        {
                            q.enqueue(test);
                            count--;
                        }
                    }
                    if (count == 0) { fireRead(t); }
                }
            }
            return q.Count == 0;
        }

        public bool readUntilSignature(ISignature s)
        {
            Token t;
            while (!reader.EndOfStream)
            {
                t = new Token(reader.ReadLine(), cpArg, cpHeader);
                if (t.IsViable)
                {
                    addNewNode(t);
                    if (s.holdsWith(t))
                    {
                        OnDesiredTokenRead(t);
                        return true;
                    }
                    else
                    {
                        fireRead(t);
                    }
                }
            }
            return false;
        }
        public bool readUntilSignature(ISignature s, out Token t)
        {
            if (readUntilSignature(s))
            {
                t = LastAdded;
                return true;
            }
            else
            {
                t = null;
                return false;
            }
        }
        public bool readUntilAllSignatures(LinkedQueue<ISignature> q)
        {
            ISignature test;
            int count; Token t;
            while (q.Count > 0 && !reader.EndOfStream)
            {
                t = new Token(reader.ReadLine(), cpArg, cpHeader);
                if (t.IsViable)
                {
                    addNewNode(t);
                    count = q.Count;
                    while (count > 0)
                    {
                        test = q.dequeue();
                        if (test.holdsWith(t))
                        {
                            OnDesiredTokenRead(t);
                            break;
                        }
                        else
                        {
                            q.enqueue(test);
                            count--;
                        }
                    }
                    if (count == 0) { fireRead(t); }
                }
            }
            return q.Count == 0;
        }
        Func<ISignaturePile, int> pileSummer = (p) => { return p.PileSize; };
        public bool readUntilAllSignaturePiles(params ISignaturePile[] p)
        {
            ISignaturePile test;
            if (p == null || p.Length == 0) { return false; }
            int qcount = p.Sum(pileSummer), count; Token t;
            while (qcount > 0)
            {
                t = new Token(reader.ReadLine(), cpArg, cpHeader);
                if (t.IsViable)
                {
                    addNewNode(t);
                    count = 0;
                    while (count < p.Length)
                    {
                        test = p[count];
                        if (test.holdsWith(t))
                        {
                            if (test.PileSize > 0)
                            {
                                OnDesiredTokenRead(t);
                                test.decrement();
                                qcount--;
                            }
                            else
                            {
                                fireRead(t);
                            }
                            break;
                        }
                        else
                        {
                            count++;
                        }
                    }
                    if (count == p.Length) { fireRead(t); }
                }
            }
            return qcount == 0;
        }

        public int readAll()
        {
            Token t;
            int added = 0;
            while (!reader.EndOfStream)
            {
                t = new Token(reader.ReadLine(), cpArg, cpHeader);
                if (t.IsViable)
                {
                    addNewNode(t);
                    OnDesiredTokenRead(t);
                    added++;
                }
            }
            return added;
        }

        public void recycle()
        {
            if (TokensRemaining <= 0) { return; }
            Node n = Node.recycle(tokenQueue.First.Value);
            tokenQueue.RemoveFirst();
            tokenQueue.AddLast(n);
        }
        public void recycle(int count)
        {
            if (TokensRemaining <= 0) { return; }
            Node n;
            while (count > 0)
            {
                n = Node.recycle(tokenQueue.First.Value);
                tokenQueue.RemoveFirst();
                tokenQueue.AddLast(n);
                count--;
            }
        }

        public bool next(out Token t)
        {
            if (TokensRemaining > 0)
            {
                t = tokenQueue.First.Value.Token;
                tokenQueue.RemoveFirst();
                return true;
            }
            else
            {
                t = null;
                return false;
            }
        }
        public bool removeLastAdded(out Token t)
        {
            LinkedListNode<Node> ln = tokenQueue.Last;
            if (ln != null && ln.Value.Token != last)
            { ln = ln.Previous; }
            if (ln != null)
            {
                tokenQueue.Remove(ln);
                t = ln.Value.Token;
                return true;
            }
            else
            {
                t = null;
                return false;
            }
        }

        public bool search(string header, out Token t)
        {
            int count = TokensRemaining;
            while (count > 0)
            {
                t = First;
                if (t.hasHeader(header))
                {
                    return next(out t);
                }
                recycle();
                count--;
            }
            t = null;
            return false;
        }
        public bool search(string header, Token.CharProcessor cpHeader, out Token t)
        {
            int count = TokensRemaining;
            while (count > 0)
            {
                t = First;
                if (t.hasHeader(header, cpHeader))
                {
                    return next(out t);
                }
                recycle();
                count--;
            }
            t = null;
            return false;
        }
        public bool search(Signature s, out Token t)
        {
            int count = TokensRemaining;
            while (count > 0)
            {
                t = First;
                if (s.holdsWith(t))
                {
                    return next(out t);
                }
                recycle();
                count--;
            }
            t = null;
            return false;
        }

        public void clear()
        {
            tokenQueue.Clear();
        }

        public bool AutoFlush { get; set; }
        public void write(Token t)
        {
            last = t;
            tokenQueue.AddLast(new Node(t));
            if (AutoFlush) { flushTokens(); }
        }
        public void write(IEnumerable<Token> ta)
        {
            if (ta == null) { return; }
            foreach (Token t in ta) { write(t); }
        }
        public void write(string header, params object[] args)
        {
            write(Token.fromHeaderArgs(header, args));
        }
        public void write(string header, params string[] args)
        {
            write(Token.fromHeaderArgs(header, args));
        }

        public void flushTokens()
        {
            Node n;
            while (TokensRemaining > 0)
            {
                n = tokenQueue.First.Value;
                writer.WriteLine(n.Token);
                tokenQueue.RemoveFirst();
            }
            writer.Flush();
        }

        public struct Node
        {
            public Token Token;

            private int rCount;
            public int RecycleCount
            {
                get { return rCount; }
            }

            private Node(Token t, int rc)
            {
                Token = t;
                rCount = rc;
            }
            public Node(Token t)
                : this(t, 0)
            {
            }

            public static Node recycle(Node n)
            {
                return new Node(n.Token, n.RecycleCount + 1);
            }

            public override string ToString()
            {
                return string.Format("R[{0}]-:-[{1}]", rCount, Token);
            }
        }
    }

    #region Bindings

    #region Signatures
    public interface ISignature
    {
        string Header { get; }
        int ArgCount { get; }

        bool holdsWith(Token t);
    }

    public class Signature : ISignature
    {
        private string header;
        public string Header
        {
            get { return header; }
            set { header = value; }
        }

        private int argCount;
        public int ArgCount
        {
            get { return argCount; }
            set { argCount = value; }
        }

        public Signature(string h, int args)
        {
            header = h;
            argCount = args;
        }
        public Signature(Signature s)
            : this(s.header, s.ArgCount)
        {
        }

        public bool holdsWith(Token t)
        {
            return t.Header.Equals(header) && t.ArgumentCount >= ArgCount;
        }

        public override string ToString()
        {
            if (ArgCount == 0)
            {
                return header;
            }
            else
            {
                return string.Format("{0} [{1}]", header, string.Join(Token.StringSimpleArgSplit, Enumerable.Repeat<string>("_", ArgCount)));
            }
        }
    }
    public class ArgBinds
    {
        public IArgBind[] Binds;

        public ArgBinds(params IArgBind[] b)
        {
            Binds = b ?? new IArgBind[0];
        }
        public ArgBinds(ArgBinds b)
        {
            if (b.Binds != null && b.Binds.Length > 0)
            {
                Binds = new IArgBind[b.Binds.Length];
                Array.Copy(b.Binds, Binds, Binds.Length);
            }
            else
            {
                Binds = new IArgBind[0];
            }
        }

        public bool convert(Token t)
        {
            for (int i = 0; i < Binds.Length; i++)
            {
                if (!Binds[i].convert(t[i])) { return false; }
            }
            return true;
        }
    }
    public class SignatureArgBinds : Signature
    {
        public ArgBinds Args;

        public SignatureArgBinds(string h, int args, params IArgBind[] b)
            : base(h, args)
        {
            Args = new ArgBinds(b);
        }
        public SignatureArgBinds(string h, int args, ArgBinds b)
            : base(h, args)
        {
            Args = b ?? new ArgBinds();
        }
        public SignatureArgBinds(Signature s, params IArgBind[] b)
            : base(s)
        {
            Args = new ArgBinds(b);
        }
        public SignatureArgBinds(Signature s, ArgBinds b)
            : base(s)
        {
            Args = b ?? new ArgBinds();
        }
        public SignatureArgBinds(SignatureArgBinds sab)
            : base(sab)
        {
            Args = new ArgBinds(sab.Args);
        }

        public bool convert(Token t)
        {
            return Args.convert(t);
        }
        public bool read(Token t)
        {
            if (holdsWith(t)) { return convert(t); }
            return false;
        }
    }
    public class SignaturePile : Signature, ISignaturePile
    {
        public int PileSize { get; private set; }

        public SignaturePile(string h, int args, int count)
            : base(h, args)
        {
            PileSize = count;
        }
        public SignaturePile(Signature s, int count)
            : base(s)
        {
            PileSize = count;
        }

        public void decrement() { PileSize--; }
    }
    public class SignatureArgBindsPile : SignatureArgBinds, ISignaturePile
    {
        public int PileSize { get; private set; }

        public SignatureArgBindsPile(string h, int args, ArgBinds b, int count)
            : base(h, args, b)
        {
            PileSize = count;
        }
        public SignatureArgBindsPile(Signature s, ArgBinds b, int count)
            : base(s, b)
        {
            PileSize = count;
        }
        public SignatureArgBindsPile(SignatureArgBinds sab, int count)
            : base(sab)
        {
            PileSize = count;
        }

        public void decrement() { PileSize--; }
    }
    #endregion

    #region Piles
    public interface IPile
    {
        int PileSize { get; }

        void decrement();
    }
    public interface ISignaturePile : ISignature, IPile
    {
    }
    public struct HeaderPile : ISignaturePile
    {
        private string header;
        public string Header
        {
            get { return header; }
            private set { header = value; }
        }
        public int ArgCount
        {
            get { throw new NotImplementedException(); }
        }
        private int pc;
        public int PileSize
        {
            get { return pc; }
            private set { pc = value; }
        }

        public HeaderPile(string h, int c)
        {
            header = h;
            pc = c;
        }

        public bool holdsWith(Token t)
        {
            return t.Header.Equals(Header);
        }
        public void decrement() { PileSize--; }
    }
    #endregion


    #region Converters

    public delegate bool ArgConverter<T>(string s);
    public delegate void ArgReverter<T>(T o, out string s);

    public interface IArgBind
    {
        bool convert(string s);
    }
    public class ArgBind<T> : IArgBind
    {
        private ArgConverter<T> conv;

        public ArgBind(ArgConverter<T> ac)
        {
            conv = ac;
        }

        public bool convert(string s)
        {
            return conv(s);
        }
    }

    #region Types
    public static class TokenGenericConverters
    {
        public unsafe class Bool : IArgBind
        {
            bool* obj;

            public Bool(ref bool o)
            {
                fixed (bool* _obj = &o)
                {
                    obj = _obj;
                }
            }

            public bool convert(string s)
            {
                bool i;
                if (bool.TryParse(s, out i))
                {
                    *obj = i;
                    return true;
                }
                return true;
            }
        }

        public unsafe class Byte : IArgBind
        {
            byte* obj;

            public Byte(ref byte o)
            {
                fixed (byte* _obj = &o)
                {
                    obj = _obj;
                }
            }

            public bool convert(string s)
            {
                byte i;
                if (byte.TryParse(s, out i))
                {
                    *obj = i;
                    return true;
                }
                return true;
            }
        }

        public unsafe class UShort : IArgBind
        {
            ushort* obj;

            public UShort(ref ushort o)
            {
                fixed (ushort* _obj = &o)
                {
                    obj = _obj;
                }
            }

            public bool convert(string s)
            {
                ushort i;
                if (ushort.TryParse(s, out i))
                {
                    *obj = i;
                    return true;
                }
                return true;
            }
        }
        public unsafe class UInt : IArgBind
        {
            uint* obj;

            public UInt(ref uint o)
            {
                fixed (uint* _obj = &o)
                {
                    obj = _obj;
                }
            }

            public bool convert(string s)
            {
                uint i;
                if (uint.TryParse(s, out i))
                {
                    *obj = i;
                    return true;
                }
                return true;
            }
        }
        public unsafe class ULong : IArgBind
        {
            ulong* obj;

            public ULong(ref ulong o)
            {
                fixed (ulong* _obj = &o)
                {
                    obj = _obj;
                }
            }

            public bool convert(string s)
            {
                ulong i;

                if (ulong.TryParse(s, out i))
                {
                    *obj = i;
                    return true;
                }
                return true;
            }
        }

        public unsafe class Short : IArgBind
        {
            short* obj;

            public Short(ref short o)
            {
                fixed (short* _obj = &o)
                {
                    obj = _obj;
                }
            }

            public bool convert(string s)
            {
                short i;
                if (short.TryParse(s, out i))
                {
                    *obj = i;
                    return true;
                }
                return true;
            }
        }
        public unsafe class Int : IArgBind
        {
            int* obj;

            public Int(ref int o)
            {
                fixed (int* _obj = &o)
                {
                    obj = _obj;
                }
            }

            public bool convert(string s)
            {
                int i;
                if (int.TryParse(s, out i))
                {
                    *obj = i;
                    return true;
                }
                return true;
            }
        }
        public unsafe class Long : IArgBind
        {
            long* obj;

            public Long(ref long o)
            {
                fixed (long* _obj = &o)
                {
                    obj = _obj;
                }
            }

            public bool convert(string s)
            {
                long i;

                if (long.TryParse(s, out i))
                {
                    *obj = i;
                    return true;
                }
                return true;
            }
        }
        public unsafe class Float : IArgBind
        {
            float* obj;

            public Float(ref float o)
            {
                fixed (float* _obj = &o)
                {
                    obj = _obj;
                }
            }

            public bool convert(string s)
            {
                float i;
                if (float.TryParse(s, out i))
                {
                    *obj = i;
                    return true;
                }
                return true;
            }
        }
        public unsafe class Double : IArgBind
        {
            double* obj;

            public Double(ref double o)
            {
                fixed (double* _obj = &o)
                {
                    obj = _obj;
                }
            }

            public bool convert(string s)
            {
                double i;
                if (double.TryParse(s, out i))
                {
                    *obj = i;
                    return true;
                }
                return true;
            }
        }
    }
    #endregion

    #endregion

    #endregion
}
