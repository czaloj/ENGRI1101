using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ORLabs.Framework;
using ORLabs.Algorithms;
using ORLabs.Graphics;

namespace ORLabs.Screens
{
    // Djikstra's Algorithm
    public class Lab2 : LabGraphSreen<TSAOR_Dijkstras, TSAOR_Dijkstras.Input, TSAOR_Dijkstras.PathTree>
    {
        public override bool beginAlg(ORGraph.Node node)
        {
            alg.begin(new TSAOR_Dijkstras.Input()
            {
                Edges = graph.Edges,
                Nodes = graph.Nodes,
                NodeCount = graph.NodeCount,
                StartNode = node.Index
            });
            return true;
        }
        public override bool beginAlg()
        {
            return false;
        }

        protected override void makeAlg(out TSAOR_Dijkstras a)
        {
            a = new TSAOR_Dijkstras();
            a.UseStepping = true;
            a.PauseByStep = false;
            a.MilliPauseRate = 4;
        }

        protected override string LabName
        {
            get { return "Lab2"; }
        }
        protected override string AvailableActionText
        {
            get { return
@"Available Actions:
Previous Screen - 'P'
Load Graph - 'F1'
Clear Graph - 'C'

Begin Djikstras - Mouse Over Node, 'B' + Left Mouse
Kill Algorithm - 'T'
Next Step In Algorithm - 'N'

Node Data - Mouse Over Node
Edge Data - Mouse Over Node + '1' + Mouse Wheel
Slider On Bottom Controls Execution Speed
"
; }
        }
    }

    // Kruskal's/Prim's Algorithm
    public class Lab3 : LabGraphSreen<TSAOR_MinSpanTree, TSAOR_MinSpanTree.Input, TSAOR_MinSpanTree.Output>
    {
        public override bool beginAlg(ORGraph.Node node)
        {
            alg.MilliPauseRate = 2;
            alg.begin(new TSAOR_MinSpanTree.Input(graph, node));
            return true;
        }
        public override bool beginAlg()
        {
            alg.MilliPauseRate = 4;
            alg.begin(new TSAOR_MinSpanTree.Input(graph, MinSpanTreeAlgorithm.KruskalAdd));
            return true;
        }

        protected override void makeAlg(out TSAOR_MinSpanTree a)
        {
            a = new TSAOR_MinSpanTree();
            a.UseStepping = true;
            a.PauseByStep = false;
            a.MilliPauseRate = 4;
        }

        protected override string LabName
        {
            get { return "Lab3"; }
        }
        protected override string AvailableActionText
        {
            get
            {
                return
@"Available Actions:
Previous Screen - 'P'
Load Graph - 'F1'
Clear Graph - 'C'

Begin Prim's - Mouse Over Node, 'B' + Left Mouse
Begin Kruskal's - No Mouse Over Node, 'B' + Left Mouse
Kill Algorithm - 'T'
Next Step In Algorithm - 'N'

Node Data - Mouse Over Node
Edge Data - Mouse Over Node + '1' + Mouse Wheel
Slider On Bottom Controls Execution Speed
"
                    ;
            }
        }
    }

    // Max-Flow Algorithm
    public abstract class LabMF : LabGraphSreen<TSAOR_MaxFlow, TSAOR_MaxFlow.Input, TSAOR_MaxFlow.Output>
    {
        ORGraph.Node source;
        int[] flow, resid;

        public override void onEntry(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.onEntry(gameTime);
            gr.AsEdge = false;
        }
        public override void onExit(Microsoft.Xna.Framework.GameTime gameTime)
        {
            gr.AsEdge = true;
            base.onExit(gameTime);
        }
        public override bool beginAlg(ORGraph.Node node)
        {
            if (source == null)
            {
                source = node;
                source.setBColor(ORColors.CNStart);
                return false;
            }
            else
            {
                flow = new int[graph.Edges.Length];
                resid = new int[graph.Edges.Length];
                foreach (var e in graph.Edges)
                {
                    flow[e.Index] = 0;
                    resid[e.Index] = e.Data.Weight;
                }
                alg.UseStepping = true;
                alg.MilliPauseRate = 50;
                alg.begin(new TSAOR_MaxFlow.Input()
                {
                    Edges = graph.Edges,
                    Nodes = graph.Nodes,
                    Source = source,
                    Sink = node,
                    Flow = flow,
                    Resid = resid
                });
                source = null;
                return true;
            }
        }
        public override bool beginAlg()
        {
            return false;
        }

        public override void updateInfo(ORGraph.Edge e)
        {
            if(e == null || flow == null) {  }
            else
            {
                gData.resetData(e, true, flow[e.Index], resid[e.Index]);
            }
        }

        protected override void makeAlg(out TSAOR_MaxFlow a)
        {
            a = new TSAOR_MaxFlow();
            a.UseStepping = true;
            a.PauseByStep = false;
            a.MilliPauseRate = 1;
        }
    }
    public class Lab4 : LabMF
    {
        protected override string LabName
        {
            get { return "Lab4"; }
        }
        protected override string AvailableActionText
        {
            get
            {
                return
@"Available Actions:
Previous Screen - 'P'
Load Graph - 'F1'
Clear Graph - 'C'

Begin Max Flow - 
  Hold 'B' while
  Mouse Over Source Node + Left Mouse,
  Mouse Over Sink Node + Left Mouse
Kill Algorithm - 'T'
Next Step In Algorithm - 'N'

Node Data - Mouse Over Node
Edge Data - Mouse Over Node + '1' + Mouse Wheel
Slider On Bottom Controls Execution Speed
"
                    ;
            }
        }
    }
    public class Lab5 : LabMF
    {
        protected override string LabName
        {
            get { return "Lab5"; }
        }
        protected override string AvailableActionText
        {
            get
            {
                return
@"Available Actions:
Previous Screen - 'P'
Load Graph - 'F1'
Clear Graph - 'C'

Begin Max Flow - 
  Hold 'B' while
  Mouse Over Source Node + Left Mouse,
  Mouse Over Sink Node + Left Mouse
Kill Algorithm - 'T'
Next Step In Algorithm - 'N'

Node Data - Mouse Over Node
Edge Data - Mouse Over Node + '1' + Mouse Wheel
Slider On Bottom Controls Execution Speed
"
                    ;
            }
        }
    }
}
