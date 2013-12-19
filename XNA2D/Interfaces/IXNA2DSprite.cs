using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZLibrary.ADT;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace XNA2D.Interfaces
{
    public interface IXNA2DSprite : ISpriteBody, ISpriteVisible, ICloneable, IIDHolder
    {
        long ID { get; }
        void setID(long ID);

        void build(ContentManager content);
    }
}
