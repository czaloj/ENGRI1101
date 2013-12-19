using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XNA3D.Cameras;

namespace XNA3D.DeferredRendering
{
    public class Skydome : IDisposable
    {
        public static Effect SkyDomeEffect;
        public static void loadEffect(string file, ContentManager content)
        {
            SkyDomeEffect = content.Load<Effect>(file);
            SkyDomeEffect.CurrentTechnique = SkyDomeEffect.Techniques[0];
        }

        protected Texture2D gradient;

        protected Vertex[] vertices;
        protected VertexBuffer vb;
        protected IndexBuffer ib;

        protected int slices;
        protected int rings;
        protected int triangles;
        protected float dayLength;
        float timeX;
        float angle;

        public Skydome(GraphicsDevice g, Info info, Texture2D gradient)
        {
            slices = info.Slices;
            rings = info.Rings;
            dayLength = info.DayLength;
            this.gradient = gradient;
            timeX = 0f;

            build(g);
            //int[] ind = getIndeces();
            //vb = new VertexBuffer(g, Vertex.Declaration, vertices.Length, BufferUsage.None);
            //ib = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, ind.Length, BufferUsage.None);
            //vb.SetData<Vertex>(vertices);
            //ib.SetData<int>(ind);

            SunRayDirection = Vector3.Down;
        }
        ~Skydome()
        {
            if (vb != null && !vb.IsDisposed) { vb.Dispose(); }
            if (ib != null && !ib.IsDisposed) { ib.Dispose(); }
        }
        public void Dispose()
        {
            System.GC.SuppressFinalize(this);
            if (vb != null && !vb.IsDisposed) { vb.Dispose(); }
            if (ib != null && !ib.IsDisposed) { ib.Dispose(); }
        }

        protected void build(GraphicsDevice g)
        {
            vertices = new Vertex[rings * slices + 2];
            vertices[0] = new Vertex(Vector3.Up, Vector3.Up);
            vertices[vertices.Length - 1] = new Vertex(Vector3.Down, Vector3.Down);

            int i = 1;
            float dPhi = MathHelper.Pi / (rings + 1f);
            float dTheta = (MathHelper.Pi * 2f) / slices;
            //float phi = MathHelper.Pi;
            //Vector3 pos;
            //Matrix rot;
            //for (int r = 0; r < rings; r++)
            //{
            //    phi -= dPhi;
            //    rot = Matrix.CreateRotationX(phi);
            //    for (int s = 0; s < slices; s++, i++)
            //    {
            //        rot = rot * Matrix.CreateRotationY(dTheta);
            //        pos = Vector3.Transform(Vector3.Forward, rot);
            //        vertices[getVertexIndex(r, s)] = new Vertex(pos, Vector3.Normalize(pos));
            //    }
            //}

            float[] phi = new float[rings + 2];
            for (i = 0; i < phi.Length; i++)
            {
                phi[i] = (dPhi * i) - MathHelper.PiOver2;
            }
            phi[0] = -MathHelper.PiOver2;
            phi[rings + 1] = MathHelper.PiOver2;
            float[] theta = new float[slices + 1];
            for (i = 0; i < theta.Length; i++)
            {
                theta[i] = dTheta * i;
            }
            theta[slices] = MathHelper.Pi * 2f;
            MeshCreator.Vertex[] verts;
            int[] ind;
            MeshCreator.buildSphere(new MeshCreator.SphereInfo(phi, theta), out verts, out ind);
            vertices = new Vertex[verts.Length];
            for (i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vertex(verts[i].Position, verts[i].Normal);
            }
            triangles = ind.Length / 3;
            vb = new VertexBuffer(g, Vertex.Declaration, vertices.Length, BufferUsage.None);
            ib = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, ind.Length, BufferUsage.None);
            vb.SetData<Vertex>(vertices);
            ib.SetData<int>(ind);
        }
        protected int[] getIndeces()
        {
            triangles = slices * 2 * rings;
            int[] ind = new int[3 * triangles];
            int i = 0;

            for (int s = 0; s < slices; s++)
            {
                //Top
                ind[i++] = 0;
                ind[i++] = getVertexIndex(0, s);
                ind[i++] = getVertexIndex(0, (s + 1) % slices);

                //Bottom
                ind[i++] = vertices.Length - 1;
                ind[i++] = getVertexIndex(rings - 1, (s + 1) % slices);
                ind[i++] = getVertexIndex(rings - 1, s);
            }

            for (int r = 0; r < rings - 1; r++)
            {
                for (int s = 0; s < slices; s++)
                {
                    ind[i++] = getVertexIndex(r, s);
                    ind[i++] = getVertexIndex(r + 1, s);
                    ind[i++] = getVertexIndex(r + 1, (s + 1) % slices);

                    ind[i++] = getVertexIndex(r + 1, (s + 1) % slices);
                    ind[i++] = getVertexIndex(r, (s + 1) % slices);
                    ind[i++] = getVertexIndex(r, s);
                }
            }
            return ind;
        }
        protected int getVertexIndex(int r, int s)
        {
            return slices * r + s + 1;
        }

        public Vector3 SunRayDirection;

        public void update(float dt)
        {
            angle = MathHelper.WrapAngle(angle + dt * 4f * MathHelper.Pi / dayLength);
            timeX = MathHelper.Clamp((float)(Math.Cos(angle) + 1.0) / 2f, 0, 1);
        }
        public void draw(GraphicsDevice g, ACCamera camera, Effect e)
        {
            g.DepthStencilState = DepthStencilState.None;
            g.RasterizerState = RasterizerState.CullClockwise;

            SkyDomeEffect.Parameters["View"].SetValue(camera.View);
            SkyDomeEffect.Parameters["Projection"].SetValue(camera.Projection);
            SkyDomeEffect.Parameters["TimeX"].SetValue(timeX);
            SkyDomeEffect.Parameters["SunRayDirection"].SetValue(SunRayDirection);
            g.Textures[0] = gradient;
            g.SamplerStates[0] = SamplerState.LinearClamp;
            SkyDomeEffect.CurrentTechnique.Passes[0].Apply();

            g.SetVertexBuffer(vb);
            g.Indices = ib;
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, triangles);

            g.RasterizerState = RasterizerState.CullCounterClockwise;
            g.DepthStencilState = DepthStencilState.Default;
        }

        public struct Info
        {
            public int Slices;
            public int Rings;

            public float DayLength;

            /// <summary>
            /// Coloring Info For The Skydome
            /// </summary>
            /// <param name="s">Slices Around Dome</param>
            /// <param name="top">Top Color</param>
            /// <param name="bottom">Bottom Color</param>
            /// <param name="ch">The Color Of The Ring And Height From 1 to -1 (Sine of Angle)</param>
            public Info(int s, int r, float dl)
            {
                Slices = s;
                Rings = r;
                DayLength = dl;
            }
        }
        public struct Vertex : IVertexType
        {
            public Vector3 Position;
            public Vector3 Normal;

            public Vertex(Vector3 position, Vector3 normal)
            {
                Position = position;
                Normal = normal;
            }

            public override String ToString()
            {
                return "(" + Position + "),(" + Normal + ")";
            }

            //Vertex Information
            public const int SizeInBytes = 6;
            public static VertexDeclaration Declaration = new VertexDeclaration(
                new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
                );
            public VertexDeclaration VertexDeclaration
            {
                get { return Declaration; }
            }
        }
    }
}
