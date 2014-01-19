using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ORLabs.Framework;
using BlisterUI;
using XNA3D.Graphics;

namespace ORLabs.Graphics.Widgets {
    public class GraphDataWidget : IMTransVisible {
        protected string nodeData;
        private const int NodeDataLength = 5 + 16 * 3;
        private const string NodeNoData = "Node:\nNone           \nSelected       \n               ";
        private const string NodeFormat = "Node:\nIndex:    {0,5}\nDegree:   {1,5}\nDistance: {2,5}";
        public void resetData(ORGraph.Node n) {
            if(n == null) { nodeData = NodeNoData; }
            else { nodeData = string.Format(NodeFormat, n.Index, n.Degree, n.Distance); }
        }

        protected string edgeData;
        private const int EdgeDataLength = 5 + 16 * 3;
        private const string EdgeNoData = "Edge:\nNone           \nSelected       \n               ";
        private const string EdgeFormat = "Edge:\nIndex:    {0,5}\nWeight:   {1,5}\n               ";
        private const string EdgeFormatFlow = "Edge:\nIndex:    {0,5}\nFlow:     {1,5}\nResidual: {2,5}";
        public void resetData(ORGraph.Edge e, bool useFlow = false, int flow = 0, int resid = 0) {
            if(e == null) { edgeData = EdgeNoData; }
            else {
                edgeData = useFlow
                ? string.Format(EdgeFormatFlow, e.Index, flow, resid)
                : string.Format(EdgeFormat, e.Index, e.Data.Weight);
            }
        }

        protected SpriteFont font;
        public Color FontColor { get; set; }
        protected Vector2 backRectPos;
        protected Vector2 backRectSize;

        public GraphDataWidget(SpriteFont f) {
            font = f;
            nodeData = NodeNoData;
            edgeData = EdgeNoData;
        }

        public void draw(SpriteBatch batch) {
            if(IsVisible) batch.DrawString(font, nodeData + "\n" + edgeData, mWorld.Position, FontColor, mWorld.Rotation, Vector2.Zero, mWorld.Scaling, SpriteEffects.None, mWorld.Depth);
        }

        private WidgetFrame mWorld;
        public WidgetFrame World {
            get {
                return mWorld;
            }
            set {
                mWorld = value;
                backRectPos = WidgetFrame.Transform(new Vector2(-10, -10), mWorld);
            }
        }

        public bool IsVisible {
            get;
            private set;
        }
        public void setVisible(bool b) {
            if(IsVisible != b) {
                IsVisible = b;
            }
        }
    }
}
