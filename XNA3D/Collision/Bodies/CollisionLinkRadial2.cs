using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA3D.Collision.Interfaces;

namespace XNA3D.Collision.Bodies
{
    public class CollisionLinkRadial2 : CollisionLink<ICollisionRadial, ICollisionRadial>
    {
        public CollisionLinkRadial2(ICollisionRadial c1, ICollisionRadial c2)
            : base(c1, c2)
        {

        }

        public override CollisionInformation collide()
        {
            CollisionInformation info = new CollisionInformation();
            info.Offset = c1.CollisionCenter - c2.CollisionCenter;
            info.Distance = info.Offset.Length();
            info.CollisionDepth = (c1.CollisionRadius + c2.CollisionRadius) - info.Distance;
            info.HasCollided = info.CollisionDepth > 0f;
            return info;
        }
    }
}
