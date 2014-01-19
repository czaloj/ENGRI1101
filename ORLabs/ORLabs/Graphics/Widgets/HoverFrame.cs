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
    public class Frame : IMTransVisible
    {
        #region Static
        private static Texture2D pixel;
        public static void build(GraphicsDevice g)
        {
            pixel = new Texture2D(g, 1, 1);
            pixel.SetData(new Color[] { Color.White });
        }
        public static void create<T>(ref T f, WidgetFrame frame, Vector2 size, Color cb) where T : Frame
        {
            f.Texture = pixel;
            f.visible = false;
            f.transform.Frame = frame;
            f.transform.RectSize = size;
            f.setColor(cb);
        }
        #endregion

        protected WidgetTransform transform = new WidgetTransform();
        public WidgetTransform Transform
        {
            get{return transform;}
        }

        private Texture2D texture;
        private Vector2 tScale;
        public Texture2D Texture {
            private get { return texture; }
            set {
                texture = value;
                tScale = new Vector2(1f / texture.Width, 1f / texture.Height);
            }
        }
        public Vector2 StartLocation
        {
            get { return transform.Frame.Position; }
        }
        public Vector2 Size
        {
            get { return transform.RectSize; }
        }

        private bool visible;
        public bool IsVisible { get { return visible; } }

        protected Color colorBase;
        public virtual Color Color
        {
            get { return colorBase; }
        }

        public virtual WidgetFrame World
        {
            get
            {
                return transform.Frame;
            }
            set
            {
                transform.Frame = value;
            }
        }

        public void setColor(Color b)
        {
            colorBase = b;
        }
        public void setVisible(bool b)
        {
            visible = b;
        }

        public void draw(SpriteBatch batch)
        {
            if (visible)
            {
                batch.Draw(
                    Texture,
                    StartLocation,
                    null,
                    Color,
                    transform.Frame.Rotation,
                    Vector2.Zero,
                    transform.Frame.Scaling * transform.RectSize * tScale,
                    SpriteEffects.None,
                    transform.Frame.Depth
                    );
            }
        }

        public override string ToString()
        {
            return WidgetFrame.Transform(StartLocation, World).ToString();
        }

        public struct BuildOptions
        {
            public Vector2 Location, Size;
            public Color Base;

            public BuildOptions(Vector2 l, Vector2 s, Color cb)
            {
                Location = l;
                Size = s;
                Base = cb;
            }
        }
    }

    public class HoverFrame : Frame
    {
        public delegate void HoverUpdate(HoverFrame sender, bool b);

        private static Vector2 mouseLast;
        static HoverFrame()
        {
            MouseEventDispatcher.OnMouseMotion += (p, d) =>
                {
                    mouseLast = p;
                };
        }
        public static void create<T>(ref T f, WidgetFrame frame, Vector2 size, Color cb, Color ch) where T : HoverFrame
        {
            Frame.create<T>(ref f, frame, size, cb);
            f.colorHover = ch;
            //f.hook();
        }

        protected Color colorHover;
        public override Color Color
        {
            get { return isHovered ? colorHover : colorBase; }
        }

        public override WidgetFrame World
        {
            get { return transform.Frame; }
            set
            {
                transform.Frame = value; ;
                checkHover(mouseLast, Vector2.Zero);
            }
        }

        protected bool isHovered;
        public bool IsHovered
        {
            get { return isHovered; }
        }
        public event HoverUpdate OnHoverUpdate;

        public void hook()
        {
            MouseEventDispatcher.OnMouseMotion += checkHover;
        }
        public void unhook()
        {
            MouseEventDispatcher.OnMouseMotion -= checkHover;
            isHovered = false;
        }

        public bool isInFrame(Vector2 p)
        {
            Vector2 tp = WidgetFrame.InvTransform(p, transform.Frame);
            return
                tp.X >= 0 &&
                tp.X < Size.X &&
                tp.Y >= 0 &&
                tp.Y < Size.Y;
        }
        public Vector2 displaceFromCenter(Vector2 p)
        {
            Vector2 tp;
            tp = WidgetFrame.InvTransform(p, transform.Frame);
            Vector2 d = tp - (StartLocation + Size / 2f);
            tp = WidgetFrame.TransformNormal(d, transform.Frame);
            return tp;
        }
        public void checkHover(Vector2 mPos, Vector2 mDisp)
        {
            bool b = isInFrame(mPos);
            if (b != isHovered)
            {
                isHovered = b;
                if (OnHoverUpdate != null)
                {
                    OnHoverUpdate(this, isHovered);
                }
            }
        }

        public void setColors(Color b, Color h)
        {
            setColor(b);
            colorHover = h;
        }

        new public struct BuildOptions
        {
            public Frame.BuildOptions FrameOptions;
            public Color Hover;

            public BuildOptions(Vector2 l, Vector2 s, Color cb, Color ch)
            {
                FrameOptions = new Frame.BuildOptions(l, s, cb);
                Hover = ch;
            }
        }
    }
}
