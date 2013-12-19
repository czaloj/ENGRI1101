using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using XNA2D.Sprites.Collision;

namespace XNA2D.Sprites
{
    public class CircleSprite : ACCollisionSpriteRadial
    {
        public override void build(ContentManager content)
        {
            setSpriteTexture(XNA2DProcessor.getSpriteTexture(textureID));
            collisionRadius = (TextureSize.X + TextureSize.Y) / 4f;
        }
    }
}
