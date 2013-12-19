
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ZLibrary.Graphics;

namespace XNA2D.Graphics
{
    public abstract class BMFontList : BatchList<BMFontList.Info>
    {
        protected BMFont font;

        public BMFontList(BMFont f, int maxCharCount)
            : base(maxCharCount)
        {
            font = f;
        }

        public struct Info
        {
            public Vector4 UVRect;
            public Matrix World;

            public static Info[] getInfo(string s, float sUV, BMFont font, Matrix worldStart)
            {
                Info[] info = new Info[s.Length];
                Vector3 moveTransform = Vector3.Zero;
                BMFont.GlyphInformation gi;
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    gi = font[c];
                    if (gi.Character == c)
                    {
                        info[i] = new Info()
                        {
                            UVRect = new Vector4(gi.UVStart, gi.UVSize.X, gi.UVSize.Y),
                            World = Matrix.CreateScale(new Vector3(gi.Size, 1)) * Matrix.CreateTranslation(moveTransform) * worldStart
                        };
                        moveTransform.X += gi.Size.X;
                    }
                }
                return info;
            }
        }
    }

    public class BMFontInstanceList : BMFontList
    {
        #region Used For Hardware Instancing
        public static VertexBuffer vRectBuffer;
        public static IndexBuffer iRectBuffer;
        public static void buildRectBuffers(GraphicsDevice g)
        {
            vRectBuffer = new VertexBuffer(g, CharRectVertex.Declaration, 4, BufferUsage.None);
            vRectBuffer.SetData<CharRectVertex>(new CharRectVertex[]{
                CharRectVertex.TopLeft,
                CharRectVertex.TopRight,
                CharRectVertex.BottomLeft,
                CharRectVertex.BottomRight
            });
            iRectBuffer = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.None);
            iRectBuffer.SetData<int>(new int[] { 0, 1, 2, 2, 1, 3 });
        }

        public struct CharRectVertex : IVertexType
        {
            public static readonly CharRectVertex TopLeft = new CharRectVertex() { SizeApplication = new Vector4(0, 0, 0, 1) };
            public static readonly CharRectVertex TopRight = new CharRectVertex() { SizeApplication = new Vector4(1, 0, 0, 1) };
            public static readonly CharRectVertex BottomLeft = new CharRectVertex() { SizeApplication = new Vector4(0, -1, 0, 1) };
            public static readonly CharRectVertex BottomRight = new CharRectVertex() { SizeApplication = new Vector4(1, -1, 0, 1) };

            public Vector4 SizeApplication;

            public static readonly VertexDeclaration Declaration = new VertexDeclaration(
                new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0)
                );
            public VertexDeclaration VertexDeclaration
            {
                get { return Declaration; }
            }

        }
        public struct CharVertex : IVertexType
        {
            public Matrix World;
            public Vector4 UVRect;

            public CharVertex(Vector4 uv, Matrix w)
            {
                UVRect = uv;
                World = w;
            }

            public static readonly VertexDeclaration Declaration = new VertexDeclaration(
                new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(sizeof(float) * 12, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
                new VertexElement(sizeof(float) * 16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4)
                );
            public VertexDeclaration VertexDeclaration
            {
                get { return Declaration; }
            }

            public override string ToString()
            {
                return Vector4.Transform(new Vector4(1, -1, 0, 1), World).ToString();// +" - " + UVRect.ToString();
            }
        }
        #endregion

        protected DynamicVertexBuffer vCharBuffer;

        public BMFontInstanceList(GraphicsDevice g, BMFont f, int maxCharCount)
            : base(f, maxCharCount)
        {
            vCharBuffer = new DynamicVertexBuffer(g, CharVertex.Declaration, maxCount, BufferUsage.None);
        }

        CharVertex[] v;
        public override void begin()
        {
            count = 0;
            v = new CharVertex[maxCount];
        }
        public override void add(BMFontList.Info data)
        {
            v[count] = new CharVertex(data.UVRect, data.World);
            count++;
        }
        public override void end()
        {
            vCharBuffer.SetData<CharVertex>(v);
            v = null;
        }

        public override void set(GraphicsDevice g)
        {
            g.SetVertexBuffers(
                vRectBuffer,
                new VertexBufferBinding(vCharBuffer, 0, 1)
                );
            g.Indices = iRectBuffer;
        }
        public override void draw(GraphicsDevice g)
        {
            g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2, count);
        }
    }

    public class BMFontSimpleList : BMFontList
    {
        protected DynamicVertexBuffer vCharBuffer;
        protected IndexBuffer iCharBuffer;

        public BMFontSimpleList(GraphicsDevice g, BMFont f, int maxCharCount)
            : base(f, maxCharCount)
        {
            vCharBuffer = new DynamicVertexBuffer(g, Vertex.Declaration, maxCount, BufferUsage.None);
            iCharBuffer = new IndexBuffer(g, IndexElementSize.SixteenBits, maxCount * 6, BufferUsage.None);
            short[] ind = new short[maxCount * 6];
            short vi = 0;
            for (int i = 0; i < ind.Length; vi += 4)
            {
                ind[i++] = vi;
                ind[i++] = (short)(vi + 1);
                ind[i++] = (short)(vi + 2);
                ind[i++] = (short)(vi + 2);
                ind[i++] = (short)(vi + 1);
                ind[i++] = (short)(vi + 3);
            }
            iCharBuffer.SetData<short>(ind);
        }

        Vertex[] v;
        public override void begin()
        {
            v = new Vertex[MaxCount * 4];
            count = 0;
        }
        public override void add(BMFontList.Info data)
        {
            int vi = count * 4;
            v[vi] = Vertex.topLeft(data.UVRect, data.World);
            v[vi + 1] = Vertex.topRight(data.UVRect, data.World);
            v[vi + 2] = Vertex.bottomLeft(data.UVRect, data.World);
            v[vi + 3] = Vertex.bottomRight(data.UVRect, data.World);
            count++;
        }
        public override void end()
        {
            vCharBuffer.SetData<Vertex>(v);
        }

        public override void set(GraphicsDevice g)
        {
            g.SetVertexBuffer(vCharBuffer);
            g.Indices = iCharBuffer;
        }
        public override void draw(GraphicsDevice g)
        {
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, count * 4, 0, count * 2);
        }

        public struct Vertex : IVertexType
        {
            public Vector3 Position;
            public Vector2 UV;

            public static readonly VertexDeclaration Declaration = new VertexDeclaration(
                new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
                );

            public static Vertex topLeft(Vector4 uvRect, Matrix World)
            {
                return new Vertex()
                {
                    Position = Vector3.Transform(new Vector3(0, 0, 0), World),
                    UV = new Vector2(uvRect.X, uvRect.Y)
                };
            }
            public static Vertex topRight(Vector4 uvRect, Matrix World)
            {
                return new Vertex()
                {
                    Position = Vector3.Transform(new Vector3(1, 0, 0), World),
                    UV = new Vector2(uvRect.X + uvRect.Z, uvRect.Y)
                };
            }
            public static Vertex bottomLeft(Vector4 uvRect, Matrix World)
            {
                return new Vertex()
                {
                    Position = Vector3.Transform(new Vector3(0, -1, 0), World),
                    UV = new Vector2(uvRect.X, uvRect.Y + uvRect.W)
                };
            }
            public static Vertex bottomRight(Vector4 uvRect, Matrix World)
            {
                return new Vertex()
                {
                    Position = Vector3.Transform(new Vector3(1, -1, 0), World),
                    UV = new Vector2(uvRect.X + uvRect.Z, uvRect.Y + uvRect.W)
                };
            }

            public VertexDeclaration VertexDeclaration
            {
                get { return Declaration; }
            }
        }
    }
}
