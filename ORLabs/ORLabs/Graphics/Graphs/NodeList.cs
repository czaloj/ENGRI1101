using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ZLibrary.Graphics;

namespace ORLabs.Graphics.Graphs {
    public struct NodeInfo {
        public Vector3 Position;
        public Vector2 HalfSize;
        public Color Color1;
        public Color Color2;

        public Vector3 TopLeft {
            get { return new Vector3(Position.X - HalfSize.X, Position.Y + HalfSize.Y, Position.Z); }
        }
        public Vector3 TopRight {
            get { return new Vector3(Position.X + HalfSize.X, Position.Y + HalfSize.Y, Position.Z); }
        }
        public Vector3 BottomLeft {
            get { return new Vector3(Position.X - HalfSize.X, Position.Y - HalfSize.Y, Position.Z); }
        }
        public Vector3 BottomRight {
            get { return new Vector3(Position.X + HalfSize.X, Position.Y - HalfSize.Y, Position.Z); }
        }

        public NodeInfo(Vector3 pos, Vector2 size, Color c1, Color c2) {
            Position = pos;
            HalfSize = size;
            Color1 = c1;
            Color2 = c2;
        }
    }

    public class NodeList : BatchList<NodeInfo, NodeList.Vertex> {
        private static IndexBuffer iBuffer;
        private const int IndicesPerRect = 6;
#if HIDEF
        public static readonly IndexElementSize IndexSize = IndexElementSize.ThirtyTwoBits;
        public const int MaxNodes = 500000;
#else
        public static readonly IndexElementSize IndexSize = IndexElementSize.SixteenBits;
        public const int MaxNodes = 30000;
#endif

        public bool ShouldBuild;

        public NodeList(GraphicsDevice g, int rectCount)
            : base(g, rectCount < MaxNodes ? rectCount : MaxNodes, 4) {
        }
        public override void resizeList(int maxCount) {
            base.resizeList(maxCount < MaxNodes ? maxCount : MaxNodes);
        }

        protected VertexBuffer vBuffer;
        public override void buildBuffers(GraphicsDevice g) {
            if(maxCount == 0) { vBuffer = null; return; }
            if(iBuffer == null || iBuffer.IndexCount < maxCount * IndicesPerRect) {
                iBuffer = new IndexBuffer(g, IndexSize, maxCount * IndicesPerRect, BufferUsage.WriteOnly);
#if HIDEF
                int[] ind = new int[maxCount * IndicesPerRect];
                for (int i = 0, ii = 0, vi = 0; i < maxCount; i++, ii += 6, vi += 4)
                {
                    ind[ii] = vi;
                    ind[ii + 1] = vi + 1;
                    ind[ii + 2] = vi + 2;
                    ind[ii + 3] = vi + 2;
                    ind[ii + 4] = vi + 1;
                    ind[ii + 5] = vi + 3;
                }
                iBuffer.SetData<int>(ind);
#elif REACH
                short[] ind = new short[maxCount * IndicesPerRect];
                int i = 0, ii = 0;
                for(short vi = 0; i < maxCount; i++, ii += 6, vi += 4) {
                    ind[ii] = vi;
                    ind[ii + 1] = (short)(vi + 1);
                    ind[ii + 2] = (short)(vi + 2);
                    ind[ii + 3] = (short)(vi + 2);
                    ind[ii + 4] = (short)(vi + 1);
                    ind[ii + 5] = (short)(vi + 3);
                }
                iBuffer.SetData<short>(ind);
#endif
            }
            vBuffer = new VertexBuffer(g, Vertex.Declaration, maxCount * vPerInstance, BufferUsage.WriteOnly);
        }

        public override void end() {
            if(vBuffer != null) {
                vBuffer.SetData<Vertex>(vertices);
                ShouldBuild = false;
            }
        }

        public override void set(GraphicsDevice g) {
            g.SetVertexBuffer(vBuffer);
            g.Indices = iBuffer;
        }
        public override void draw(GraphicsDevice g) {
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, count * 4, 0, count * 2);
        }

        public override void add(NodeInfo data) {
            set(count, data);
            count++;
        }
        public override void set(int i, NodeInfo data) {
            i = i * vPerInstance;
            vertices[i + 0] = new Vertex(data.TopLeft, 0, data.Color1, data.Color2);
            vertices[i + 1] = new Vertex(data.TopRight, 1, data.Color1, data.Color2);
            vertices[i + 2] = new Vertex(data.BottomLeft, 2, data.Color1, data.Color2);
            vertices[i + 3] = new Vertex(data.BottomRight, 3, data.Color1, data.Color2);
            ShouldBuild = true;
        }

        public void setColor(int i, Color c1, Color c2) {
            i = i * vPerInstance;
            vertices[i + 0].Color1 = c1; vertices[i + 0].Color2 = c2;
            vertices[i + 1].Color1 = c1; vertices[i + 1].Color2 = c2;
            vertices[i + 2].Color1 = c1; vertices[i + 2].Color2 = c2;
            vertices[i + 3].Color1 = c1; vertices[i + 3].Color2 = c2;
            ShouldBuild = true;
        }

        public struct Vertex : IVertexType {
            public Vector4 PosCorner;
            public Color Color1;
            public Color Color2;

            public Vertex(Vector3 pos, int corner, Color c1, Color c2) {
                PosCorner = new Vector4(pos, corner);
                Color1 = c1;
                Color2 = c2;
            }

            public static readonly VertexDeclaration Declaration = new VertexDeclaration(
                new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * 4, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(sizeof(float) * 5, VertexElementFormat.Color, VertexElementUsage.Color, 1)
                );
            public VertexDeclaration VertexDeclaration {
                get { return Declaration; }
            }
        }
    }
}
