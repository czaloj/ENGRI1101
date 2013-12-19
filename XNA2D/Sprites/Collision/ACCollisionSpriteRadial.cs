using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using XNA2D.Sprites.Abstract;
using XNA2D.Interfaces.Collision;
using XNA2D.Framework.Collision;

namespace XNA2D.Sprites.Collision
{
    public abstract class ACCollisionSpriteRadial : ACCollisionSprite, ISpriteCollideableRadial
    {
        public override COLLISION_TYPE CollisionType
        {
            get { return COLLISION_TYPE.RADIAL; }
        }

        public Vector2 CollisionCenter
        {
            get { return position; }
        }

        protected float collisionRadius;
        public float CollisionRadius
        {
            get { return collisionRadius; }
        }
    }
}
