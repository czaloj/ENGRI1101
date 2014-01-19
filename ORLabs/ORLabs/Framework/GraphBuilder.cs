using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ORLabs.Framework {
    public sealed class GraphBuilder {
        private ORGraph graph;
        public ORGraph Graph { get { return graph; } }

        private Vector2 gridTL, gridSize;

        public GraphBuilder() {
            graph = null;
        }
    }
}
