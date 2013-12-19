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
    public class TSAOR_Dijkstras : TSAOR<TSAOR_Dijkstras.Input, TSAOR_Dijkstras.PathTree>
    {
        protected ORGraph.Node[] nodes;
        protected int nCount;
        protected ORGraph.Edge[] edges;
        protected ORGraph.Node startNode;

        #region Data Structs
        public struct Input
        {
            public int NodeCount;
            public ORGraph.Node[] Nodes;
            public ORGraph.Edge[] Edges;
            public int StartNode;
        }
        public struct PathTree
        {
            public PathNode Root;
        }
        public class PathNode : IComparable<PathNode>
        {
            public ORGraph.Node Node;

            public double Distance;
            public BranchBinding PreviousBB;
            public PathNode PreviousNode;

            public LinkedList<BranchBinding> Branches;

            public PathNode(ORGraph.Node n)
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
        public struct BranchBinding
        {
            public PathNode Node;
            public ORGraph.Edge Edge;

            public BranchBinding(PathNode pn, ORGraph.Edge e)
            {
                Node = pn;
                Edge = e;
            }

            public override string ToString()
            {
                return Node.Node.Index.ToString();
            }
        }
        #endregion

        public const byte FlagPartOfAlg = Flags.Bit1;
        public const byte FlagStart = Flags.Bit2;
        public const byte FlagMin = Flags.Bit3;

        protected override void processThread()
        {
            //Set Input Information
            nodes = input.Nodes;
            edges = input.Edges;
            nCount = input.NodeCount;
            startNode = nodes[input.StartNode];

            //Remake The State Change Queue
            clearStates();

            //Set Graph To Default Information
            foreach (var e in edges)
            {
                if (e != null)
                {
                    addState(new TSAORChange("Setting Up Edges", e, ORColors.CEPassive));
                    e.Flags = FlagPartOfAlg;
                }
            }
            foreach (var n in nodes)
            {
                if (n != null)
                {
                    addState(new TSAORChange("Setting Up Nodes", n, ORColors.CNPassive, double.PositiveInfinity));
                    n.Flags = FlagPartOfAlg;
                }
            }

            //Create The Path Nodes
            TSAOR_Dijkstras.PathTree pTree = new PathTree();
            PathNode[] pNodes = new PathNode[nCount];
            for (int i = 0; i < pNodes.Length; i++)
            {
                pNodes[i] = new PathNode(nodes[i]);
                pNodes[i].Distance = double.PositiveInfinity;
                pNodes[i].PreviousBB = new BranchBinding(null, null);
            }
            pTree.Root = pNodes[input.StartNode];
            pTree.Root.Distance = 0;

            //Set Information For Start Node
            pTree.Root.Node.Flags |= FlagStart;
            addState(new TSAORChange("Setting Start Node", pTree.Root.Node, ORColors.CNStart, 0));
            handleStepping(25);

            MinHeap<PathNode> nHeap = new MinHeap<PathNode>(nCount);
            #region Insert All Locations Near The Start First
            foreach(ORGraph.Edge edge in pTree.Root.Node)
            {
                //Make Sure The Edge Is A Part Of The Algorithm
                if ((edge.Flags & FlagPartOfAlg) != FlagPartOfAlg) { continue; }
                PathNode opn = pNodes[(edge.Start.Index == pTree.Root.Node.Index) ? edge.End.Index : edge.Start.Index];

                addState(new TSAORChange(string.Format("Looking At Edge {0}-{1}", pTree.Root.Node.Index, opn.Node.Index), edge, ORColors.CELooking, FlagMin));
                handleStepping(25);

                if (edge.Data.Weight + pTree.Root.Distance < opn.Distance)
                {
                    opn.Distance = pTree.Root.Distance + edge.Data.Weight;

                    addState(new TSAORChange("Setting This Path As The Shortest", opn.Node, ORColors.CNIndexed, opn.Distance));
                    handleStepping(25);

                    addState(new TSAORChange("Setting This Path As The Shortest", edge, ORColors.CEResult, FlagMin));
                    edge.Flags |= FlagMin;
                    handleStepping(25);

                    opn.PreviousBB = new BranchBinding(opn, edge);
                    pTree.Root.addBranch(opn.PreviousBB);
                    opn.PreviousNode = pTree.Root;
                    nHeap.insert(opn);
                }
            }
            #endregion
            while (nHeap.Count > 0)
            {
                PathNode pn = nHeap.extract();
                foreach(ORGraph.Edge edge in pn.Node)
                {
                    //Make Sure The Edge Is A Part Of The Algorithm
                    if ((edge.Flags & FlagPartOfAlg) != FlagPartOfAlg) { continue; }
                    PathNode opn = pNodes[(edge.Start.Index == pn.Node.Index) ? edge.End.Index : edge.Start.Index];

                    addState(new TSAORChange(string.Format("Looking At Edge {0}-{1}", pn.Node.Index, opn.Node.Index), edge, ORColors.CELooking, FlagMin));
                    handleStepping(25);

                    if (edge.Data.Weight + pn.Distance < opn.Distance)
                    {
                        opn.Distance = pn.Distance + edge.Data.Weight;

                        addState(new TSAORChange("Setting This Path As The Shortest", opn.Node, ORColors.CNIndexed, opn.Distance));
                        handleStepping(25);

                        addState(new TSAORChange("Setting This Path As The Shortest", edge, ORColors.CEResult, FlagMin));
                        edge.Flags |= FlagMin;
                        handleStepping(25);

                        if (opn.PreviousBB.Edge != null)
                        {
                            unchecked { edge.Flags &= (byte)~(FlagMin); }
                            addState(new TSAORChange("Erasing Previous Best Path", opn.PreviousBB.Edge, ORColors.CEPassive));
                            handleStepping(25);
                        }
                        if (opn.PreviousNode != null) { opn.PreviousNode.removeBranch(opn.PreviousBB); }
                        opn.PreviousBB = new BranchBinding(opn, edge);
                        pn.addBranch(opn.PreviousBB);
                        opn.PreviousNode = pn;
                        nHeap.insert(opn);
                    }
                    else
                    {
                        //Make It Back To Its Old Color
                        if ((edge.Flags & FlagMin) == FlagMin)
                        {
                            addState(new TSAORChange("This Path Is Not The Shortest", edge, ORColors.CEResult, FlagMin));
                        }
                        else
                        {
                            addState(new TSAORChange("This Path Is Not The Shortest", edge, ORColors.CEPassive, FlagMin));
                        }
                    }
                }

                addState(new TSAORChange(string.Format("Looking At Node {0}", pn.Node.Index), pn.Node, ORColors.CNLooking));
                handleStepping(25);
            }

            addState(new TSAORChange("Algorithm Is Finished", pTree.Root.Node, ORColors.CNEnd));


            result = pTree;
            endThread(true);
        }

        public override TSAOR_Dijkstras.PathTree process(TSAOR_Dijkstras.Input input)
        {
            nodes = input.Nodes;
            edges = input.Edges;
            nCount = input.NodeCount;
            startNode = nodes[input.StartNode];

            TSAOR_Dijkstras.PathTree pTree = new PathTree();
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
                foreach (ORGraph.Edge edge in pn.Node)
                {
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
