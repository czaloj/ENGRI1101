using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA3D.Collision.Bodies;
using XNA3D.Collision.Interfaces;

namespace XNA3D.Collision.Abstract
{
    public abstract class AC3DCollidableAABB : AC3DCollidable, I3DCollideableAABB
    {
        public override COLLISION_TYPE CollisionType
        {
            get { return COLLISION_TYPE.AABB; }
        }

        protected Vector3 rectCenterOffset = Vector3.Zero;
        public Vector3 BoxCenter
        {
            get { return location + rectCenterOffset; }
        }

        protected Vector3 rectangleHalfSize;
        public Vector3 BoxHalfSize
        {
            get { return rectangleHalfSize; }
        }
    }
}
