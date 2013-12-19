using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA2D.Interfaces;

namespace XNA2D.Framework
{
    public struct SpriteFile<T> where T : class, IXNA2DSprite
    {
        public string file;
    }
}
