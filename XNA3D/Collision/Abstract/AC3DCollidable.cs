using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA3D.Objects;
using XNA3D.Collision.Bodies;
using XNA3D.Collision.Interfaces;

namespace XNA3D.Collision.Abstract
{
    public abstract class AC3DCollidable : GameObject, I3DCollideable
    {
        public abstract COLLISION_TYPE CollisionType { get; }
    }
}
