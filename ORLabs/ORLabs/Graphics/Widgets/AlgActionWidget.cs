using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA3D.Graphics;
using XNAGUI.Graphics;
namespace ORLabs.Graphics.Widgets
{
    public sealed class AlgActionWidget
    {
        protected string actionText;
        private BMFont font;
        protected Matrix mWorld;
        UVRectList uvr;

        public const int MaxStringLength = 200;

        public AlgActionWidget(BMFont font, GraphicsDevice g)
        {
            this.font = font;
            uvr = new UVRectList(g, 200);
        }

        public void rebuild(GraphicsDevice g)
        {
            int c;
            UVRectList.Info[] sInfo = font.getStringInfo(actionText, mWorld, 4, Color.Black, out c);
            uvr.clearList();
            uvr.begin();
            uvr.append(g, sInfo);
            uvr.end();
        }
        public void draw(GraphicsDevice g)
        {
            uvr.draw(g);
        }
    }
}
