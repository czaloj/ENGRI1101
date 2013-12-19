using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA3D.Cameras;
using XNA3D.Objects.Graphics;
using XNA3D.Objects.Interfaces;

namespace XNA3D.Objects
{
    public abstract class AC3DDrawable : I3DObject
    {
        protected bool visible = false;
        public bool IsVisible
        {
            get
            {
                return visible;
            }
        }
        protected bool drawable = false;
        public bool IsDrawable
        {
            get
            {
                return drawable;
            }
        }

        protected long id;
        public long ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        public void setID(long ID)
        {
            id = ID;
        }

        public abstract void draw(GraphicsDevice g, ACCamera camera, Effect e);
        public abstract void setVisible(bool b);

        public abstract void build();

        public abstract object Clone();
    }
}
