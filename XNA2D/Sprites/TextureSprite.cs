using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XNA2D.Framework;
using XNA2D.Interfaces;
using XNA2D.Sprites.Abstract;

namespace XNA2D.Sprites
{
    public class TextureSprite : ACVisibleSprite
    {
        public override void build(ContentManager content)
        {
            setSpriteTexture(XNA2DProcessor.getSpriteTexture(textureID));
        }
    }
}
