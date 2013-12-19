using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ZLibrary.Graphs;
using BlisterUI;
using BlisterUI.Input;
using ORLabs.Graphics;
using ORLabs.Graphics.Widgets;
using ORLabs.Framework;

namespace ORLabs.Screens
{
    public class CreationScreen : GraphScreen
    {
        const float ListMoveDuration = 0.2f;

        public bool[] KeyFlags = new bool[KeyFlagCount];
        public const int KeyFlagCount = 3;
        public const int KF_Addition = 0;
        public const int KF_AdditionEdge = 1;
        public const int KF_Removal = 2;

        ORGraph.Node startNode;

        SpriteFont font;
        const int MCC = 150;

        Frame dataFrame;
        SimpleText nData, eData;

        MTransVisibleList neData;

        public override void build()
        {
            font = game.Content.Load<SpriteFont>(@"Fonts\Arial12");

            #region Node/Edge Data
            nData = new SimpleText(font);
            nData.Color = Color.Teal;
            nData.setText(getNodeInfo(null));

            eData = new SimpleText(font);
            eData.Color = Color.Teal;
            eData.setText(getEdgeInfo(null));

            dataFrame = new Frame();
            Frame.create<Frame>(ref dataFrame, WidgetFrame.Identity, new Vector2(210, 200), new Color(12, 12, 12, 255));

            neData = new MTransVisibleList(
                new MTVLBinding(dataFrame, WidgetFrame.Identity),
                new MTVLBinding(nData, new WidgetFrame(new Vector2(10, 10), 0)),
                new MTVLBinding(eData, new WidgetFrame(new Vector2(10, 74), 0))
                );
            neData.World = new WidgetFrame(new Vector2(game.GraphicsDevice.Viewport.Width - dataFrame.Size.X, 0), 0.5f);
            #endregion

            buildGraph();
            buildRenderer();

            ORGraph.Node.OnNewHoverNode += updateInfo;
            ORGraph.Edge.OnNewHoverEdge += updateInfo;

            game.Window.ClientSizeChanged += (o, s) =>
            {
                neData.World = new WidgetFrame(new Vector2(game.GraphicsDevice.Viewport.Width - dataFrame.Size.X, 0), 0.5f);
            };
        }
        public override void destroy(GameTime gameTime)
        {
            ORGraph.Node.OnNewHoverNode -= updateInfo;
            ORGraph.Edge.OnNewHoverEdge -= updateInfo;
        }

        public override void onEntry(GameTime gameTime)
        {
            GIOScreen = this;
            neData.setVisible(true);

            unregisterCameraInput();
            MouseEventDispatcher.OnMouseMotion += onMouseMotion;
            MouseEventDispatcher.OnMousePress += onMousePress;
            MouseEventDispatcher.OnMouseRelease += onMouseRelease;
            MouseEventDispatcher.OnMouseScroll += onMouseWheel;
            KeyboardEventDispatcher.OnKeyPressed += onKeyPress;
            KeyboardEventDispatcher.OnKeyReleased += onKeyRelease;
            KeyboardEventDispatcher.OnKeyPressed += ORGraph.Edge.OnKeyPress;
            KeyboardEventDispatcher.OnKeyReleased += ORGraph.Edge.OnKeyRelease;
        }
        public override void onExit(GameTime gameTime)
        {
            neData.setVisible(false);

            unregisterCameraInput();
            MouseEventDispatcher.OnMouseMotion -= onMouseMotion;
            MouseEventDispatcher.OnMousePress -= onMousePress;
            MouseEventDispatcher.OnMouseRelease -= onMouseRelease;
            MouseEventDispatcher.OnMouseScroll -= onMouseWheel;
            KeyboardEventDispatcher.OnKeyPressed -= onKeyPress;
            KeyboardEventDispatcher.OnKeyReleased -= onKeyRelease;
            KeyboardEventDispatcher.OnKeyPressed -= ORGraph.Edge.OnKeyPress;
            KeyboardEventDispatcher.OnKeyReleased -= ORGraph.Edge.OnKeyRelease;
        }

        public override void update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            updateGraph(dt);
        }
        public override void draw(GameTime gameTime)
        {
            game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil,
                new Color(216, 216, 216, 255), 1, 0
                );
            drawGraph(camera.View, camera.Projection, camera.ViewSize);
            game.SpriteBatch.Begin();
            dataFrame.draw(game.SpriteBatch);
            nData.draw(game.SpriteBatch);
            eData.draw(game.SpriteBatch);
            game.SpriteBatch.End();
            game.GraphicsDevice.SetVertexBuffers(null);
        }

        public void onMouseMotion(Vector2 loc, Vector2 move)
        {
            Vector2 w = project(loc);
            graph.checkHover(w);
            if (mNode != null && movingNode)
            {
                mNode.Data.Position.X = w.X;
                mNode.Data.Position.Y = w.Y;
                gr.rebuild(game.GraphicsDevice);
            }
        }
        public void onMousePress(Vector2 loc, MOUSE_BUTTON button)
        {
            if (IsCamRegistered) { return; }

            Vector2 w = project(loc);
            if (button == MOUSE_BUTTON.LEFT_BUTTON)
            {
                if (KeyFlags[KF_Addition])
                {
                    graph.addNode(new ORGraph.Node(new NodeData(project(loc), 4f, Color.Red, 0x00)));
                    gr.rebuild(game.GraphicsDevice);
                    graph.resizeFit();
                }
                else if (KeyFlags[KF_AdditionEdge])
                {
                    if (startNode != null)
                    {
                        ORGraph.Node n = ORGraph.Node.HoveredNode;
                        if (n != null && startNode != n)
                        {
                            graph.addEdge(new ORGraph.Edge(new EdgeData(
                                (int)((startNode.Data.Position2 - n.Data.Position2).Length()), Color.BlueViolet, 1f)),
                                startNode.Index, n.Index, false
                                );
                            gr.rebuild(game.GraphicsDevice);
                            graph.resizeFit();
                        }
                        startNode = null;
                    }
                    else
                    {
                        startNode = ORGraph.Node.HoveredNode;
                    }
                }
                else if (movingNode)
                {
                    mNode = ORGraph.Node.HoveredNode;
                }
                else if (KeyFlags[KF_Removal])
                {
                    ORGraph.Node n = ORGraph.Node.HoveredNode;
                    if (n != null)
                    {
                        graph.removeNode(n);
                        gr.rebuild(game.GraphicsDevice);
                        graph.resizeFit();
                    }
                }
                else { graph.checkSelection(w); }
            }
            else if (button == MOUSE_BUTTON.RIGHT_BUTTON)
            {
                if (KeyFlags[KF_AdditionEdge]) { startNode = null; }
            }
        }
        public void onMouseRelease(Vector2 loc, MOUSE_BUTTON button)
        {
            mNode = null;
        }
        public void onMouseWheel(int loc, int dis)
        {
            if (IsCamRegistered) { return; }
            ORGraph.Edge.cycleHovers(ORGraph.Node.HoveredNode);
        }
        protected override void onMouseWheelCam(int loc, int dis)
        {
            base.onMouseWheelCam(loc, dis);
            var ms = Mouse.GetState();
            graph.checkHover(project(new Vector2(ms.X, ms.Y)));
            gr.rebuild(game.GraphicsDevice);
        }

        public void onKeyPress(object s, KeyEventArgs args)
        {
            var ml = KeyboardEventDispatcher.getCurrentModifiers();
            if (ml.IsControlPressed) { registerCameraInput(); }
            switch (args.KeyCode)
            {
                case Keys.F2: runGIO(GraphIO.IOTaskType.Load, this); break;
                case Keys.F3: runGIO(GraphIO.IOTaskType.Save, this); break;

                case Keys.Q: KeyFlags[KF_Addition] |= true; break;
                case Keys.E: KeyFlags[KF_AdditionEdge] |= true; break;
                case Keys.Z: KeyFlags[KF_Removal] |= true; break;

                case Keys.R:
                    if (gio.Visible) gio.Hide();

                    graph.clear(nCount, nCount * eAdd);
                    buildThread = new Thread(makeRandGraph);
                    buildThread.IsBackground = true;
                    buildThread.TrySetApartmentState(ApartmentState.STA);
                    buildThread.Priority = ThreadPriority.Normal;
                    buildThread.Start();
                    
                    break;


                case Keys.P:
                    GIOScreen = null;
                    State = ScreenState.ChangePrevious;
                    break;
                case Keys.C:
                    graph.clear(10, 10);
                    gr.rebuild(game.GraphicsDevice);
                    break;

                case Keys.M:
                    movingNode = true;
                    break;

                default: break;
            }
        }
        public void onKeyRelease(object s, KeyEventArgs args)
        {
            var ml = KeyboardEventDispatcher.getCurrentModifiers();
            if (!ml.IsControlPressed) { unregisterCameraInput(); }
            switch (args.KeyCode)
            {
                case Keys.Q: KeyFlags[KF_Addition] &= false; break;
                case Keys.E: KeyFlags[KF_AdditionEdge] &= false; startNode = null; break;
                case Keys.R: KeyFlags[KF_Removal] &= false; break;
                case Keys.M: movingNode = false; mNode = null; break;
                default: break;
            }
        }

        void deleteGraph()
        {
            ORGraph.clearGraph(graph, 0, 0);
        }

        public void updateInfo(ORGraph.Node n)
        {
            nData.setText(getNodeInfo(n));
        }
        public void updateInfo(ORGraph.Edge e)
        {
            eData.setText(getEdgeInfo(e));
        }
        public string getNodeInfo(ORGraph.Node n)
        {
            if (n == null) { return "==No Node Selected=="; }
            string s = string.Format(
@"==Node==
Index: {0,4}
Degree: {1,4}
Distance: {2}",
                n.Index, n.Degree, n.Distance
                );
            return s;
        }
        public string getEdgeInfo(ORGraph.Edge e)
        {
            if (e == null) { return "==No Edge Selected=="; }
            string s = string.Format(
@"==Edge==
Index: {0,4}
Weight: {1,4}",
                e.Index, e.Data.Weight, getNodeInfo(e.Start as ORGraph.Node), getNodeInfo(e.End as ORGraph.Node)
                );
            return s;
        }

        #region Random Graph Building
        Thread buildThread;
        const int barWidth = 500, barHeight = 10;
        static readonly Vector2 barSize = new Vector2(barWidth, barHeight);
        int gSize = 1000; int nCount = 3000;
        int eAdd = 6, echeck = 3000;

        public void setRandBuildInfo(int gs, RandGraph rg)
        {
            gSize = gs;
            nCount = rg.NodeCount;
            eAdd = rg.EdgeAddPerNode;
            echeck = rg.EdgeCheckCount;
        }
        void makeRandGraph()
        {
            MouseEventDispatcher.OnMouseMotion -= onMouseMotion;
            MouseEventDispatcher.OnMouseMotion -= onMouseMotionCam;
            if (echeck > nCount) { echeck = nCount; }

            graph.clear(nCount, eAdd * nCount);
            Random r = new Random();

            //Add All Nodes
            for (int i = 0; i < nCount; i++)
            {
                Vector2 pos = new Vector2(r.Next(1, gSize), r.Next(1, gSize));
                Color col = new Color(r.Next(100, 256), r.Next(100, 256), r.Next(100, 256));
                graph.addNode(new ORGraph.Node(new NodeData(pos, 4f, col, 0x00)));
            }
            foreach (ORGraph.Node n in graph.Nodes) { n.Distance = double.PositiveInfinity; }

            //Add All Edges
            MinHeap<NodeDist> nodeDists = new MinHeap<NodeDist>(nCount - 1);
            for (int ni = 0; ni < nCount; ni++)
            {
                nodeDists = new MinHeap<NodeDist>(nCount - 1);
                for (int ei = 0; ei < echeck; ei++)
                {
                    int oi = r.Next(0, nCount);
                    if (ni == oi) { continue; }
                    else
                    {
                        double d = (graph.Nodes[ni].Data.Position - graph.Nodes[oi].Data.Position).Length();
                        nodeDists.insert(new NodeDist() { Index = oi, Distance = d });
                    }
                }
                for (int oi = 0; oi < eAdd; oi++)
                {
                    if (nodeDists.Count == 0) { break; }
                    NodeDist nd = nodeDists.extract();
                    if (graph.edgeCount(ni, nd.Index) == 0)
                    {
                        graph.addEdge(new ORGraph.Edge(new EdgeData(nd.Dist, Color.Gray, 1f)), ni, nd.Index);
                    }
                    else
                    {
                        oi--;
                    }
                }
            }
            graph.resizeFit();
            graph.buildGrid(game.GraphicsDevice, GraphGrid.BuildInfo.encapsulate(graph));
            setupNewGraph();

            MouseEventDispatcher.OnMouseMotion += onMouseMotion;
            MouseEventDispatcher.OnMouseMotion += onMouseMotionCam;
        }

        class ProgBar
        {
            Rectangle rect;
            Color color;
            VertexPositionColor[] verts;
            short[] ind;

            public ProgBar(Rectangle r, Color c)
            {
                color = c;
                setRect(r);
                ind = new short[6] { 0, 1, 2, 2, 1, 3 };
            }

            public void setRect(Rectangle r)
            {
                rect = r;
                verts = new VertexPositionColor[4]
                {
                    new VertexPositionColor(new Vector3(rect.Left, rect.Top, 0.5f), color),
                    new VertexPositionColor(new Vector3(rect.Right, rect.Top, 0.5f), color),
                    new VertexPositionColor(new Vector3(rect.Left, rect.Bottom, 0.5f), color),
                    new VertexPositionColor(new Vector3(rect.Right, rect.Bottom, 0.5f), color)
                };
            }
            public void setWidth(int w)
            {
                rect.Width = w;
                setRect(rect);
            }
            public void setHeight(int h)
            {
                rect.Height = h;
                setRect(rect);
            }

            public void draw(GraphicsDevice g)
            {
                g.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, verts, 0, 4, ind, 0, 2);
            }
        }
        public struct RandGraph
        {
            public int NodeCount;
            public int EdgeCheckCount;
            public int EdgeAddPerNode;

            public RandGraph(int nc, int ea, int ec)
            {
                NodeCount = nc;
                EdgeCheckCount = ec;
                EdgeAddPerNode = ea;
            }
        }
        public struct GridOptions
        {
            int GSX, GSY, GCW, GCH;
            public GridOptions(int gsx, int gsy, int gcw, int gch)
            {
                GSX = gsx;
                GSY = gsy;
                GCW = gcw;
                GCH = gch;
            }
        }
        struct NodeDist : IComparable<NodeDist>
        {
            public int Index;
            public double Distance;

            public int Dist
            {
                get
                {
                    return (int)Distance;
                }
            }

            public int CompareTo(NodeDist other)
            {
                return Distance.CompareTo(other.Distance);
            }
        }
        #endregion

        #region For Graph Movement
        bool movingNode = false;
        ORGraph.Node mNode;
        #endregion
    }
}
