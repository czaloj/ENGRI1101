using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ZLibrary.Graphics;

namespace XNA3D.Graphics
{
    public class UVRectList : BatchList<UVRectList.Info, UVRectList.Vertex>
    {
        private static IndexBuffer iBuffer;
        private const int IndicesPerRect = 6;
#if HIDEF
        public static readonly IndexElementSize IndexSize = IndexElementSize.ThirtyTwoBits;
        public const int MaxRectangles = 500000;        
#else
        public static readonly IndexElementSize IndexSize = IndexElementSize.SixteenBits;
        public const int MaxRectangles = 30000;
#endif

        public UVRectList(GraphicsDevice g, int rectCount)
            : base(g, rectCount < MaxRectangles ? rectCount : MaxRectangles, 4)
        {

        }
        public override void resizeList(int maxCount)
        {
            base.resizeList(maxCount < MaxRectangles ? maxCount : MaxRectangles);
        }


        protected VertexBuffer vBuffer;
        public override void buildBuffers(GraphicsDevice g)
        {
            if (maxCount > 0)
            {
                if (iBuffer == null || iBuffer.IndexCount < maxCount * IndicesPerRect)
                {
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
                for (short vi = 0; i < maxCount; i++, ii += 6, vi += 4)
                {
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
        }

        public override void end()
        {
            vBuffer.SetData<Vertex>(vertices);
        }

        public override void set(GraphicsDevice g)
        {
            g.SetVertexBuffer(vBuffer);
            g.Indices = iBuffer;
        }
        public override void draw(GraphicsDevice g)
        {
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, count * 4, 0, count * 2);
        }

        public override void add(UVRectList.Info data)
        {
            set(count, data);
            count++;
        }
        public override void set(int i, UVRectList.Info data)
        {
            i = i * vPerInstance;
            vertices[i + 0] = new Vertex(data.TopLeft, data.UVTopLeft, data.ColorTopLeft);
            vertices[i + 1] = new Vertex(data.TopRight, data.UVTopRight, data.ColorTopRight);
            vertices[i + 2] = new Vertex(data.BottomLeft, data.UVBottomLeft, data.ColorBottomLeft);
            vertices[i + 3] = new Vertex(data.BottomRight, data.UVBottomRight, data.ColorBottomRight);
        }

        public struct Info
        {
            public Vector3 TopLeft;
            public Vector3 Right, Down;
            public Vector2 Size;
            public Vector3 TopRight
            {
                get { return TopLeft + Right * Size.X; }
            }
            public Vector3 BottomLeft
            {
                get { return TopLeft + Down * Size.Y; }
            }
            public Vector3 BottomRight
            {
                get { return TopLeft + Right * Size.X + Down * Size.Y; }
            }

            public Vector4 UVRect;
            public Vector2 UVTopLeft
            {
                get { return new Vector2(UVRect.X, UVRect.Y); }
            }
            public Vector2 UVTopRight
            {
                get { return new Vector2(UVRect.X + UVRect.Z, UVRect.Y); }
            }
            public Vector2 UVBottomLeft
            {
                get { return new Vector2(UVRect.X, UVRect.Y + UVRect.W); }
            }
            public Vector2 UVBottomRight
            {
                get { return new Vector2(UVRect.X + UVRect.Z, UVRect.Y + UVRect.W); }
            }

            public Color ColorTopLeft;
            public Color ColorTopRight;
            public Color ColorBottomLeft;
            public Color ColorBottomRight;

            #region Initializers
            public Info(Info model, Vector2 alignL, Matrix mTransform, Color cTL, Color cTR, Color cBL, Color cBR) :
                this(
                    model.TopLeft, model.Right, model.Down, model.Size, alignL,
                    mTransform, model.UVRect, cTL, cTR, cBL, cBR
                    )
            { }
            public Info(
                Vector3 modelLoc, Vector3 modelRight, Vector3 modelDown, Vector2 modelSize, Vector2 alignL,
                Matrix mTransform, Vector4 uvRect, Color cTL, Color cTR, Color cBL, Color cBR)
            {
                Vector3.Transform(ref modelLoc, ref mTransform, out TopLeft);
                Vector3.TransformNormal(ref modelRight, ref mTransform, out Right);
                Vector3.TransformNormal(ref modelDown, ref mTransform, out Down);
                TopLeft = TopLeft - (Right * alignL.X) - (Down * alignL.Y);
                Size = modelSize * new Vector4(mTransform.M11, mTransform.M12, mTransform.M13, mTransform.M14).Length();
                UVRect = uvRect;
                ColorTopLeft = cTL;
                ColorTopRight = cTR;
                ColorBottomLeft = cBL;
                ColorBottomRight = cBR;
            }
            public Info(Vector3 topLeft, Vector3 right, Vector3 down, Vector2 size, Vector4 uvRect)
            {
                TopLeft = topLeft;
                Right = right;
                Down = down;
                Size = size;
                UVRect = uvRect;
                ColorTopLeft = Color.White;
                ColorTopRight = Color.White;
                ColorBottomLeft = Color.White;
                ColorBottomRight = Color.White;
            }
            public Info(Vector3 center, Vector2 halfSize, Matrix wPosOrient, Vector4 uvRect)
            {
                Right = Vector3.TransformNormal(Vector3.Right, wPosOrient);
                Down = Vector3.TransformNormal(Vector3.Down, wPosOrient);
                TopLeft = center - Right * halfSize.X - Down * halfSize.Y;
                Size = halfSize * 2f;
                UVRect = uvRect;
                ColorTopLeft = Color.White;
                ColorTopRight = Color.White;
                ColorBottomLeft = Color.White;
                ColorBottomRight = Color.White;
            }
            public Info(
                Vector3 topLeft, Vector3 right, Vector3 down, Vector2 size, Vector4 uvRect,
                Color cTL, Color cTR, Color cBL, Color cBR
                )
            {
                TopLeft = topLeft;
                Right = right;
                Down = down;
                Size = size;
                UVRect = uvRect;
                ColorTopLeft = cTL;
                ColorTopRight = cTR;
                ColorBottomLeft = cBL;
                ColorBottomRight = cBR;
            }
            public Info(
                Vector3 center, Vector2 halfSize, Matrix wPosOrient, Vector4 uvRect,
                Color cTL, Color cTR, Color cBL, Color cBR
                )
            {
                Right = Vector3.TransformNormal(Vector3.Right, wPosOrient);
                Down = Vector3.TransformNormal(Vector3.Down, wPosOrient);
                TopLeft = center - Right * halfSize.X - Down * halfSize.Y;
                Size = halfSize * 2f;
                UVRect = uvRect;
                ColorTopLeft = cTL;
                ColorTopRight = cTR;
                ColorBottomLeft = cBL;
                ColorBottomRight = cBR;
            } 
            #endregion

            public Vertex[] getVertices()
            {
                return new Vertex[]
                {
                    new Vertex(TopLeft, UVTopLeft, ColorTopLeft),
                    new Vertex(TopRight, UVTopRight, ColorTopRight),
                    new Vertex(BottomLeft, UVBottomLeft, ColorBottomLeft),
                    new Vertex(BottomRight, UVBottomRight, ColorBottomRight)
                };
            }
        }
        public struct Vertex : IVertexType
        {
            public Vector3 Position;
            public Vector2 UV;
            public Color Color;

            public Vertex(Vector3 pos, Vector2 uv, Color c)
            {
                Position = pos;
                UV = uv;
                Color = c;
            }

            public static readonly VertexDeclaration Declaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(sizeof(float) * 5, VertexElementFormat.Color, VertexElementUsage.Color, 0)
                );
            public VertexDeclaration VertexDeclaration { get { return Declaration; } }
        }
    }
}
