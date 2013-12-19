using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA2D.Framework;

namespace XNA2D.Interfaces
{
    public interface ISpriteTextureHolder : ISpriteTextureBody
    {
        int TextureID { get; set; }
        Color SpriteColor { get; set; }

        bool hasSpriteTexture();
        void setSpriteTexture(SpriteTexture texture);
        SpriteTexture getSpriteTexture();
    }
}
