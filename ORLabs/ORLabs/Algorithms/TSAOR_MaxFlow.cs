using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZLibrary.Graphs;
using ZLibrary.Algorithms;
using ZLibrary.Math;
using ZLibrary.IO;
using Microsoft.Xna.Framework;
using ORLabs.Framework;

namespace ORLabs.Algorithms
{
    public class TSAOR_MaxFlow : TSAOR<TSAOR_MaxFlow.Input, TSAOR_MaxFlow.Output>
    {
        public struct Input
        {
            public int[] Flow, Resid;
            public ORGraph.Node[] Nodes;
            public ORGraph.Edge[] Edges;
            public ORGraph.Node Source;
            public ORGraph.Node Sink;
        }
        public struct Output
        {
            public int MaxFlow;
        }

        public struct Path : IEnumerable<int>
        {
            public bool HasEdges { get { return LLEI != null && LLEI.Count > 0; } }
            public LinkedList<int> LLEI;
            public int MaxFlow;

            public IEnumerator<int> GetEnumerator()
            {
                foreach (int i in LLEI) { yield return i; }
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                foreach (int i in LLEI) { yield return i; }
            }
        }

        public int[] flow, resid;
        public bool[] labels;

        public const byte FInAlg = Flags.Bit1;
        public const byte FLabelled = Flags.Bit2;
        public const byte FSource = Flags.Bit3;
        public const byte FSink = Flags.Bit4;
        public const byte FAugmented = Flags.Bit5;

        protected override void processThread()
        {
            // Setup Flow, Resid
            flow = input.Flow;
            resid = input.Resid;
            labels = new bool[input.Nodes.Length];
            eraseLabels();
            for (int i = 0; i < flow.Length; i++) { flow[i] = 0; }
            for (int i = 0; i < resid.Length; i++) { resid[i] = input.Edges[i].Data.Weight; }

            //Label All Edges/Nodes As In The Algorithms
            foreach (var n in input.Nodes)
            {
                Flags.addFlags(ref n.Flags, FInAlg);
                addState(new TSAORChange("Setting Up Nodes", n, ORColors.CNPassive));
            }
            Flags.addFlags(ref input.Source.Flags, FInAlg | FSource);
            addState(new TSAORChange("Adding Source Node", input.Source, ORColors.CNStart));
            Flags.addFlags(ref input.Sink.Flags, FInAlg | FSink);
            addState(new TSAORChange("Adding Sink Node", input.Sink, ORColors.CNEnd));
            foreach (var e in input.Edges)
            {
                Flags.addFlags(ref e.Flags, FInAlg);
                addState(new TSAORChange("Setting Up Edges", e, ORColors.CEPassive));
            }


            handleStepping(25);
            Output o = new Output();
            o.MaxFlow = 0;
            Path path = findPath();
            LinkedList<Path> lp = new LinkedList<Path>();
            while (path.HasEdges)
            {
                foreach (var ei in path)
                {
                    var e = input.Edges[ei];
                    Flags.addFlags(ref e.Flags, FAugmented);
                    addState(new TSAORChange("Augmenting Edge", e, ORColors.CEResult));
                    resid[ei] -= path.MaxFlow;
                    flow[ei] += path.MaxFlow;
                    e.Data.Weight -= path.MaxFlow;
                }
                o.MaxFlow += path.MaxFlow;
                lp.AddLast(path);

                eraseLabels();
                path = findPath();
            }

            // Set Back To Original Data
            foreach (var p in lp)
            {
                foreach (var ei in p)
                {
                    input.Edges[ei].Data.Weight += p.MaxFlow;
                    addState(new TSAORChange("Finished", input.Edges[ei], ORColors.CEResult));
                }
            }
            handleStepping(25);
        }
      
        public void eraseLabels()
        {
            for (int i = 0; i < labels.Length; i++) { labels[i] = false; }
        }

        public Path findPath()
        {
            Path p = new Path() { LLEI = new LinkedList<int>(), MaxFlow = int.MaxValue };
            labels[input.Source.Index] = true;
            findPath(input.Source.Index, p.LLEI);
            foreach (int i in p)
            {
                if (resid[i] < p.MaxFlow) { p.MaxFlow = resid[i]; }
            }
            if (!p.HasEdges) { p.MaxFlow = 0; }
            return p;
        }
        bool findPath(int ni, LinkedList<int> llei)
        {
            if (input.Nodes[ni].Index == input.Sink.Index) { return true; }

            foreach (var e in input.Nodes[ni].OutEdges)
            {
                addState(new TSAORChange("Looking At Outgoing Edge", e, ORColors.CELooking));
                handleStepping(25);

                // Check If A Path Can Move Through
                if (resid[e.Index] <= 0)
                {
                    addState(new TSAORChange("Not Enough Residual Flow To Pass Through", e, flow[e.Index] > 0 ? ORColors.CEResult : ORColors.CEPassive));
                    handleStepping(25);
                    continue;
                }

                int nio = e.EndNodeIndex;

                // Check If The Node Is Already Labelled
                if (labels[nio])
                {
                    addState(new TSAORChange("Path Already Taken", e, flow[e.Index] > 0 ? ORColors.CEResult : ORColors.CEPassive));
                    handleStepping(25);
                    continue;
                }
                labels[nio] = true;
                if (findPath(nio, llei))
                {
                    addState(new TSAORChange("Path Available", e, ORColors.CEIndexed));
                    handleStepping(25);
                    llei.AddFirst(e.Index); return true;
                }
            }
            return false;
        }

        public override TSAOR_MaxFlow.Output process(TSAOR_MaxFlow.Input input)
        {
            throw new NotImplementedException();
        }
    }
}
