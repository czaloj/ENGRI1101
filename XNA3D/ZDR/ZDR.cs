using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA3D.ZDR
{
    public static class ZDR
    {
        // Version Info
        public static readonly int[] Version = { 0, 1, 0 };
        public static int VersionMajor { get { return Version[0]; } }
        public static int VersionMinor { get { return Version[1]; } }
        public static int VersionRevision { get { return Version[2]; } }

        public static bool readInfo<I>(string file, IZDRCalculator<I> converter, out I[] verts, out int[] inds) where I : struct
        {
            bool b;
            using (ZDRPropImporter<I> importer = ZDRPropImporter<I>.fromFile(file, converter))
            {
                importer.read();
                if (importer.IsBuilt)
                {
                    verts = importer.Vertices;
                    inds = importer.Indices;
                    b = true;
                }
                else
                {
                    verts = null;
                    inds = null;
                    b = false;
                }
            }
            return b;
        }
        public static bool readBuffers<V>(GraphicsDevice g, string file, IZDRConverter<V> converter, out VertexBuffer vb, out IndexBuffer ib) where V : struct , IVertexType
        {
            bool b;
            using (ZDRImporter<V> importer = ZDRImporter<V>.fromFile(file, converter))
            {
                importer.read();
                importer.buildBuffers(g);
                if (importer.IsBuilt)
                {
                    vb = importer.VBuffer;
                    ib = importer.IBuffer;
                    b = true;
                }
                else
                {
                    vb = null;
                    ib = null;
                    b = false;
                }
            }
            return b;
        }
    }

    #region Stream Info Helper
    public enum ZDRPropertyType : byte
    {
        None = Flags.NoFlags,
        Vertex = Flags.Bit1,
        Face = Flags.Bit2,
        FaceVertex = Flags.Bit3
    }
    public class ZDRProperty
    {
        protected ZDRPropertyType type;
        public ZDRPropertyType Type
        {
            get { return type; }
        }

        protected string identifier;
        public string Identifier
        {
            get { return identifier; }
        }

        protected Token[] info;
        public int InfoCount
        {
            get { return info.Length; }
        }
        public Token this[int index]
        {
            get { return info[index]; }
            set { info[index] = value; }
        }

        public ZDRProperty(ZDRPropertyType t, string id, int infoCount)
        {
            type = t;
            identifier = id;
            info = new Token[infoCount];
        }
    }
    public class VertexConvInfo<V> where V : struct
    {
        public readonly Token[] Tokens;
        public Token this[int index]
        {
            get { return Tokens[index]; }
        }
        public readonly int VertexIndex;
        public V Vertex;

        public VertexConvInfo(Token[] t, int vi)
        {
            Tokens = t;
            VertexIndex = vi;
            Vertex = default(V);
        }

        public int indexOfInfo(Signature s)
        {
            for (int i = 0; i < Tokens.Length; i++)
            { if (s.holdsWith(Tokens[i])) { return i; } }
            return -1;
        }
        public int indexOfInfo(string h)
        {
            for (int i = 0; i < Tokens.Length; i++)
            { if (Tokens[i].hasHeader(h)) { return i; } }
            return -1;
        }
    }
    public class VertIndexing
    {
        public int VertIndex;
        public int[] VI;
        public int[] FI;
        public int[] VFI;

        public VertIndexing(string vi, string fi, string vfi)
        {
            if (string.IsNullOrWhiteSpace(vi)) { VI = new int[0]; }
            else
            {
                string[] s = vi.Split(',');
                if (s.Length <= 0) { VI = new int[0]; }
                else
                {
                    VI = new int[s.Length];
                    for (int i = 0; i < s.Length; i++)
                    { if (!int.TryParse(s[i], out VI[i])) { throw new ArgumentException("Could Not Read Index " + s[i]); } }
                }
            }

            if (string.IsNullOrWhiteSpace(fi)) { FI = new int[0]; }
            else
            {
                string[] s = fi.Split(',');
                if (s.Length <= 0) { FI = new int[0]; }
                else
                {
                    FI = new int[s.Length];
                    for (int i = 0; i < s.Length; i++)
                    { if (!int.TryParse(s[i], out FI[i])) { throw new ArgumentException("Could Not Read Index " + s[i]); } }
                }
            }

            if (string.IsNullOrWhiteSpace(vfi)) { VFI = new int[0]; }
            else
            {
                string[] s = vfi.Split(',');
                if (s.Length <= 0) { VFI = new int[0]; }
                else
                {
                    VFI = new int[s.Length];
                    for (int i = 0; i < s.Length; i++)
                    { if (!int.TryParse(s[i], out VFI[i])) { throw new ArgumentException("Could Not Read Index " + s[i]); } }
                }
            }
        }

        public static bool operator ==(VertIndexing p1, VertIndexing p2)
        {
            if (p1.VI.Length != p2.VI.Length) { return false; }
            for (int i = 0; i < p1.VI.Length; i++)
            { if (p1.VI[i] != p2.VI[i]) { return false; } }

            if (p1.FI.Length != p2.FI.Length) { return false; }
            for (int i = 0; i < p1.FI.Length; i++)
            { if (p1.FI[i] != p2.FI[i]) { return false; } }

            if (p1.VFI.Length != p2.VFI.Length) { return false; }
            for (int i = 0; i < p1.VFI.Length; i++)
            { if (p1.VFI[i] != p2.VFI[i]) { return false; } }

            return true;
        }
        public static bool operator !=(VertIndexing p1, VertIndexing p2)
        {
            return !(p1 == p2);
        }
        public override bool Equals(object obj)
        {
            return VertIndexing.ReferenceEquals(this, obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    } 
    #endregion

    public interface IZDRCalculator<I>
        where I : struct
    {
        bool CreatesUniqueVertices { get; }
        bool CullClockwise { get; }

        /// <summary>
        /// Process A Polygon Worth Of Information
        /// With Clockwise Vertex Order
        /// </summary>
        /// <param name="v1">The First Vertex To Be Built</param>
        /// <param name="v2">The Second Vertex To Be Built</param>
        /// <param name="v3">The Third Vertex To Be Built</param>
        /// <returns>True If Succeeded</returns>
        bool processPolygon(VertexConvInfo<I> v1, VertexConvInfo<I> v2, VertexConvInfo<I> v3);
    }
    public interface IZDRConverter<V> : IZDRCalculator<V>
        where V : struct, IVertexType
    {
        VertexDeclaration VDeclaration { get; }
        IndexElementSize IndexType { get; }
    }

    #region Streams
    public class ZDRPropImporter<I> : IDisposableState<ZDRPropImporter<I>>
        where I : struct
    {
        protected TokenStream s;
        protected IZDRCalculator<I> calculator;

        public I[] Vertices;
        public int[] Indices;
        protected bool built;
        public bool IsBuilt
        {
            get { return built; }
        }

        protected int vPC, fPC, vfPC, polyC;
        public int PropCountVertices { get { return vPC; } }
        public int PropCountFaces { get { return fPC; } }
        public int PropCountFaceVertices { get { return vfPC; } }
        public int PolyCount { get { return polyC; } }

        protected ZDRPropImporter(TokenStream ts, IZDRCalculator<I> calc)
        {
            built = false;
            s = ts;
            calculator = calc;
        }
        public static ZDRPropImporter<I> fromFile(string file, IZDRCalculator<I> calc)
        {
            if (!File.Exists(file)) { throw new FileNotFoundException("Could Not Find ZDR File", file); }
            return new ZDRPropImporter<I>(new TokenStream(file, FileMode.Open), calc);
        }

        public void read()
        {
            // Read The Header Information
            if (!readHeader()) { return; }

            // Read The Rest Of The File
            s.readAll();

            // Search For Information
            ZDRProperty[] pV, pF, pVF;
            if (!searchPropertyType(ZDRPropertyType.Vertex, out pV)) { return; }
            if (!searchPropertyType(ZDRPropertyType.Face, out pF)) { return; }
            if (!searchPropertyType(ZDRPropertyType.FaceVertex, out pVF)) { return; }

            if (!readPolygons(pV, pF, pVF)) { return; }

            built = true;
        }

        public static readonly Signature SigVersion = new Signature("zdr", 3);
        public static readonly Signature SigPropCount = new Signature("propcount", 3);
        public static readonly Signature SigPolyCount = new Signature("polycount", 1);
        protected bool readHeader()
        {
            Token t, ft = null;
            if (!s.readUntilAllSignatures(new LinkedQueue<ISignature>(SigVersion, SigPropCount, SigPolyCount)))
            { return false; }

            // Get The First Correct Token And Keep Track Of Stream Order
            if (!SigVersion.holdsWith(s.First))
            { ft = s.First; s.search(SigVersion, out t); }
            else { s.next(out t); }

            #region Check For Correct Version
            int[] version = new int[3];
            if (!t.getArg<int>(0, ref version[0]) ||
                !t.getArg<int>(1, ref version[1]) ||
                !t.getArg<int>(2, ref version[2])
                )
            { return false; }
            if (version[0] != ZDR.VersionMajor ||
                version[1] != ZDR.VersionMinor ||
                version[2] != ZDR.VersionRevision
                )
            { return false; }
            #endregion

            if (!SigPropCount.holdsWith(s.First))
            { ft = ft ?? s.First; s.search(SigPropCount, out t); }
            else { s.next(out t); }

            #region Get Property Counts
            if (!t.getArg<int>(0, ref vPC) ||
                !t.getArg<int>(1, ref fPC) ||
                !t.getArg<int>(2, ref vfPC)
                )
            { return false; }
            if (vPC < 0 || fPC < 0 || vfPC < 0) { return false; }
            #endregion

            if (!SigPolyCount.holdsWith(s.First))
            { ft = ft ?? s.First; s.search(SigPolyCount, out t); }
            else { s.next(out t); }

            #region Get Polygon Counts
            if (!t.getArg<int>(0, ref polyC)) { return false; }
            if (polyC < 0) { return false; }
            #endregion

            // Recycle Until The Beginning
            if (ft != null) { while (s.First != ft) { s.recycle(); } }

            return true;
        }

        public static readonly Signature SigBeginPropType = new Signature("begin", 1);
        public static readonly Signature SigBeginProp = new Signature("begin", 4);
        public static readonly Signature SigEndProp = new Signature("end", 3);
        public static readonly Signature SigEndPropType = new Signature("end", 1);
        protected bool searchPropertyType(ZDRPropertyType type, out ZDRProperty[] p)
        {
            string pt;
            switch (type)
            {
                case ZDRPropertyType.Vertex: pt = "v"; p = new ZDRProperty[PropCountVertices]; break;
                case ZDRPropertyType.Face: pt = "f"; p = new ZDRProperty[PropCountFaces]; break;
                case ZDRPropertyType.FaceVertex: pt = "vf"; p = new ZDRProperty[PropCountFaceVertices]; break;
                default: p = null; return false;
            }

            Token t = null;
            int tr = s.TokensRemaining, i = 0;
            while (i < tr)
            {
                if (SigBeginPropType.holdsWith(s.First) && s.First[0].Equals(pt))
                {
                    s.next(out t);
                    i = -1;
                    break;
                }
                i++;
            }
            if (i != -1) { return false; }

            tr = s.TokensRemaining;
            i = 0;
            string currentPropKey = null;
            int propIndex = -1, propInfoCount = -1, propInfoIndex = -1;
            while (i < tr)
            {
                if (currentPropKey == null)
                {
                    #region Check For End Of Property Type If Not Reading A Property
                    if (SigEndPropType.holdsWith(s.First) && s.First[0].Equals(pt))
                    {
                        // End Reading The Property Type
                        s.next(out t);
                        i = -1;
                        break;
                    }
                    #endregion
                    else if (SigBeginProp.holdsWith(s.First) && s.First[0].Equals(pt))
                    {
                        // Create A New Property
                        s.next(out t);
                        if (!t.getArg<int>(1, ref propIndex) ||
                            string.IsNullOrWhiteSpace(t[2]) ||
                            !t.getArg<int>(3, ref propInfoCount)
                            )
                        { return false; }
                        currentPropKey = t[2];

                        // Set To Read Headers As Their Originals
                        s.HeaderProcessor = Token.ReturnOriginal;
                        p[propIndex] = new ZDRProperty(type, currentPropKey, propInfoCount);
                    }
                    else { s.recycle(); }
                }
                else
                {
                    #region While Reading A Property
                    if (s.First.hasHeader(currentPropKey) && s.First.ArgumentCount > 1)
                    {
                        // Read The Property Info
                        s.next(out t);
                        if (!t.getArg<int>(0, ref propInfoIndex))
                        { return false; }
                        p[propIndex][propInfoIndex] = propInfo(t);
                    }
                    else if (SigEndProp.holdsWith(s.First) && s.First[0].Equals(pt))
                    {
                        // End Reading Property
                        s.next(out t);
                        currentPropKey = null;
                        propIndex = -1;
                        propInfoCount = -1;
                        propInfoIndex = -1;

                        // Reset To Read Headers As Lowercase
                        s.HeaderProcessor = Token.ReturnLower;
                    }
                    #endregion
                    else { s.recycle(); }
                }
                i++;
            }
            if (i != -1) { return false; }
            return true;
        }
        protected Token propInfo(Token t)
        {
            StringBuilder sb = new StringBuilder(t.Original.Length);
            int ai = 1;
            for (; ai < t.ArgumentCount - 1; ai++)
            {
                sb.Append(t[ai]);
                sb.Append('|');
            }
            sb.Append(t[ai]);
            return new Token(string.Format("{0} [{1}]", t.Header, sb.ToString()));
        }

        public static readonly Signature SigPoly = new Signature("p", 9);
        protected bool readPolygons(ZDRProperty[] pV, ZDRProperty[] pF, ZDRProperty[] pVF)
        {
            Token t = null;
            int tr = s.TokensRemaining, i = 0;
            while (i < tr)
            {
                if (SigBeginPropType.holdsWith(s.First) && s.First[0].Equals("p"))
                {
                    s.next(out t);
                    i = -1;
                    break;
                }
                i++;
            }
            if (i != -1) { return false; }

            tr = s.TokensRemaining;
            i = 0;
            int polyIndex = 0;
            VertIndexing[,] polys = new VertIndexing[PolyCount, 3];
            while (i < tr)
            {
                if (SigEndPropType.holdsWith(s.First) && s.First[0].Equals("p"))
                {
                    s.next(out t);
                    i = -1;
                    break;
                }
                else if (SigPoly.holdsWith(s.First))
                {
                    s.next(out t);
                    polys[polyIndex, 0] = new VertIndexing(t[0], t[1], t[2]);
                    polys[polyIndex, 1] = new VertIndexing(t[3], t[4], t[5]);
                    polys[polyIndex, 2] = new VertIndexing(t[6], t[7], t[8]);
                    polyIndex++;
                }
                else { s.recycle(); }
                i++;
            }
            if (polyIndex < PolyCount) { return false; }

            #region Build The IndexBuffer

            #region Generate Unique Vertices
            VertIndexing[] vUnique;
            Token[][] vUniqueInfo;
            if (!calculator.CreatesUniqueVertices)
            {
                //Find Unique Vertices
                int[] ii = new int[PolyCount];
                LinkedList<VertIndexing> uniqueVertices = new LinkedList<VertIndexing>();
                VertIndexing vio;
                for (int pi = 0; pi < PolyCount; pi++)
                {
                    for (int vfi = 0; vfi < 3; vfi++)
                    {
                        vio = getUnique(uniqueVertices, polys[pi, vfi]);
                        if (object.ReferenceEquals(vio, null))
                        {
                            polys[pi, vfi].VertIndex = uniqueVertices.Count;
                            uniqueVertices.AddLast(polys[pi, vfi]);
                        }
                        else
                        {
                            polys[pi, vfi] = vio;
                        }
                    }
                }
                vUnique = uniqueVertices.ToArray();
            }
            else
            {
                int vi = 0;
                vUnique = new VertIndexing[PolyCount * 3];
                for (int pi = 0; pi < PolyCount; pi++)
                {
                    for (int vfi = 0; vfi < 3; vfi++)
                    {
                        polys[pi, vfi].VertIndex = vi;
                        vUnique[vi] = polys[pi, vfi];
                        vi++;
                    }
                }
            }
            vUniqueInfo = new Token[vUnique.Length][];
            for (i = 0; i < vUniqueInfo.Length; i++)
            {
                vUniqueInfo[i] = obtainInfo(vUnique[i], pV, pF, pVF);
            }
            #endregion

            Indices = new int[PolyCount * 3];
            i = 0;
            for (int pi = 0; pi < PolyCount; pi++)
            {
                for (int vfi = 0; vfi < 3; vfi++)
                {
                    Indices[i] = (short)polys[pi, calculator.CullClockwise ? vfi : 2 - vfi].VertIndex;
                    i++;
                }
            }

            #endregion

            #region Build The Vertex Buffer
            Vertices = new I[vUnique.Length];
            for (int pi = 0; pi < PolyCount; pi++)
            {
                VertexConvInfo<I>
                    v1 = new VertexConvInfo<I>(vUniqueInfo[polys[pi, 0].VertIndex], polys[pi, 0].VertIndex),
                    v2 = new VertexConvInfo<I>(vUniqueInfo[polys[pi, 1].VertIndex], polys[pi, 1].VertIndex),
                    v3 = new VertexConvInfo<I>(vUniqueInfo[polys[pi, 2].VertIndex], polys[pi, 2].VertIndex)
                    ;
                calculator.processPolygon(v1, v2, v3);
                Vertices[v1.VertexIndex] = v1.Vertex;
                Vertices[v2.VertexIndex] = v2.Vertex;
                Vertices[v3.VertexIndex] = v3.Vertex;
            }
            #endregion

            return true;
        }
        protected VertIndexing getUnique(LinkedList<VertIndexing> l, VertIndexing test)
        {
            foreach (var v in l)
            {
                if (v == test) { return v; }
            }
            return null;
        }
        protected Token[] obtainInfo(VertIndexing vi, ZDRProperty[] pV, ZDRProperty[] pF, ZDRProperty[] pVF)
        {
            Token[] s = new Token[PropCountVertices + PropCountFaces + PropCountFaceVertices];
            int i = 0;
            for (int pi = 0; pi < PropCountVertices; pi++)
            { s[i++] = pV[pi][vi.VI[pi]]; }
            for (int pi = 0; pi < PropCountFaces; pi++)
            { s[i++] = pF[pi][vi.FI[pi]]; }
            for (int pi = 0; pi < PropCountFaceVertices; pi++)
            { s[i++] = pVF[pi][vi.VFI[pi]]; }
            return s;
        }

        #region Disposal
        public bool IsDisposed { get; private set; }
        public event Action<ZDRPropImporter<I>> OnDisposal;
        ~ZDRPropImporter() { this.TryDispose(); }
        public void Dispose()
        {
            IsDisposed = true;
            if (OnDisposal != null) { OnDisposal(this); }
            s.Dispose();
        }
        #endregion
    }
    public class ZDRImporter<V> : ZDRPropImporter<V>
        where V : struct, IVertexType
    {
        protected IZDRConverter<V> converter;

        protected VertexBuffer vBuffer;
        protected IndexBuffer iBuffer;
        public VertexBuffer VBuffer
        {
            get { return vBuffer; }
        }
        public IndexBuffer IBuffer
        {
            get { return iBuffer; }
        }

        protected ZDRImporter(TokenStream ts, IZDRConverter<V> conv)
            : base(ts, conv)
        {
            vBuffer = null;
            iBuffer = null;
            converter = conv;
        }
        new public static ZDRImporter<V> fromFile(string file, IZDRConverter<V> conv)
        {
            if (!File.Exists(file)) { throw new FileNotFoundException("Could Not Find ZDR File", file); }
            return new ZDRImporter<V>(new TokenStream(file, FileMode.Open), conv);
        }

        public void buildBuffers(GraphicsDevice g)
        {
            if (IsBuilt)
            {
                vBuffer = new VertexBuffer(g, converter.VDeclaration, Vertices.Length, BufferUsage.WriteOnly);
                vBuffer.SetData(Vertices);
                iBuffer = new IndexBuffer(g, converter.IndexType, Indices.Length, BufferUsage.WriteOnly);
                switch (converter.IndexType)
                {
                    case IndexElementSize.SixteenBits:
                        short[] s = new short[Indices.Length];
                        for (int i = 0; i < Indices.Length; i++) { s[i] = (short)Indices[i]; }
                        iBuffer.SetData(s);
                        break;
                    case IndexElementSize.ThirtyTwoBits:
                        iBuffer.SetData(Indices);
                        break;
                    default:
                        built = false;
                        vBuffer.Dispose();
                        iBuffer.Dispose();
                        return;
                }
            }
        }
    } 
    #endregion
}
