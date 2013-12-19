using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ZLibrary.ADT;
using ZLibrary.Graphs;
using ZLibrary.Math;

namespace ZLibrary.Algorithms
{
    public class DjikstrasTSA : ACStateAlgorithm<DjikstrasTSA.Input, DjikstrasTSA.PathTree, DjikstrasTSA.Change>
    {
        protected SVGraph.Node[] nodes;
        protected int nCount;
        protected SVGraph.Edge[] edges;
        protected SVGraph.Node startNode;

        public struct Input
        {
            public int NodeCount;
            public SVGraph.Node[] Nodes;
            public SVGraph.Edge[] Edges;
            public int StartNode;
        }
        
        public struct Change
        {
            public bool IsNode;
            public int Index;
            public Color Color;
            public byte FlagsOr;
            public byte FlagsAnd;
            public double Distance;

            public Change(SVGraph.Edge e, Color c, byte flags = NoNewFlags)
            {
                IsNode = false;
                Index = e.Index;
                Color = c;
                FlagsOr = (byte)((flags == NoNewFlags) ? 0x00 : flags);
                FlagsAnd = 0xff;
                Distance = 0;
            }
            public Change(SVGraph.Node n, Color c, byte flags = NoNewFlags, double d = -1, byte clearFlags = 0x00)
            {
                IsNode = true;
                Index = n.Index;
                Color = c;
                byte pf = n.Data.Flags;
                FlagsAnd = (byte)(~clearFlags);
                FlagsOr = (byte)((flags == NoNewFlags) ? 0x00 : flags);
                //Flags = (flags == NoNewFlags) ? n.Data.Flags :
                //    (byte)((n.Data.Flags & ~clearFlags) | flags);
                //if(((pf & FlagStart) == FlagStart) && ((Flags & FlagStart) != FlagStart))
                //{throw new ArgumentException(string.Format("{0,02} & {1,02} | {2} = {3}"
                //    , n.Data.Flags, ~clearFlags, flags, Flags
                //    ));}
                Distance = d;
            }
        }
        public struct Changes
        {
            public Change[] changes;

            public Change this[int i]
            {
                get
                {
                    return changes[i];
                }
                set
                {
                    changes[i] = value;
                }
            }
            public Changes(int ChangeCount)
            {
                changes = new Change[ChangeCount];
            }
        }

        public struct PathTree
        {
            public PathNode Root;
        }
        public struct BranchBinding
        {
            public PathNode Node;
            public SVGraph.Edge Edge;

            public BranchBinding(PathNode pn, SVGraph.Edge e)
            {
                Node = pn;
                Edge = e;
            }

            public override string ToString()
            {
                return Node.Node.Index.ToString();
            }
        }
        public class PathNode : IComparable<PathNode>
        {
            public SVGraph.Node Node;

            public double Distance;
            public BranchBinding PreviousBB;
            public PathNode PreviousNode;

            public LinkedList<BranchBinding> Branches;

            public PathNode(SVGraph.Node n)
            {
                Node = n;
                Branches = new LinkedList<BranchBinding>();
            }
            public void addBranch(BranchBinding bb)
            {
                Branches.AddLast(bb);
            }
            public void removeBranch(BranchBinding bb)
            {
                Branches.Remove(bb);
            }

            public int CompareTo(PathNode other)
            {
                return Distance.CompareTo(other.Distance);
            }

            public override string ToString()
            {
                return Node.Index.ToString() + " -> " + string.Join(".", Branches);
            }
        }

        public const byte FlagMin = 0x01;
        public const byte FlagPassive = 0x00;
        public const byte FlagStart = 0x02;
        public const byte FlagIndexed = 0x04;
        public const byte FlagLookedAt = 0x08;
        public const byte NoNewFlags = 0xff;

        public static readonly Color cEPassive = new Color(30, 30, 30, 110);
        public static readonly Color cELookedAt = new Color(10, 85, 10, 25);
        public static readonly Color cEMin = new Color(255, 10, 10, 255);

        public static readonly Color cNStart = new Color(10, 25, 255, 10);
        public static readonly Color cNPassive = new Color(40, 0, 40, 140);
        public static readonly Color cNLookedAt = new Color(255, 255, 0, 255);
        public static readonly Color cNIndexed = new Color(200, 0, 250, 255);

        protected override void processThread()
        {
            nodes = input.Nodes;
            edges = input.Edges;
            nCount = input.NodeCount;
            //graph = input.Graph;
            startNode = nodes[input.StartNode];

            

            DjikstrasTSA.PathTree pTree = new PathTree();
            PathNode[] pNodes = new PathNode[nCount];
            for (int i = 0; i < pNodes.Length; i++)
            {
                pNodes[i] = new PathNode(nodes[i]);
                pNodes[i].Distance = double.PositiveInfinity;
                pNodes[i].PreviousBB = new BranchBinding(null, null);
            }
            pTree.Root = pNodes[input.StartNode];
            pTree.Root.Distance = 0;

            StateQueue = new Queue<Change>(edges.Length + nodes.Length + 30);

            foreach (var e in edges) { if (e != null) { StateQueue.Enqueue(new Change(e, cEPassive)); } }
            System.Threading.Thread.Sleep(100);
            foreach (var n in nodes) { if (n != null) { StateQueue.Enqueue(new Change(n, cNPassive, FlagPassive, double.PositiveInfinity, 0xff)); } }
            handleStepping(25);
            StateQueue.Enqueue(new Change(pTree.Root.Node, cNStart, FlagStart, 0, 0xff));

            MinHeap<PathNode> nHeap = new MinHeap<PathNode>(nCount);
            nHeap.insert(pTree.Root);
            while (nHeap.Count > 0)
            {
                PathNode pn = nHeap.extract();

                foreach(SVGraph.Edge edge in pn.Node)
                //var enumerator = pn.Node.getEnumerator(3);
                //while (enumerator.MoveNext())
                {
                    //var edge = enumerator.Current as SVGraph.Edge;
                    PathNode opn = pNodes[(edge.Start.Index == pn.Node.Index) ? edge.End.Index : edge.Start.Index];

                    if (edge.Data.Weight + pn.Distance < opn.Distance)
                    {
                        opn.Distance = pn.Distance + edge.Data.Weight;
                        StateQueue.Enqueue(new Change(opn.Node, cNIndexed, FlagIndexed, opn.Distance, FlagPassive | FlagLookedAt));
                        StateQueue.Enqueue(new Change(edge, cEMin, FlagMin));
                        if (opn.PreviousBB.Edge != null)
                        {
                            StateQueue.Enqueue(new Change(opn.PreviousBB.Edge, cEPassive, FlagPassive));
                        }
                        if (opn.PreviousNode != null) { opn.PreviousNode.removeBranch(opn.PreviousBB); }
                        opn.PreviousBB = new BranchBinding(opn, edge);
                        pn.addBranch(opn.PreviousBB);
                        opn.PreviousNode = pn;
                        nHeap.insert(opn);
                    }
                }
                StateQueue.Enqueue(new Change(pn.Node, cNLookedAt, FlagLookedAt, -1, FlagIndexed));

                handleStepping(25);
            }


            result = pTree;
            endThread(true);
        }

        public override DjikstrasTSA.PathTree process(DjikstrasTSA.Input input)
        {
            nodes = input.Nodes;
            edges = input.Edges;
            nCount = input.NodeCount;
            //graph = input.Graph;
            startNode = nodes[input.StartNode];

            DjikstrasTSA.PathTree pTree = new PathTree();
            PathNode[] pNodes = new PathNode[nCount];
            for (int i = 0; i < pNodes.Length; i++)
            {
                pNodes[i] = new PathNode(nodes[i]);
                pNodes[i].Distance = double.PositiveInfinity;
                pNodes[i].PreviousBB = new BranchBinding(null, null);
            }
            pTree.Root = pNodes[input.StartNode];
            pTree.Root.Distance = 0;

            MinHeap<PathNode> nHeap = new MinHeap<PathNode>(nCount);
            nHeap.insert(pTree.Root);
            while (nHeap.Count > 0)
            {
                PathNode pn = nHeap.extract();
                foreach(SVGraph.Edge edge in pn.Node)
                //var enumerator = pn.Node.getEnumerator(3);
                //while (enumerator.MoveNext())
                {
                    //var edge = enumerator.Current as SVGraph.Edge;
                    PathNode opn = pNodes[(edge.Start.Index == pn.Node.Index) ? edge.End.Index : edge.Start.Index];
                    if (edge.Data.Weight + pn.Distance < opn.Distance)
                    {
                        opn.Distance = pn.Distance + edge.Data.Weight;
                        if (opn.PreviousNode != null) { opn.PreviousNode.removeBranch(opn.PreviousBB); }
                        opn.PreviousBB = new BranchBinding(opn, edge);
                        pn.addBranch(opn.PreviousBB);
                        opn.PreviousNode = pn;
                        nHeap.insert(opn);
                    }
                }
            }

            return pTree;
        }
    }
}
