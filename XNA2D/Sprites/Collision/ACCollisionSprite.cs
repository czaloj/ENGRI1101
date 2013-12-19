using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using XNA2D.Sprites.Abstract;
using XNA2D.Interfaces.Collision;
using XNA2D.Framework.Collision;

namespace XNA2D.Sprites.Collision
{
    public abstract class ACCollisionSprite : ACVisibleSprite, ISpriteCollideable
    {
        public abstract COLLISION_TYPE CollisionType { get; }
    }
}
