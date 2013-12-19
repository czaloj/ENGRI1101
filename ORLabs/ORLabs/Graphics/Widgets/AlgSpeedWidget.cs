using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;
using XNA3D.Graphics;
using ORLabs.Framework;
using ORLabs.Screens;

namespace ORLabs.Graphics.Widgets
{
    public class AlgSpeedWidget : IMTransVisible
    {
        private Texture2D pixel;
        private Frame backFrame;
        private Frame slideExtents;
        private HoverFrame slider;
        private bool isMoving;
        private MTransVisibleList slm;
        private Color csSlow, csFast, csHover;

        public float Percent { get; private set; }
        public event Action<float> OnNewSpeed;

        public AlgSpeedWidget(Texture2D pixelTex, Info info)
        {
            isMoving = false;
            pixel = pixelTex;

            WidgetFrame wfSlideExtents = new WidgetFrame((info.BackFrameSize - info.SliderExtentSize) / 2f, 0, Vector2.One, 0);
            WidgetFrame wfSOff = new WidgetFrame(new Vector2(0, -(info.SliderSize.Y - info.SliderExtentSize.Y) / 2f), 0);

            backFrame = new Frame();
            Frame.create<Frame>(ref backFrame, WidgetFrame.Identity, info.BackFrameSize, info.CBackFrame);
            slideExtents = new Frame();
            Frame.create<Frame>(ref slideExtents, WidgetFrame.Identity, info.SliderExtentSize, info.CSEFrame);
            slider = new HoverFrame();
            HoverFrame.create<HoverFrame>(ref slider, WidgetFrame.Identity, info.SliderSize, info.CSSlow, info.CSHover);
            
            csSlow = info.CSSlow; csFast = info.CSFast; csHover = info.CSHover;

            slm = new MTransVisibleList(
                new MTVLBinding(slider, wfSOff)
                );
            ml = new MTransVisibleList(
                new MTVLBinding(backFrame, WidgetFrame.Identity),
                new MTVLBinding(slideExtents, wfSlideExtents),
                new MTVLBinding(slm, wfSlideExtents)
                );

            setSliderPercent(0f);
        }

        public void hook()
        {
            slider.hook();
            isMoving = false;
            MouseEventDispatcher.OnMousePress += onMousePress;
            MouseEventDispatcher.OnMouseRelease += onMouseRelease;
            MouseEventDispatcher.OnMouseMotion += onMouseMovement;
        }
        public void unhook()
        {
            slider.unhook();
            isMoving = false;
            MouseEventDispatcher.OnMousePress -= onMousePress;
            MouseEventDispatcher.OnMouseRelease -= onMouseRelease;
            MouseEventDispatcher.OnMouseMotion -= onMouseMovement;
        }
        public void onMousePress(Vector2 pos, MOUSE_BUTTON b)
        {
            if (b == MOUSE_BUTTON.LEFT_BUTTON && !isMoving && slider.IsHovered && IsVisible)
            {
                isMoving = true;
                slider.unhook();
            }
        }
        public void onMouseRelease(Vector2 pos, MOUSE_BUTTON b)
        {
            if (b == MOUSE_BUTTON.LEFT_BUTTON && isMoving && IsVisible)
            {
                isMoving = false;
                if (OnNewSpeed != null) { OnNewSpeed(Percent); }
                slider.hook();
            }
        }
        public void onMouseMovement(Vector2 pos, Vector2 dis)
        {
            if (isMoving && IsVisible)
            {
                Vector2 tp;
                tp = WidgetFrame.InvTransform(pos, slideExtents.Transform.Frame);
                Vector2 d = tp;
                setSliderPercent(MathHelper.Clamp(d.X / slideExtents.Transform.RectSize.X, 0, 1));
                slider.checkHover(pos, dis);
            }
        }

        public void enable()
        {
            hook();
            isMoving = false;
        }
        public void disable()
        {
            unhook();
            setSliderPercent(0);
        }

        public void setSliderPercent(float p)
        {
            WidgetFrame curOffset = ml.Children[2].Offset;
            curOffset.Position.X = p * slideExtents.Size.X;
            ml.Children[2].Offset = curOffset;
            ml.World = ml.World;
            slider.setColors(Color.Lerp(csSlow, csFast, p), csHover);
            Percent = p;
            if (OnNewSpeed != null) { OnNewSpeed(Percent); }
        }

        public void draw(SpriteBatch batch)
        {
            if (IsVisible)
            {
                backFrame.draw(batch);
                slideExtents.draw(batch);
                slider.draw(batch);
            }
        }

        private MTransVisibleList ml;
        public WidgetFrame World
        {
            get
            {
                return ml.World;
            }
            set
            {
                ml.World = value;
            }
        }

        public bool IsVisible
        {
            get { return ml.IsVisible; }
        }
        public void setVisible(bool b)
        {
            ml.setVisible(b);
        }

        public struct Info
        {
            public Vector2 BackFrameSize;
            public Vector2 SliderExtentSize;
            public Vector2 SliderSize;
            public Color
                CBackFrame,
                CSEFrame,
                CSSlow, CSFast, CSHover;
        }
    
    }
}
