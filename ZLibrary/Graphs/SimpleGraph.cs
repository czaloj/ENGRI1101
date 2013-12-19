using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace ZLibrary.Graphs
{
    public class SVGraph : Graph<SVGraph.Edge, SVGraph.Node>
    {
        public SVGraph(int nCap, int eCap) : base(nCap, eCap) { }

        public class Edge : Edge<Node>
        {
            public Edge() : base() { }

            EdgeData data;

            public override void read(TokenStream s)
            {
                data.read(s);
            }
            public override void write(TokenStream s)
            {
                data.write(s);
            }
        }
        public class EdgeData
        {
            public int Weight;
            public float Width;
            public Color Color;
            public byte Flags;

            public EdgeData(int w, Color c, float width, byte f = 0x00)
            {
                Weight = w;
                Width = width;
                Color = c;
                Flags = f;
            }

            #region IGraphData Members
            static readonly Signature EDSig = new Signature("ed", 3);

            public void read(TokenStream s)
            {
                if(!s.readUntilSignature(EDSig))
                { throw new Exception("Could Not Find Data"); }
                ArgBinds ab = new ArgBinds(
                    new TokenGenericConverters.Int(ref Weight),
                    new TokenGenericConverters.Float(ref Width),
                    new TokenGenericConverters.Byte(ref Flags)
                    );
                if (!ab.convert(s.LastAdded))
                { throw new Exception("Could Not Read Arguments"); }
                Color = new Color(0, 0, 150, 30);
            }
            public void write(TokenStream s)
            {
                s.write(getToken());
            }
            public Token getToken()
            {
                return Token.fromHeaderArgs(EDSig.Header, Weight, Width, Flags);
            }
            #endregion
        }

        public class Node : Node<Edge>
        {
            public Node() : base() { Data = NodeData.None; }

            public NodeData Data;

            public override void read(TokenStream s)
            {
                Data.read(s);
            }
            public override void write(TokenStream s)
            {
                Data.write(s);
            }
        }
        public class NodeData
        {
            private static readonly NodeData none = new NodeData(Vector2.Zero, 0, Color.Black, System.Flags.NoFlags);
            public static NodeData None { get { return new NodeData(none); } }

            public Vector3 Position;
            public float Radius;
            public Vector2 Position2 { get { return new Vector2(Position.X, Position.Y); } }
            public Color Color;
            public byte Flags;

            public NodeData(NodeData d)
            {
                Position = d.Position;
                Radius = d.Radius;
                Color = d.Color;
                Flags = d.Flags;
            }

            public NodeData(Vector2 p, float r, Color c, byte flags = 0x00)
                : this(new Vector3(p, -0.5f), r, c, flags)
            {
            }
            public NodeData(Vector3 p, float r, Color c, byte flags = 0x00)
            {
                Position = p;
                Radius = r;
                Color = c;
                Flags = flags;
            }

            #region IGraphData Members
            static readonly Signature NDSig = new Signature("nd", 5);

            public void read(TokenStream s)
            {
                if (!s.readUntilSignature(NDSig))
                { throw new Exception("Could Not Find Data"); }
                ArgBinds ab = new ArgBinds(
                    new TokenGenericConverters.Float(ref Position.X),
                    new TokenGenericConverters.Float(ref Position.Y),
                    new TokenGenericConverters.Float(ref Position.Z),
                    new TokenGenericConverters.Float(ref Radius),
                    new TokenGenericConverters.Byte(ref Flags)
                    );
                if (!ab.convert(s.LastAdded))
                { throw new Exception("Could Not Read Arguments"); }
                Color = new Color(150, 0, 0, 30);
            }
            public void write(TokenStream s)
            {
                s.write(getToken());
            }
            public Token getToken()
            {
                return Token.fromHeaderArgs(NDSig.Header, Position.X, Position.Y, Position.Z, Radius, Flags);
            }
            #endregion
        }
    }
}
