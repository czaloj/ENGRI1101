using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZLibrary.ADT;
using ZLibrary.Graphs;
using ZLibrary.Algorithms;
using ZLibrary.Math;
using ZLibrary.IO;
using Microsoft.Xna.Framework;
using ORLabs.Framework;

namespace ORLabs.Algorithms
{
    public enum MinSpanTreeAlgorithm
    {
        KruskalAdd,
        KruskalRemove,
        Prim
    }
    // TODO: Add Text To Changes
    public class TSAOR_MinSpanTree : TSAOR<TSAOR_MinSpanTree.Input, TSAOR_MinSpanTree.Output>
    {
        public const byte FlagInAlg = Flags.Bit1;
        public const byte FlagMin = Flags.Bit3;

        #region Inner
        public struct Input
        {
            public ORGraph.Edge[] Edges;
            public ORGraph.Node[] Nodes;
            public ORGraph.Node Start;
            public MinSpanTreeAlgorithm AlgType;
            public Input(ORGraph g, MinSpanTreeAlgorithm a = MinSpanTreeAlgorithm.KruskalAdd)
            {
                Edges = g.Edges;
                Nodes = g.Nodes;
                AlgType = a;
                Start = null;
            }
            public Input(ORGraph g, ORGraph.Node n)
            {
                Edges = g.Edges;
                Nodes = g.Nodes;
                AlgType = MinSpanTreeAlgorithm.Prim;
                Start = n;
            }
        }
        public struct Output
        {
            public ORGraph.Edge[] Edges;

            public Output(int nCount)
            {
                Edges = new ORGraph.Edge[nCount - 1];
            }
        }
        public struct EdgeBind : IComparable<EdgeBind>
        {
            private ORGraph.Edge e;
            public ORGraph.Edge Edge
            {
                get { return e; }
            }

            public EdgeBind(ORGraph.Edge e)
            {
                this.e = e;
            }

            public int CompareTo(EdgeBind other)
            {
                return e.Data.Weight.CompareTo(other.e.Data.Weight);
            }
        }
        #endregion

        protected override void processThread()
        {
            clearStates();

            switch (input.AlgType)
            {
                case MinSpanTreeAlgorithm.Prim:
                    result = algPrimThread();
                    break;
                case MinSpanTreeAlgorithm.KruskalAdd:
                default:
                    result = algKruskalThread();
                    break;
            }
            endThread(true);
        }

        double randhue(Random r)
        {
            switch (r.Next(4))
            {
                case 0: return r.NextDouble() * 60 + 0;
                case 1: return r.NextDouble() * 60 + 60;
                case 2: return r.NextDouble() * 60 + 120;
                default: return r.NextDouble() * 60 + 300;
            }
        }
        void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color
                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color
                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color
                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color
                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.
                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.
                    default:
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }
        int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }

        protected Output algKruskalThread()
        {
            Output output = new Output(input.Nodes.Length);
            PartitionTree pt = new PartitionTree(input.Nodes);
            int ei = 0;

            Color[] colors = new Color[pt.Partitions.Length];
            Random r = new Random();
            int red, green, blue;
            for (int i = 0; i < colors.Length; i++)
            {
                HsvToRgb(randhue(r), 1, r.NextDouble() * 0.7 + 0.3, out red, out green, out blue);
                colors[i] = new Color(red, green, blue, 255);
            }

            //Preset
            MinHeap<EdgeBind> heap = new MinHeap<EdgeBind>(input.Edges.Length);
            foreach (var edge in input.Edges)
            {
                edge.Flags = FlagInAlg;
                addState(new TSAORChange("Setting Up Edges", edge, ORColors.CEPassive));
                heap.insert(new EdgeBind(edge));
            }
            foreach (var node in input.Nodes)
            {
                node.Flags = FlagInAlg;
                addState(new TSAORChange("Setting Up Nodes", node, colors[node.Index]));
            }
            System.Threading.Thread.Sleep(100);
            handleStepping(25);

            while (heap.Count > 0 && ei < input.Nodes.Length - 1)
            {
                ORGraph.Edge edge = heap.extract().Edge;
                ORGraph.Node
                    n1 = edge.Start as ORGraph.Node,
                    n2 = edge.End as ORGraph.Node;
                if (!n1.Flags.hasFlags(FlagInAlg) ||
                    !n2.Flags.hasFlags(FlagInAlg)
                    )
                {
                    continue;
                }
                addState(new TSAORChange(string.Format("Looking At Edge {0}", edge.Index), edge, ORColors.CELooking));
                handleStepping(25);
                if (!pt.loopBetween(n1, n2))
                {
                    Color pc;
                    pt.mergePartitions(n1, n2);
                    int pi = pt.Partitions[n1.Index].PartitionIndex;
                    pc = colors[pi];
                    foreach (Partition p in pt.Partitions[n1.Index].LinkedPartitions)
                    {
                        foreach (ORGraph.Edge ee in p.node)
                        {
                            if (ee.Flags.hasFlags(FlagMin))
                            {
                                addState(new TSAORChange(string.Format("Setting Edge {0} To Partition {1}", ee.Index, pi), ee, pc));
                            }
                        }
                        addState(new TSAORChange(string.Format("Setting Node {0} To Partition {1}", p.node.Index, pi), p.node, pc));
                    }

                    output.Edges[ei++] = edge;
                    addState(new TSAORChange(string.Format("Setting Edge Connector To Partition {0}", pi), edge, pc));
                    Flags.addFlags(ref edge.Flags, FlagMin);
                    handleStepping(25);
                }
                else
                {
                    addState(new TSAORChange(string.Format("Removing Edge {0} From Tree", edge.Index), edge, ORColors.CExcluded));
                    handleStepping(25);
                }
            }

            foreach (var e in heap) { addState(new TSAORChange(string.Format("Removing Edge {0} From Tree", e.Edge.Index), e.Edge, ORColors.CExcluded)); }
            handleStepping(25);

            foreach (var e in input.Edges)
            {
                if (!e.Flags.hasFlags(FlagMin))
                { addState(new TSAORChange("Making Edges Invisible", e, Color.Transparent)); }
            }

            return output;
        }
        protected Output algPrimThread()
        {
            Output output = new Output(input.Nodes.Length);
            PartitionTree pt = new PartitionTree(input.Nodes);
            int ei = 0;

            //Preset
            MinHeap<EdgeBind> heap = new MinHeap<EdgeBind>(input.Edges.Length);
            foreach (var edge in input.Edges)
            {
                edge.Flags = FlagInAlg;
                addState(new TSAORChange("Setting Up Edges", edge, ORColors.CEPassive));
            }
            foreach (var node in input.Nodes)
            {
                node.Flags = FlagInAlg;
                addState(new TSAORChange("Setting Up Nodes", node, ORColors.CNPassive));
            }
            Flags.addFlags(ref input.Start.Flags, FlagMin);
            foreach (ORGraph.Edge edge in input.Start)
            {
                heap.insert(new EdgeBind(edge));
                //addState(new TSAORChange("TODO", edge, CEIndexed));
            }
            System.Threading.Thread.Sleep(100);
            handleStepping(25);

            while (heap.Count > 0 && ei < input.Nodes.Length - 1)
            {
                ORGraph.Edge edge = heap.extract().Edge;
                ORGraph.Node
                    n1 = edge.Start as ORGraph.Node,
                    n2 = edge.End as ORGraph.Node;
                if (!n1.Flags.hasFlags(FlagInAlg) ||
                    !n2.Flags.hasFlags(FlagInAlg)
                    )
                {
                    continue;
                }
                addState(new TSAORChange(string.Format("Looking At Edge {0}", edge.Index), edge, ORColors.CELooking));
                handleStepping(25);

                if (!pt.loopBetween(n1, n2))
                {
                    pt.mergePartitions(n1, n2);
                    Flags.addFlags(ref edge.Flags, FlagMin);

                    if (!n1.Flags.hasFlags(FlagMin))
                    {
                        Flags.addFlags(ref n1.Flags, FlagMin);
                        foreach (ORGraph.Edge ee in n1)
                        {
                            if (!ee.Flags.hasFlags(FlagMin)) { heap.insert(new EdgeBind(ee)); }
                            addState(new TSAORChange(string.Format("Marking Node As Visited"), n1, ORColors.CEIndexed));
                        }
                    }
                    else if (!n2.Flags.hasFlags(FlagMin))
                    {
                        Flags.addFlags(ref n2.Flags, FlagMin);
                        foreach (ORGraph.Edge ee in n2)
                        {
                            if (!ee.Flags.hasFlags(FlagMin)) { heap.insert(new EdgeBind(ee)); }
                            addState(new TSAORChange(string.Format("Marking Node As Visited"), n2, ORColors.CEIndexed));
                        }
                    }

                    output.Edges[ei++] = edge;
                    addState(new TSAORChange("Adding Edge To Tree", edge, ORColors.CEResult));
                    handleStepping(25);
                }
                else
                {
                    addState(new TSAORChange("Excluding Edge From Tree", edge, ORColors.CExcluded));
                    handleStepping(25);
                }
            }
            foreach (var e in input.Edges)
            {
                if (!e.Flags.hasFlags(FlagMin))
                { addState(new TSAORChange(string.Format("Making Edge {0} Invisible", e.Index), e, Color.Transparent)); }
            }

            return output;
        }

        public override TSAOR_MinSpanTree.Output process(TSAOR_MinSpanTree.Input input)
        {
            throw new NotImplementedException();
        }
    }
}
