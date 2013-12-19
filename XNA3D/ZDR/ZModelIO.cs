using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA3D.ZDR
{
    public interface IVertexConverter<T> where T : IVertexType
    {
        void read(string[] vArgs, out T v);
        void convert(T v, out Token t);
    }
    public interface IVertexCalculator<T> : IVertexConverter<T> where T : IVertexType
    {
        void calculateTriangle(ref T v1, ref T v2, ref  T v3);
    }

    /* File Setup:
     * // Give The Definition Of How Args Are Concatenated
     * ArgDef [v:ec#|vt:ec#|vn:ec#]
     * 
     * // Give All Data
     * v [_|_|_]
     * ...
     * vt[_|_]
     * ...
     * vn[_|_|_]
     * ...
     * 
     * // Begin All The Vertex Creation Here
     * TriCount [#]
     * Tri [v#,vt#,vn#|v#,vt#,vn#|v#,vt#,vn#]
     * ...
     */

    public class ZModelReader<T> : IDisposable where T : IVertexType
    {
        public const string hADDefine = "argdef";
        public const string hTriCount = "tricount";
        public const string hTri = "tri";

        TokenStream tstream;
        IVertexConverter<T> converter;

        public ZModelReader(IVertexConverter<T> c, string file)
        {
            tstream = new TokenStream(file, FileMode.Open);
            tstream.ArgProcessor = Token.ReturnOriginal;
            tstream.HeaderProcessor = Token.ReturnLower;
            converter = c;
        }
        ~ZModelReader() { dispose(); }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            dispose();
        }
        protected virtual void dispose()
        {
            tstream.Dispose();
        }

        public virtual void readAll(out T[] vertices, out int[] indices)
        {
            Token t;
            StringBuilder sb = new StringBuilder();
            LinkedList<ModelTriangle> tris = new LinkedList<ModelTriangle>();
            int tc = 2048000;
            if (tstream.readUntilHeader(hADDefine, out t))
            {
                ZMVArgData data = new ZMVArgData(new Token(t.Original, Token.ReturnLower, Token.ReturnLower));
                tstream.removeLastAdded(out t);
                while (tstream.next(out t))
                {
                    if (data.hasArgType(t.Header))
                    {
                        foreach (string arg in t)
                        {
                            sb.Append(Token.CharSimpleArgSplit);
                            sb.Append(arg);
                        }
                        data.addArg(t.Header, sb.ToString(1, sb.Length - 1));
                        sb.Clear();
                    }
                    else if (t.hasHeader(hTri))
                    {
                        if (tris.Count < tc)
                        {
                            tris.AddLast(new ModelTriangle(t[0], t[1], t[2]));
                        }
                    }
                    else if (t.hasHeader(hTriCount))
                    {
                        if (!int.TryParse(t[0], out tc))
                        { throw new ArgumentException("Triangle Count Expected To Be Integer"); }
                    }
                }

                #region Read The Rest Of The Information
                tstream.readAll();
                while (tstream.next(out t))
                {
                    if (data.hasArgType(t.Header))
                    {
                        foreach (string arg in t)
                        {
                            sb.Append(Token.CharSimpleArgSplit);
                            sb.Append(arg);
                        }
                        data.addArg(t.Header, sb.ToString(1, sb.Length - 1));
                        sb.Clear();
                    }
                    else if (t.hasHeader(hTri))
                    {
                        if (tris.Count < tc)
                        {
                            tris.AddLast(new ModelTriangle(t[0], t[1], t[2]));
                        }
                    }
                    else if (t.hasHeader(hTriCount))
                    {
                        if (!int.TryParse(t[0], out tc))
                        { throw new ArgumentException("Triangle Count Expected To Be Integer"); }
                    }
                }
                #endregion

                #region Make The Vertices / Indices
                if (tc > tris.Count) { tc = tris.Count; }
                LinkedListNode<ModelTriangle> node = tris.First;
                string[] sa;
                LinkedList<string> sll = new LinkedList<string>();
                vertices = new T[tc * 3];
                indices = new int[tc * 3];
                int vi = 0, indi = 0;
                // TODO: Allow ClockWise As Option
                bool clockwise = true;
                for (int i = 0; i < tc && node != null; i++)
                {
                    for (int ti = 0; ti < 3; ti++)
                    {
                        for (int ii = 0; ii < data.ArgTypeCount; ii++)
                        {
                            sa = data[ii, node.Value[ti, ii]].Split(Token.CharSimpleArgSplit);
                            foreach (string sd in sa) { sll.AddLast(sd); }
                        }
                        converter.read(sll.ToArray(), out vertices[vi + ti]);
                        sll.Clear();
                    }
                    if (clockwise)
                    {
                        indices[indi++] = vi;
                        indices[indi++] = vi + 1;
                        indices[indi++] = vi + 2;
                    }
                    else
                    {
                        indices[indi++] = vi;
                        indices[indi++] = vi + 2;
                        indices[indi++] = vi + 1;
                    }
                    vi += 3;
                    node = node.Next;
                }
                #endregion
            }
            else
            {
                // Can't Figure Out The Vertex Information To Parse
                vertices = null;
                indices = null;
            }
        }

        #region Reading Helpers
        private struct ModelTriangle
        {
            public const char SISplit = ',';

            private int[,] iArr;
            public int this[int tri, int index]
            {
                get
                {
                    return iArr[tri, index];
                }
            }
            public ModelTriangle(string t1, string t2, string t3)
            {
                string[][] sa = 
                {
                    t1.Split(SISplit),
                    t2.Split(SISplit),
                    t3.Split(SISplit)
                };
                if (sa[0].Length != sa[1].Length ||
                    sa[1].Length != sa[2].Length
                    )
                { throw new ArgumentException("Tri Indeces Have Different Lengths"); }
                else
                {
                    iArr = new int[3, sa[0].Length];
                    for (int ti = 0; ti < 3; ti++)
                    {
                        for (int ii = 0; ii < sa[ti].Length; ii++)
                        {
                            if (!int.TryParse(sa[ti][ii], out iArr[ti, ii]))
                            {
                                throw new ArgumentException("Index Expected");
                            }
                        }
                    }
                }
            }
        }
        public class ZMVArgData
        {
            public const char SplitHeaderChar = ':';
            public static readonly char[] SplitHeader = { SplitHeaderChar };

            public Dictionary<string, ZMVArgList> argDict;
            public List<ZMVArgList> argList;
            public string this[string argType, int index]
            {
                get { return argDict[argType][index]; }
                set { argDict[argType][index] = value; }
            }
            public string this[int argIndex, int index]
            {
                get { return argList[argIndex][index]; }
            }
            public int ArgTypeCount
            {
                get { return argList.Count; }
            }
            public ZMVArgData(Token header)
            {
                argDict = new Dictionary<string, ZMVArgList>();
                argList = new List<ZMVArgList>();
                int ec;
                foreach (string arg in header)
                {
                    string[] s = arg.Split(SplitHeader, 2);
                    if (int.TryParse(s[1], out ec))
                    {
                        addArgType(s[0], ec);
                    }
                }
            }

            public bool hasArgType(string type) { return argDict.ContainsKey(type); }
            public void addArgType(string type, int expectedCount)
            {
                var al = new ZMVArgList(expectedCount);
                argDict.Add(type, al);
                argList.Add(al);
            }
            public void addArg(string type, string data)
            {
                argDict[type].add(data);
            }
        }
        public class ZMVArgList
        {
            public int listLength;
            Pager<IPI, string> list;
            public string this[int index]
            {
                get { return list[new IPI(index, listLength)]; }
                set { list[new IPI(index, listLength)] = value; }
            }
            public int Count
            {
                get { return list.Count; }
            }

            public ZMVArgList(int ec)
            {
                listLength = ec;
                list = new Pager<IPI, string>(listLength);
            }

            public void add(string data)
            {
                list.add(new IPI(Count, listLength), data);
            }

            struct IPI : IPageable
            {
                int i;
                int pi;
                public IPI(int _i, int max)
                {
                    i = _i;
                    pi = i % max;
                }
                public int PageIndex
                {
                    get { return pi; }
                }
            }
        }
        #endregion
    }
    public class ZModelReaderCalculator<T> : ZModelReader<T>
        where T : IVertexType
    {
        IVertexCalculator<T> calculator;

        public ZModelReaderCalculator(IVertexCalculator<T> c, string file)
            : base(c, file)
        {
            calculator = c;
        }

        public override void readAll(out T[] vertices, out int[] indices)
        {
            base.readAll(out vertices, out indices);
            for (int i = 0; i < indices.Length; )
            {
                calculator.calculateTriangle(
                    ref vertices[indices[i]],
                    ref vertices[indices[i + 1]],
                    ref vertices[indices[i + 2]]
                    );
                i += 3;
            }
        }
    }
}
