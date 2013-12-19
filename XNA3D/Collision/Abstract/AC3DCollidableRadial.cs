using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA3D.Collision.Bodies;
using XNA3D.Collision.Interfaces;

namespace XNA3D.Collision.Abstract
{
    public abstract class AC3DCollidableRadial : AC3DCollidable, I3DCollideableRadial
    {
        public override COLLISION_TYPE CollisionType
        {
            get { return COLLISION_TYPE.RADIAL; }
        }

        protected Vector3 sphereCenterOffset;
        public Vector3 CollisionCenter
        {
            get { return location + sphereCenterOffset; }
        }

        protected float collisionRadius;
        public float CollisionRadius
        {
            get { return collisionRadius; }
        }
    }
}
