using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA2D.Framework.Collision
{
    public struct CollisionInformation
    {
        public bool HasCollided { get; set; }
        public float CollisionDepth { get; set; }
        public Vector2 Offset { get; set; }
        public float Distance { get; set; }
    }
}
