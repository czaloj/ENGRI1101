using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace XNA2D.Interfaces
{
    public interface ISpriteVisible : ISpriteTextureHolder
    {
        bool Visible { get; set; }
        float LayerDepth { get; set; }

        void draw(SpriteBatch batch);

        void setSpriteFlipping(SpriteEffects spriteFlipping);
        SpriteEffects getSpriteFlipping();
    }
}
