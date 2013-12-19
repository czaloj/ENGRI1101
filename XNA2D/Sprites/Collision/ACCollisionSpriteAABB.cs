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
    public abstract class ACCollisionSpriteAABB : ACCollisionSprite, ISpriteCollideableAABB
    {
        public override COLLISION_TYPE CollisionType
        {
            get { return COLLISION_TYPE.AABB; }
        }

        public Vector2 RectangleCenter
        {
            get { return position; }
        }

        protected Vector2 rectangleHalfSize;
        public Vector2 RectangleHalfSize
        {
            get { return rectangleHalfSize; }
        }
    }
}
