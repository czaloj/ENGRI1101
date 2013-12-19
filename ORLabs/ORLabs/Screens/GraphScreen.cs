using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using XNA2D.Cameras;
using XNA3D.Cameras;
using BlisterUI;
using BlisterUI.Input;
using ORLabs.Framework;
using ORLabs.Graphics.Graphs;

namespace ORLabs.Screens
{
    public interface IORGraphHolder
    {
        ORGraph Graph { get; }
    }

    public abstract class GraphScreen : GameScreen<Main>, IORGraphHolder
    {
        public static GraphIO gio = new GraphIO();
        private static System.Threading.Thread tGIO = null;
        public static GraphScreen GIOScreen { get; protected set; }
        public static void runGIO(GraphIO.IOTaskType task, GraphScreen screen)
        {
            if (screen == null) return;
            GIOScreen = screen;
            if (tGIO == null || !tGIO.IsAlive)
            {
                tGIO = new System.Threading.Thread(() =>
                {
                    gio.ShowDialog();
                });
                tGIO.TrySetApartmentState(System.Threading.ApartmentState.STA);
                tGIO.Start();
            }
            switch (task)
            {
                case GraphIO.IOTaskType.Load:
                    gio.setLoading();
                    break;
                case GraphIO.IOTaskType.Save:
                    gio.setSaving();
                    break;
            }
        }
        public static void terminateGIO()
        {
            if (tGIO != null && tGIO.IsAlive) tGIO.Abort();
        }

        public static readonly Vector2 InitialGraphSize = new Vector2(1000f, 1000f);
        public static readonly float MaxGraphSide = MathHelper.Max(InitialGraphSize.X, InitialGraphSize.Y);

        public override int Next
        {
            get { return game.ScrStart.Index; }
            protected set { }
        }
        public override int Previous
        {
            get { return game.ScrStart.Index; }
            protected set { }
        }

        private readonly object graphLock = new object();
        protected ORGraph graph;
        public ORGraph Graph { get { return graph; } }
        protected GraphRenderer gr;
        protected Camera2D camera;

        //Camera Input Helpers
        protected Vector2 mouseAnchor;
        protected Vector2 camAnchorLoc;
        protected bool IsCamRegistered { get; private set; }
        protected bool IsMovingCam { get; private set; }

        public GraphScreen()
            : base()
        {

        }

        protected void buildGraph()
        {
            graph = new ORGraph(10, 10);
            graph.buildGrid(game.GraphicsDevice, new GraphGrid.BuildInfo(new Vector2(0, 0), new Vector2(1000, 1000)));
        }
        protected void buildRenderer()
        {
            //Make The Camera
            camera = new Camera2D(InitialGraphSize / 2, new Vector2(game.GraphicsDevice.Viewport.AspectRatio, 1) * MaxGraphSide, 0f);
            game.Window.ClientSizeChanged += (o, s) =>
            {
                camera.setViewSize(new Vector2(game.GraphicsDevice.Viewport.AspectRatio, 1) * MaxGraphSide);
            };

            //Make The Graph Renderer
            gr = new GraphRenderer(game.GraphicsDevice, graph);
#if HIDEF
            gr.loadEffect(@"Effects\Graph_HiDef", "Default", game.Content);
#else
            gr.loadEffect(@"Effects\Graph_Reach", "Default", game.Content);
#endif
            refreshGraphBatch();
        }
        protected void drawGraph(Matrix mView, Matrix mProjection, Vector2 camVS)
        {
            gr.draw(game.GraphicsDevice, mView, mProjection, camVS, new Vector2(game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height));
            
        }

        protected void destroyGraph()
        {
            ORGraph.clearGraph(graph, 0, 0);
            gr.rebuild(game.GraphicsDevice);
        }
        protected void setupNewGraph()
        {
            gr.setGraph(graph, game.GraphicsDevice);
            graph.buildGrid(game.GraphicsDevice);
            gr.rebuild(game.GraphicsDevice);
        }

        public void setGraph(ORGraph g)
        {
            lock (graphLock)
            {
                if (graph != null) { destroyGraph(); }
                graph = g;
                setupNewGraph();
            }
        }
        protected void updateGraph(float dt)
        {
            lock (graphLock)
            {
                gr.update(dt);
            }
        }
        protected void refreshGraphBatch()
        {
            gr.rebuild(game.GraphicsDevice);
        }

        public void registerCameraInput()
        {
            if (!IsCamRegistered)
            {
                IsCamRegistered = true;
                IsMovingCam = false;
                MouseEventDispatcher.OnMouseMotion += onMouseMotionCam;
                MouseEventDispatcher.OnMousePress += onMousePressCam;
                MouseEventDispatcher.OnMouseRelease += onMouseReleaseCam;
                MouseEventDispatcher.OnMouseScroll += onMouseWheelCam;
            }
        }
        public void unregisterCameraInput()
        {
            if (IsCamRegistered)
            {
                MouseEventDispatcher.OnMouseMotion -= onMouseMotionCam;
                MouseEventDispatcher.OnMousePress -= onMousePressCam;
                MouseEventDispatcher.OnMouseRelease -= onMouseReleaseCam;
                MouseEventDispatcher.OnMouseScroll -= onMouseWheelCam;
                IsMovingCam = false;
                IsCamRegistered = false;
            }
        }

        protected Vector2 project(Vector2 screenPos)
        {
            Vector2 norm = screenPos / new Vector2(game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
            norm.Y = 1 - norm.Y;
            norm *= 2f;
            norm -= Vector2.One;
            return Vector2.Transform(norm, Matrix.Invert(camera.View * camera.Projection));
        }

        // Mouse Event Handlers For The Camera
        protected virtual void onMouseMotionCam(Vector2 loc, Vector2 move)
        {
            Vector2 w = project(loc);
            graph.checkHover(w);
            if (IsMovingCam)
            {
                Vector2 p = project(mouseAnchor) - project(loc);
                Vector4 dis = Vector4.Transform(new Vector4(loc - mouseAnchor, 0, 0), Matrix.Invert(camera.View));
                camera.setLocation(camAnchorLoc + p);
            }
        }
        protected virtual void onMousePressCam(Vector2 loc, MOUSE_BUTTON button)
        {
            Vector2 w = project(loc);
            if (button == MOUSE_BUTTON.LEFT_BUTTON)
            {
                IsMovingCam = true;
                mouseAnchor = loc;
                camAnchorLoc = new Vector2(camera.Location.X, camera.Location.Y);
            }
        }
        protected virtual void onMouseReleaseCam(Vector2 loc, MOUSE_BUTTON button)
        {
            if (IsMovingCam)
            {
                if (button == MOUSE_BUTTON.LEFT_BUTTON)
                {
                    IsMovingCam = false;
                }
            }
        }
        protected virtual void onMouseWheelCam(int loc, int dis)
        {
            if (dis < 0) //Zoom Out
            {
                camera.zoom(Vector2.One / 1.3f);
            }
            else if (dis > 0) //Zoom In
            {
                camera.zoom(Vector2.One * 1.3f);
            }
        }
    }
}
