using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA3D.Collision.Bodies
{
    public struct CollisionInformation
    {
        public bool HasCollided { get; set; }
        public float CollisionDepth { get; set; }
        public Vector3 Offset { get; set; }
        public float Distance { get; set; }
    }
}
