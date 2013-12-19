using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using XNA3D.Cameras;
using XNA3D.Objects.Graphics;

namespace XNA3D.Objects.Interfaces
{
    public interface I3DDrawable
    {
        bool IsDrawable { get; }
        bool IsVisible { get; }

        //void draw(ACCamera camera, ModelEffect effect);
        void draw(GraphicsDevice g, ACCamera camera, Effect e);
        void setVisible(bool b);
    }
}
