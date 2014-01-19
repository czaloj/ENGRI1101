using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ORLabs.Framework;
using ZLibrary.Graphs;

namespace ORLabs.Graphics.Graphs {
    public class GraphRenderer {
        #region Static
        public static readonly DepthStencilState DSS_Nodes = new DepthStencilState() {
            StencilEnable = true,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 1
        };
        public static readonly DepthStencilState DSS_Edges = new DepthStencilState() {
            StencilEnable = true,
            StencilFunction = CompareFunction.Equal,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 0
        };
        #endregion

        public Effect fxGraph;
        public EffectTechniqueCollection Techniques {
            get { return fxGraph.Techniques; }
        }
        public EffectPassCollection Passes {
            get { return fxGraph.CurrentTechnique.Passes; }
        }
        public EffectParameterCollection Parameters {
            get { return fxGraph.Parameters; }
        }

        ORGraph graph;
        EdgeList lEdges;
        NodeList lNodes;
        float thetaTime;

        public bool AsEdge { get; set; }
        public float Transparency { get; set; }
        public float Amplitude { get; set; }
        public float Frequency { get; set; }

        public GraphRenderer(GraphicsDevice g, ORGraph gr) {
            setGraph(gr, g);
            thetaTime = 0;

            AsEdge = true;
            Transparency = 0.8f;
            Amplitude = 1f;
            Frequency = 2f;
        }

        public void loadEffect(string file, string technique, ContentManager content) {
            fxGraph = content.Load<Effect>(file);
            fxGraph.CurrentTechnique = fxGraph.Techniques[technique];
        }

        public void setGraph(ORGraph gr, GraphicsDevice g) {
            graph = gr;
#if HIDEF
            lEdges = new EdgeList(g, graph.EdgeCount);
            lNodes = new NodeList(g, graph.NodeCount);
#else
            lEdges = new EdgeList(g, graph.EdgeCount);
            lNodes = new NodeList(g, graph.NodeCount);
#endif
        }
        public void setCurrentTechnique(string technique) {
            fxGraph.CurrentTechnique = fxGraph.Techniques[technique];
        }
        public void apply(int pIndex) {
            fxGraph.CurrentTechnique.Passes[pIndex].Apply();
        }

        public void rebuild(GraphicsDevice g) {
            ORGraph.Edge e;
            ORGraph.Node n;

            if(lEdges.MaxCount < graph.EdgeCount) { lEdges.append(g, new EdgeInfo[graph.EdgeCount - lEdges.MaxCount]); }
            lEdges.begin();
            for(int i = 0; i < graph.EdgeCount; i++) {
                e = graph.Edges[i];
                if(e == null) { break; }
                EdgeInfo li = new EdgeInfo(
                    e.Start.Data.Position,
                    e.Data.Color,
                    e.Start.Data.Radius,
                    e.End.Data.Position,
                    e.Data.Color,
                    e.End.Data.Radius,
                    Color.Transparent
                    );
                e.setListIndex(lEdges.Count, lEdges);
                lEdges.add(li);
            }
            lEdges.end();

            if(lNodes.MaxCount < graph.NodeCount) { lNodes.append(g, new NodeInfo[graph.NodeCount - lNodes.MaxCount]); }
            lNodes.begin();
            for(int i = 0; i < graph.NodeCount; i++) {
                n = graph.Nodes[i];
                if(n == null) { break; }
                NodeInfo ni = new NodeInfo(
                    n.Data.Position,
                    Vector2.One * n.Data.Radius,
                    n.Data.Color,
                    Color.Transparent
                    );
                n.setListIndex(lNodes.Count, lNodes);
                lNodes.add(ni);
            }
            lNodes.end();
        }

        public void update(float dt) {
            thetaTime = (float)Math.IEEERemainder(thetaTime + dt * (AsEdge ? Frequency : 1), 2 * Math.PI);
            if(lNodes.ShouldBuild) { lNodes.end(); }
            if(lEdges.ShouldBuild) { lEdges.end(); }
        }

        public void draw(GraphicsDevice g, Matrix mView, Matrix mProjection, Vector2 camVS, Vector2 viewSize) {
            g.RasterizerState = RasterizerState.CullNone;
            g.BlendState = BlendState.AlphaBlend;
            if(graph.Grid != null) {
                graph.Grid.draw(g, mView * mProjection, (viewSize / camVS).X * 2f);
            }

            if(lEdges.Count <= 0 && lNodes.Count <= 0) { return; }

            Parameters["World"].SetValue(Matrix.Identity);
            Parameters["View"].SetValue(mView);
            Parameters["Projection"].SetValue(mProjection);
#if HIDEF
            Parameters["ThetaTime"].SetValue(thetaTime);
            Parameters["Amplitude"].SetValue(Amplitude);
            Parameters["Frequency"].SetValue(AsEdge ? 1 : Frequency);
#endif
            Parameters["AsEdge"].SetValue(AsEdge);
            Parameters["Transparency"].SetValue(Transparency);

            g.DepthStencilState = DSS_Nodes;
            if(lNodes.Count > 0) {
                Passes["Node"].Apply();
                lNodes.set(g);
                lNodes.draw(g);
            }

            g.DepthStencilState = DSS_Edges;
            if(lEdges.Count > 0) {
                Passes["Line"].Apply();
                lEdges.set(g);
                lEdges.draw(g);
            }
        }
    }
}
