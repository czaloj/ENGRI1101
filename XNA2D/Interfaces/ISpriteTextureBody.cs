using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA2D.Interfaces
{
    public interface ISpriteTextureBody
    {
        Vector2 TextureSize { get; set; }
        Vector2 PivotCenterOffset { get; set; }
    }
}
