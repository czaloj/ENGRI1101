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

namespace ORLabs.Graphics.Widgets {
    public class AlgSpeedWidget : IMTransVisible {
        private Texture2D pixel, pause, play, next;
        private Frame backFrame;
        private Frame slideExtents;
        private HoverFrame slider, pauseButton, nextButton;
        private bool isMoving;
        private MTransVisibleList slm;
        private Color csSlow, csFast, csHover;
        bool paused;

        public float Percent { get; private set; }
        public event Action<float> OnNewSpeed;
        public event Action OnNextStep;

        public AlgSpeedWidget(Texture2D[] t, Info info) {
            isMoving = false;
            paused = true;
            pixel = t[0];
            pause = t[1];
            play = t[2];
            next = t[3];

            WidgetFrame wfSlideExtents = new WidgetFrame((info.BackFrameSize - info.SliderExtentSize) / 2f, 0, Vector2.One, 0);
            WidgetFrame wfSOff = new WidgetFrame(new Vector2(0, -(info.SliderSize.Y - info.SliderExtentSize.Y) / 2f), 0);

            backFrame = new Frame();
            Frame.create<Frame>(ref backFrame, WidgetFrame.Identity, info.BackFrameSize, info.CBackFrame);
            slideExtents = new Frame();
            Frame.create<Frame>(ref slideExtents, WidgetFrame.Identity, info.SliderExtentSize, info.CSEFrame);
            slider = new HoverFrame();
            HoverFrame.create<HoverFrame>(ref slider, WidgetFrame.Identity, info.SliderSize, info.CSSlow, info.CSHover);

            Vector2 square = new Vector2(info.BackFrameSize.Y, info.BackFrameSize.Y);
            pauseButton = new HoverFrame();
            HoverFrame.create<HoverFrame>(ref pauseButton, WidgetFrame.Identity, square, info.CBackFrame, info.CSHover);
            pauseButton.Texture = play;
            nextButton = new HoverFrame();
            HoverFrame.create<HoverFrame>(ref nextButton, WidgetFrame.Identity, square, info.CBackFrame, info.CSHover);
            nextButton.Texture = next;

            csSlow = info.CSSlow; csFast = info.CSFast; csHover = info.CSHover;

            slm = new MTransVisibleList(
                new MTVLBinding(slider, wfSOff)
                );
            ml = new MTransVisibleList(
                new MTVLBinding(backFrame, WidgetFrame.Identity),
                new MTVLBinding(slideExtents, wfSlideExtents),
                new MTVLBinding(slm, wfSlideExtents),
                new MTVLBinding(pauseButton, new WidgetFrame(new Vector2(info.BackFrameSize.X, 0), 0)),
                new MTVLBinding(nextButton, new WidgetFrame(new Vector2(info.BackFrameSize.X + square.X, 0), 0))
                );

            setSliderPercent(0f);
        }

        public void hook() {
            slider.hook();
            pauseButton.hook();
            nextButton.hook();
            isMoving = false;
            MouseEventDispatcher.OnMousePress += onMousePress;
            MouseEventDispatcher.OnMouseRelease += onMouseRelease;
            MouseEventDispatcher.OnMouseMotion += onMouseMovement;
        }
        public void unhook() {
            slider.unhook();
            pauseButton.unhook();
            nextButton.unhook();
            isMoving = false;
            MouseEventDispatcher.OnMousePress -= onMousePress;
            MouseEventDispatcher.OnMouseRelease -= onMouseRelease;
            MouseEventDispatcher.OnMouseMotion -= onMouseMovement;
        }
        public void onMousePress(Vector2 pos, MOUSE_BUTTON b) {
            if(b == MOUSE_BUTTON.LEFT_BUTTON && IsVisible) {
                if(!isMoving && slider.IsHovered) {
                    isMoving = true;
                    slider.unhook();
                }
                else if(pauseButton.IsHovered) {
                    paused = !paused;
                    pauseButton.Texture = paused ? play : pause;
                    if(paused) setSliderPercent(0);
                    else setSliderPercent(Percent);
                }
                else if(nextButton.IsHovered) {
                    if(OnNextStep != null) OnNextStep();
                }
            }
        }
        public void onMouseRelease(Vector2 pos, MOUSE_BUTTON b) {
            if(b == MOUSE_BUTTON.LEFT_BUTTON && isMoving && IsVisible) {
                isMoving = false;
                slider.hook();
            }
        }
        public void onMouseMovement(Vector2 pos, Vector2 dis) {
            if(isMoving && IsVisible) {
                Vector2 tp;
                tp = WidgetFrame.InvTransform(pos, slideExtents.Transform.Frame);
                Vector2 d = tp;
                setSliderPercent(MathHelper.Clamp(d.X / slideExtents.Transform.RectSize.X, 0, 1));
                slider.checkHover(pos, dis);
            }
        }

        public void enable() {
            hook();
            paused = true;
            pauseButton.Texture = play;
            isMoving = false;
            setSliderPercent(0);
        }
        public void disable() {
            unhook();
            paused = true;
            pauseButton.Texture = play;
            isMoving = false;
            setSliderPercent(0);
        }

        public void setSliderPercent(float p) {
            WidgetFrame curOffset = ml.Children[2].Offset;
            curOffset.Position.X = p * slideExtents.Size.X;
            ml.Children[2].Offset = curOffset;
            ml.World = ml.World;
            slider.setColors(Color.Lerp(csSlow, csFast, p), csHover);
            Percent = p;
            float speed = Percent * 0.9f + 0.1f;
            if(OnNewSpeed != null) {
                if(paused) OnNewSpeed(0);
                else OnNewSpeed(speed);
            }
        }

        public void draw(SpriteBatch batch) {
            if(IsVisible) {
                backFrame.draw(batch);
                slideExtents.draw(batch);
                slider.draw(batch);
                pauseButton.draw(batch);
                nextButton.draw(batch);
            }
        }

        private MTransVisibleList ml;
        public WidgetFrame World {
            get {
                return ml.World;
            }
            set {
                ml.World = value;
            }
        }

        public bool IsVisible {
            get { return ml.IsVisible; }
        }
        public void setVisible(bool b) {
            ml.setVisible(b);
        }

        public struct Info {
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
