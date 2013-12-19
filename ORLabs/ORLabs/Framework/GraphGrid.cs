using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ZLibrary.Graphs;

namespace ORLabs.Framework
{
    public delegate void PointCheck(Vector2 p);
    public delegate void GGSRemoval(IGraphGridSelectable o);
    public interface IGraphGridSelectable
    {
        event GGSRemoval OnRemoval;
        void checkSelection(Vector2 p);
        void onHover(Vector2 p);
    }

    public class GraphGrid
    {
        #region Static Effect Information
        private static Effect fxGraph;
        private static EffectPass fxpCurrent;
        private static Texture2D texture;
        public const int TextureSize = 64;
        protected Effect FXGraph { get { return fxGraph; } }
        protected EffectPass FXPCurrent { get { return fxpCurrent; } }
        protected Texture2D GridCellTexture { get { return texture; } }

        public static void createTexture(GraphicsDevice g)
        {
            Color
                cIn = Color.White,
                cLine = new Color(8, 8, 8, 255),
                cCorner = new Color(40, 0, 0, 255);

            texture = new Texture2D(g, TextureSize, TextureSize);
            Color[] c = new Color[TextureSize * TextureSize];
            #region Make Color Data
            int x = 0, y = 0;
            c[TextureSize * 0 + 0] = cCorner;
            c[TextureSize * (TextureSize - 1) + 0] = cCorner;
            c[TextureSize * 0 + (TextureSize - 1)] = cCorner;
            c[TextureSize * (TextureSize - 1) + (TextureSize - 1)] = cCorner;
            for (int i = 1; i < (TextureSize - 1); i++)
            {
                c[TextureSize * i + 0] = cLine;
                c[TextureSize * i + (TextureSize - 1)] = cLine;
                c[TextureSize * 0 + i] = cLine;
                c[TextureSize * (TextureSize - 1) + i] = cLine;
            }
            for (x = 1; x < (TextureSize - 1); x++)
            {
                for (y = 1; y < (TextureSize - 1); y++)
                {
                    c[TextureSize * y + x] = cIn;
                }
            }
            #endregion
            texture.SetData<Color>(c);

            g.ResourceDestroyed += remakeTexture;
        }
        private static void remakeTexture(object sender, ResourceDestroyedEventArgs e)
        {
            //Color
            //       cIn = Color.White,
            //       cLine = new Color(8, 8, 8, 255),
            //       cCorner = new Color(40, 0, 0, 255);

            //Color[] c = new Color[TextureSize * TextureSize];
            //#region Make Color Data
            //int x = 0, y = 0;
            //c[TextureSize * 0 + 0] = cCorner;
            //c[TextureSize * (TextureSize - 1) + 0] = cCorner;
            //c[TextureSize * 0 + (TextureSize - 1)] = cCorner;
            //c[TextureSize * (TextureSize - 1) + (TextureSize - 1)] = cCorner;
            //for (int i = 1; i < (TextureSize - 1); i++)
            //{
            //    c[TextureSize * i + 0] = cLine;
            //    c[TextureSize * i + (TextureSize - 1)] = cLine;
            //    c[TextureSize * 0 + i] = cLine;
            //    c[TextureSize * (TextureSize - 1) + i] = cLine;
            //}
            //for (x = 1; x < (TextureSize - 1); x++)
            //{
            //    for (y = 1; y < (TextureSize - 1); y++)
            //    {
            //        c[TextureSize * y + x] = cIn;
            //    }
            //}
            //#endregion
            //texture.SetData<Color>(c);
        }
        public static void loadEffect(string file, ContentManager content)
        {
            fxGraph = content.Load<Effect>(file);
            fxGraph.CurrentTechnique = fxGraph.Techniques[0];
            fxpCurrent = fxGraph.CurrentTechnique.Passes[0];
        }
        public static void setEffectTechnique(int p)
        {
            fxGraph.CurrentTechnique = fxGraph.Techniques[p];
        }
        public static void setEffectTechnique(string p)
        {
            fxGraph.CurrentTechnique = fxGraph.Techniques[p];
        }
        public static void setEffectPass(int p)
        {
            fxpCurrent = fxGraph.CurrentTechnique.Passes[p];
        }
        public static void setEffectPass(string p)
        {
            fxpCurrent = fxGraph.CurrentTechnique.Passes[p];
        }
        #endregion

        //Dimension Information In Cells
        public Vector3 LowerLeftStart;
        //Dimension Information In Measuring Units
        public Vector2 GridSize { get; private set; }

        // Events
        event PointCheck OnSelectionCheck;
        event PointCheck OnHover;

        public GraphGrid(GraphicsDevice g, Vector2 lls, Vector2 s)
        {
            LowerLeftStart = new Vector3(lls.X, lls.Y, 0f);
            GridSize = s;
            buildBuffers(g);
            g.ResourceDestroyed += onResourceDestroy;
        }
        public GraphGrid(GraphicsDevice g, BuildInfo bi) : this(g, bi.LowerLeftStart, bi.Size) { }

        #region Graphics
        //Drawing Data
        protected IndexBuffer iBuffer;
        protected VertexBuffer vBuffer;

        public void buildBuffers(GraphicsDevice g)
        {
            if (iBuffer != null && !iBuffer.IsDisposed) { iBuffer.Dispose(); }
            iBuffer = new IndexBuffer(g, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
            if (vBuffer != null && !vBuffer.IsDisposed) { vBuffer.Dispose(); }
            vBuffer = new VertexBuffer(g, VertexPositionColorTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);

            refreshBuffers();
        }
        public void refreshBuffers()
        {
            iBuffer.SetData<short>(new short[6] { 0, 1, 2, 2, 1, 3 });
            vBuffer.SetData<VertexPositionColorTexture>(new VertexPositionColorTexture[4]
            {
                new VertexPositionColorTexture(LowerLeftStart + new Vector3(0f, 0f, -0.5f), Color.White, Vector2.Zero),
                new VertexPositionColorTexture(LowerLeftStart + new Vector3(GridSize.X, 0f, -0.5f), Color.White, Vector2.UnitX),
                new VertexPositionColorTexture(LowerLeftStart + new Vector3(0f, GridSize.Y, -0.5f), Color.White, Vector2.UnitY),
                new VertexPositionColorTexture(LowerLeftStart + new Vector3(GridSize, -0.5f), Color.White, Vector2.One)
            });
        }
        public virtual void draw(GraphicsDevice g, Matrix mWVP, float zoom)
        {
            g.SetVertexBuffer(vBuffer);
            g.Indices = iBuffer;

            g.Textures[0] = texture;
            g.SamplerStates[0] = SamplerState.LinearWrap;
            fxGraph.Parameters["WVP"].SetValue(mWVP);
            fxGraph.Parameters["ZoomLevel"].SetValue(zoom);
            fxpCurrent.Apply();
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        }

        private void onResourceDestroy(object sender, ResourceDestroyedEventArgs e)
        {
            //if (vBuffer != null && vBuffer.Name.Equals(e.Name))
            //{
            //    vBuffer.SetData<VertexPositionColorTexture>(new VertexPositionColorTexture[4]
            //    {
            //        new VertexPositionColorTexture(new Vector3(0f, 0f, -0.5f), Color.White, Vector2.Zero),
            //        new VertexPositionColorTexture(new Vector3(GridSize.X, 0f, -0.5f), Color.White, Vector2.UnitX),
            //        new VertexPositionColorTexture(new Vector3(0f, GridSize.Y, -0.5f), Color.White, Vector2.UnitY),
            //        new VertexPositionColorTexture(new Vector3(GridSize, -0.5f), Color.White, Vector2.One)
            //    });
            //}
            //else if (iBuffer != null && iBuffer.Name.Equals(e.Name))
            //{
            //    iBuffer.SetData<short>(new short[6] { 0, 1, 2, 2, 1, 3 });
            //}
        }
        #endregion

        public void checkSelection(Vector2 p)
        {
            if (OnSelectionCheck != null) { OnSelectionCheck(p); }
        }
        public void onHover(Vector2 p)
        {
            if (OnHover != null) { OnHover(p); }
        }

        public void addCircle(IGraphGridSelectable o, float x, float y, float r)
        {
            OnHover += o.onHover;
            OnSelectionCheck += o.checkSelection;
        }
        public void addLine(IGraphGridSelectable o, Vector2 p1, Vector2 p2, float w = 1)
        {
            OnHover += o.onHover;
            OnSelectionCheck += o.checkSelection;
        }

        public void remove(IGraphGridSelectable o)
        {
            OnHover -= o.onHover;
            OnSelectionCheck -= o.checkSelection;
        }

        public struct BuildInfo
        {
            public Vector2 LowerLeftStart;
            public Vector2 Size;

            public BuildInfo(Vector2 lls, Vector2 s)
            {
                LowerLeftStart = lls;
                Size = s;
            }

            public static BuildInfo encapsulate(ORGraph g)
            {
                Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
                Vector2 max = new Vector2(float.MinValue, float.MinValue);
                foreach (ORGraph.Node node in g)
                {
                    if (node.Data.Position.X < min.X) { min.X = node.Data.Position.X; }
                    if (node.Data.Position.X > max.X) { max.X = node.Data.Position.X + 1; }
                    if (node.Data.Position.Y < min.Y) { min.Y = node.Data.Position.Y; }
                    if (node.Data.Position.Y > max.Y) { max.Y = node.Data.Position.Y + 1; }
                }
                int cc = g.NodeCount > 0 ? (int)Math.Log(g.NodeCount) + 1 : 1;
                return new BuildInfo(min - Vector2.One, (max - min) + Vector2.One * 2);
            }
        }
    }

    public class GraphGridTextured : GraphGrid
    {
        protected Texture2D backTexture;
        protected bool fzoom;
        public bool FadeZoomEnabled { get { return fzoom; } set { fzoom = value; } }

        public GraphGridTextured(GraphicsDevice g, Vector2 lls, Vector2 s)
            : base(g, lls, s)
        {
            fzoom = true;
            backTexture = GridCellTexture;
        }
        public GraphGridTextured(GraphicsDevice g, GraphGrid.BuildInfo bi)
            : this(g, bi.LowerLeftStart, bi.Size)
        {
        }

        public void setTexture(Texture2D t)
        {
            fzoom = false;
            backTexture = t;
        }

        public override void draw(GraphicsDevice g, Matrix mWVP, float zoom)
        {
            g.SetVertexBuffer(vBuffer);
            g.Indices = iBuffer;

            g.Textures[0] = backTexture;
            FXGraph.Parameters["WVP"].SetValue(mWVP);
            if (fzoom)
            {
                g.SamplerStates[0] = SamplerState.LinearWrap;
                FXGraph.Parameters["ZoomLevel"].SetValue(zoom);
            }
            else
            {
                g.SamplerStates[0] = SamplerState.LinearClamp;
                FXGraph.Parameters["ZoomLevel"].SetValue(1);
            }
            FXPCurrent.Apply();
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        }
    }
}
