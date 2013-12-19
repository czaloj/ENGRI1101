using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;
using XNA3D.Graphics;

namespace ORLabs.Graphics.Widgets
{
    public class SimpleText : IMTransVisible
    {
        protected WidgetFrame mw;
        public WidgetFrame World
        {
            get { return mw; }
            set
            {
                mw = value;
            }
        }

        bool visible;
        public bool IsVisible
        {
            get { return visible; }
        }

        public Color Color;
        SpriteFont font;
        string text;
        public string Text { get { return text; } }

        public SimpleText(SpriteFont f)
        {
            visible = false;
            font = f;
        }
        public void setText(string s)
        {
            text = s;
        }
        public void setVisible(bool b)
        {
            visible = b;
        }
        public void draw(SpriteBatch batch)
        {
            if (IsVisible)
            {
                batch.DrawString(font, text, mw.Position, Color, mw.Rotation, Vector2.Zero, mw.Scaling, SpriteEffects.None, mw.Depth);
            }
        }
    }
}