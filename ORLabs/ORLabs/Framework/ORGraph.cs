using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using ORLabs.Graphics.Graphs;
using ZLibrary.Graphs;
using ZLibrary.Algorithms;
using BlisterUI.Input;
using ORLabs.Graphics;

namespace ORLabs.Framework {
    public delegate void NewSelection<T>(T obj);

    public class ORGraph : Graph<ORGraph.Edge, ORGraph.Node> {
        protected bool isGMap;
        protected GeoCodeRequest gcReq;
        protected ImageRequest imReq;
        protected GraphGrid.BuildInfo gbi;
        protected GraphGrid grid;
        public GraphGrid Grid { get { return grid; } }

        public ORGraph()
            : base() { setup(); }
        public ORGraph(int nCapacity, int eCapacity)
            : base(nCapacity, eCapacity) { setup(); }

        private void setup() {
            OnNodeDeletion += (sender, n) => { n.fireRemovalEvent(); };
            OnNodeCreation += (sender, n) => {
                if(this.grid != null) {
                    this.grid.addCircle(n, n.Data.Position.X, n.Data.Position.Y, n.Data.Radius);
                }
            };
        }

        public override void readMisc(TokenStream s) {
            MiscData miscData = new MiscData();
            miscData.read(s);
            gbi = miscData.GraphBI;
            gcReq = miscData.GR;
            imReq = miscData.IR;
            isGMap = miscData.UseMap;
        }
        public override void writeMisc(TokenStream s) {
            //miscData.write(s);
        }

        public void buildGrid(GraphicsDevice g, GraphGrid.BuildInfo bi) {
            gbi = bi;
            if(gbi.Size.X <= 0 || gbi.Size.Y <= 0) {
                gbi = GraphGrid.BuildInfo.encapsulate(this);
            }
            if(grid != null) { removeFromGrid(grid); }
            grid = new GraphGrid(g, gbi);
            addToGrid(grid);
        }
        public void addToGrid(GraphGrid grid) {
            for(int i = 0; i < NodeCount; i++) {
                grid.addCircle(
                    Nodes[i],
                    Nodes[i].Data.Position.X,
                    Nodes[i].Data.Position.Y,
                    Nodes[i].Data.Radius
                    );
            }
        }
        public void removeFromGrid(GraphGrid grid) {
            for(int i = 0; i < NodeCount; i++) {
                grid.remove(Nodes[i]);
            }
        }
        public void buildGrid(GraphicsDevice g) {
            if(grid != null) { removeFromGrid(grid); }
            grid = null;
            if(isGMap) {
                GeoCodeResults res;
                bool available = GeoCodeResults.fromRequest(gcReq, out res);
                if(available) {
                    imReq.Latitude = res.Latitude;
                    imReq.Longitude = res.Longitude;
                    Texture2D gMap = GoogleMap.getMapTexture(g, imReq);
                    if(gMap != null) {
                        grid = new GraphGridTextured(g, gbi);
                        ((GraphGridTextured)grid).setTexture(gMap);
                    }
                }
            }
            if(grid == null) {
                if(gbi.Size.X <= 0 || gbi.Size.Y <= 0) {
                    gbi = GraphGrid.BuildInfo.encapsulate(this);
                }
                grid = new GraphGrid(g, gbi);
            }
            addToGrid(grid);
        }

        public void checkHover(Vector2 p) {
            grid.onHover(p);
        }
        public void checkSelection(Vector2 p) {
            grid.checkSelection(p);
        }

        public static void clearGraph(ORGraph g, int nnc, int nec) {
            g.clear(nnc, nec);
            Node.setHoverNode(null);
            Node.SelectedNodes.Clear();
            Edge.setHoverEdge(null);
        }

        #region Nodes And Edges
        public class Node : Node<Edge>, IGraphGridSelectable {
            #region Static
            public static event NewSelection<Node> OnNewHoverNode;

            public static Node HoveredNode = null;
            public static LinkedList<Node> SelectedNodes = new LinkedList<Node>();
            public static readonly Color SelectionColor = Color.Orange;
            public static readonly Color HoverColor = Color.Purple;
            public static Color SecondaryColor = Color.Transparent;

            public static void setHoverNode(Node n) {
                if(HoveredNode != null) {
                    if(n != null && n.Index == HoveredNode.Index) { return; }
                    revertColor(HoveredNode);
                }
                HoveredNode = n;
                if(HoveredNode != null) { HoveredNode.setPColor(HoverColor); }
                if(OnNewHoverNode != null) { OnNewHoverNode(HoveredNode); }
            }
            public static void revertColor(Node n) {
                if(n.selected) { n.setPColor(SelectionColor); }
                else { n.setColorPriority(false); }
            }
            #endregion

            public Node() : base() { data = NodeData.None; }
            public Node(NodeData d)
                : base() {
                data = d;
            }

            protected NodeData data;
            public NodeData Data { get { return data; } }

            public int LIndex;
            protected NodeList nl;
            public void setListIndex(int i, NodeList l) { LIndex = i; nl = l; }

            public byte Flags;
            public double Distance;

            private bool selected;
            public bool IsSelected { get { return selected; } }

            public event GGSRemoval OnRemoval;

            public void setPColor(Color c) {
                data.ColorP.setPriority(c);
                nl.setColor(LIndex, data.Color, SecondaryColor);
            }
            public void setBColor(Color c) {
                data.ColorP.setBase(c);
                nl.setColor(LIndex, data.Color, SecondaryColor);
            }
            public void setColorPriority(bool b) {
                if(b) { data.ColorP.prioritize(); }
                else { data.ColorP.unprioritize(); }
                nl.setColor(LIndex, data.Color, SecondaryColor);
            }

            public void fireRemovalEvent() {
                if(OnRemoval != null) {
                    OnRemoval(this);
                }
            }

            public bool pointInNode(Vector2 p) {
                return (p - Data.Position2).Length() <= data.Radius;
            }
            public void checkSelection(Vector2 p) {
                if(pointInNode(p)) {
                    if(!selected) {
                        selected = true;
                        if(HoveredNode == null || HoveredNode.Index != Index) {
                            setPColor(SelectionColor);
                        }
                        SelectedNodes.AddLast(this);
                    }
                    else if(selected) {
                        SelectedNodes.Remove(this);
                        selected = false;
                        revertColor(this);
                    }
                }
            }
            public void onHover(Vector2 p) {
                //Check For New Node First
                if(pointInNode(p)) {
                    setHoverNode(this);
                }
                else if(HoveredNode == this) {
                    setHoverNode(null);
                }
            }

            public override void read(TokenStream s) {
                data.read(s);
                Flags = data.Flags;
                Distance = data.Distance;
            }
            public override void write(TokenStream s) {
                data.Flags = Flags;
                data.Distance = Distance;
                data.write(s);
            }
        }
        public class Edge : Edge<Node> {
            #region Static
            public static event NewSelection<Edge> OnNewHoverEdge;

            public static Edge HoveredLine = null;
            public static int HoverCycleMod = 0;
            public const int HoverCycleMax = 1000;
            private static bool ShowHover = false;
            public static Keys ShowHoverKey = Keys.D1;
            public static readonly Color HoverColor = Color.Green;
            public static Color SecondaryColor = Color.Transparent;

            static Edge() {
                Node.OnNewHoverNode += cycleHovers;
            }

            public static void OnKeyPress(object sender, KeyEventArgs args) {
                if(args.KeyCode == ShowHoverKey) { ShowHover = true; HoverCycleMod--; cycleHovers(Node.HoveredNode); }
            }
            public static void OnKeyRelease(object sender, KeyEventArgs args) {
                if(args.KeyCode == ShowHoverKey) { ShowHover = false; setHoverEdge(null); }
            }

            public static void cycleHovers(Node n) {
                HoverCycleMod = (HoverCycleMod + 1) % HoverCycleMax;
                if(ShowHover) {
                    setHoverEdge(null);
                    if(n != null) {
                        int e = HoverCycleMod % Node.HoveredNode.Degree;
                        foreach(ORGraph.Edge edge in Node.HoveredNode) {
                            if(e == 0) { setHoverEdge(edge); break; }
                            e--;
                        }
                    }
                }
            }
            public static void setHoverEdge(Edge e) {
                if(HoveredLine != null) {
                    if(e != null && e.Index == HoveredLine.Index) { return; }
                    revertColor(HoveredLine);
                }
                HoveredLine = e;
                if(HoveredLine != null) { HoveredLine.setPColor(HoverColor); }
                if(OnNewHoverEdge != null) { OnNewHoverEdge(HoveredLine); }
            }
            public static void revertColor(Edge e) {
                e.setColorPriority(false);
            }
            #endregion

            public Edge() : base() { data = EdgeData.None; }
            public Edge(EdgeData d)
                : base() {
                data = d;
            }

            protected EdgeData data;
            public EdgeData Data { get { return data; } }


            public int LIndex;
            protected EdgeList nl;
            public void setListIndex(int i, EdgeList l) { LIndex = i; nl = l; }

            public byte Flags;

            public void setPColor(Color c) {
                data.ColorP.setPriority(c);
                nl.setColor(LIndex, data.Color, data.Color, SecondaryColor);
            }
            public void setBColor(Color c) {
                data.ColorP.setBase(c);
                nl.setColor(LIndex, data.Color, data.Color, SecondaryColor);
            }
            public void setColorPriority(bool b) {
                if(b) { data.ColorP.prioritize(); }
                else { data.ColorP.unprioritize(); }
                nl.setColor(LIndex, data.Color, data.Color, SecondaryColor);
            }

            public override void read(TokenStream s) {
                data.read(s);
                Flags = data.Flags;
            }
            public override void write(TokenStream s) {
                data.Flags = Flags;
                data.write(s);
            }
        }
        #endregion
    }

    #region Data
    /// <summary>
    /// ed[Weight|Width|Flags]
    /// edcolor[R|G|B|A]
    /// 
    /// <example>
    /// Red Edge With Weight 12 And Width 5
    /// ed[12|5|0]
    /// edcolor[255|0|0|255]
    /// </example>
    /// </summary>
    public class EdgeData {
        public static EdgeData None { get { return new EdgeData(0, Color.Black, 1); } }

        public int Weight;
        public float Width;
        public ColorPriority ColorP;
        public Color Color { get { return ColorP.Color; } set { ColorP.setPriority(value); } }
        public byte Flags;

        public EdgeData(int w, Color c, float width, byte f = 0x00) {
            Weight = w;
            Width = width;
            ColorP = new ColorPriority(c);
            Flags = f;
        }

        #region IGraphData Members
        static readonly Signature Signature1 = new Signature("ed", 3);
        static readonly Signature Signature2 = new Signature("edcolor", 4);
        public void read(TokenStream s) {
            Token t;
            if(!s.search(Signature1, out t)) { Token.throwMissingSignature(Signature1); }
            if(!t.getArg<int>(0, ref Weight)) { t.throwBadArg<int>(0); }
            if(!t.getArg<float>(1, ref Width)) { t.throwBadArg<float>(1); }
            if(!t.getArg<byte>(2, ref Flags)) { t.throwBadArg<byte>(2); }

            if(!s.search(Signature2, out t)) { Token.throwMissingSignature(Signature2); }
            Color c = Color.Transparent;
            if(!t.getArgColor(0, ref c)) { t.throwBadArg<Color>(0); }
            ColorP = new ColorPriority(c);
        }
        public void write(TokenStream s) {
            s.write(Signature1.Header, Weight, Width, Flags);
            s.write(Signature2.Header, Color.R, Color.G, Color.B, Color.A);
        }
        public Token getToken() {
            throw new NotImplementedException();
        }
        #endregion
    }
    /// <summary>
    /// nd[X|Y|Z-Depth|Radius|Flags]
    /// ndcolor[R|G|B|A]
    /// 
    /// <example>
    /// Green Node With Radius 10 Located At (200,50) At Screen Depth Of 0.5 (Will Be Visible)
    /// nd[200|50|-0.5|10|0]
    /// ndcolor[0|255|0|255]
    /// </example>
    /// </summary>
    public class NodeData {
        public static NodeData None { get { return new NodeData(Vector2.Zero, 1, Color.Black); } }


        public Vector3 Position;
        public Vector2 Position2 { get { return new Vector2(Position.X, Position.Y); } }
        public float Radius;
        public ColorPriority ColorP;
        public Color Color { get { return ColorP.Color; } set { ColorP.setPriority(value); } }
        public byte Flags;
        public double Distance;

        public NodeData(Vector2 p, float r, Color c, byte flags = 0x00, double d = double.PositiveInfinity)
            : this(new Vector3(p, -0.5f), r, c, flags, d) {
        }
        public NodeData(Vector3 p, float r, Color c, byte flags = 0x00, double d = double.PositiveInfinity) {
            Position = p;
            Radius = r;
            ColorP = new ColorPriority(c);
            Flags = flags;
            Distance = d;
        }

        #region IGraphData Members
        static readonly Signature Signature1 = new Signature("nd", 5);
        static readonly Signature Signature2 = new Signature("ndcolor", 4);
        public void read(TokenStream s) {
            Token t;
            if(!s.search(Signature1, out t)) { Token.throwMissingSignature(Signature1); }
            if(!t.getArgVector3(0, ref Position)) { t.throwBadArg<Vector3>(0); }
            if(!t.getArg<float>(3, ref Radius)) { t.throwBadArg<float>(3); }
            if(!t.getArg<byte>(4, ref Flags)) { t.throwBadArg<byte>(4); }

            if(!s.search(Signature2, out t)) { Token.throwMissingSignature(Signature2); }
            Color c = Color.Transparent;
            if(!t.getArgColor(0, ref c)) { t.throwBadArg<Color>(0); }
            ColorP = new ColorPriority(c);
        }
        public void write(TokenStream s) {
            s.write(Signature1.Header, Position.X, Position.Y, Position.Z, Radius, Flags);
            s.write(Signature2.Header, Color.R, Color.G, Color.B, Color.A);
        }
        public Token getToken() {
            throw new NotImplementedException();
        }
        #endregion
    }
    #endregion

    #region Other Data
    public class MiscData {
        private static readonly Signature SigGridOptions = new Signature("grid", 2);
        private static readonly Signature SigGeoCode = new Signature("gmap", 5);

        public bool UseMap;
        public GraphGrid.BuildInfo GraphBI;
        public GeoCodeRequest GR;
        public ImageRequest IR;

        public MiscData() {
            UseMap = false;
            GR = new GeoCodeRequest();
            IR = new ImageRequest(0m, 0m, 12, 1024, 1024, 2, ImageRequest.MT_RoadMap);
        }

        public void read(TokenStream s) {
            Token t;
            if(!s.readUntilSignature(SigGridOptions, out t)) { Token.throwMissingSignature(SigGridOptions); }
            if(!t.getArgVector2(0, ref GraphBI.Size)) { t.throwBadArg<Vector2>(0); }

            if(!s.readUntilSignature(SigGeoCode, out t)) { UseMap = false; }
            else { UseMap = true; }
            if(UseMap) {
                if(!t.getArg<int>(0, ref IR.PWidth)) { }
                if(!t.getArg<int>(1, ref IR.PHeight)) { }
                if(!t.getArg<int>(2, ref IR.Zoom)) { }
                if(!t.getArg<int>(3, ref IR.Scale)) { }
                string a = t[4];
                if(string.IsNullOrWhiteSpace(a)) { UseMap = false; }
                else { GR = new GeoCodeRequest(a); }
            }
        }
        public void write(TokenStream s) {
            s.write(SigGridOptions.Header, GraphBI.Size.X, GraphBI.Size.Y);
            if(UseMap) {
                s.write(SigGeoCode.Header, IR.PWidth, IR.PHeight, IR.Zoom, IR.Scale, GR.Address);
            }
        }
    }
    public struct ColorPriority {
        public Color CBase, CPriority;
        private bool useP;
        public bool UsePriority { get { return useP; } }
        public Color Color { get { return useP ? CPriority : CBase; } }

        public ColorPriority(Color b) {
            CBase = b;
            useP = false;
            CPriority = Color.White;
        }

        public void prioritize() { useP = true; }
        public void unprioritize() { useP = false; }

        public void setPriority(Color c) {
            CPriority = c; prioritize();
        }
        public void setBase(Color c) {
            CBase = c;
        }
    }
    #endregion
}
