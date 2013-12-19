using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;

namespace ORLabs.Graphics.Widgets
{
    public class AvailableActionsWidget : IMTransVisible
    {
        private Frame backFrame;
        private HoverFrame toggleButton;
        private SimpleText text;
        private MTransVisibleList world;

        public string Text
        {
            get { return text.Text; }
            set { text.setText(value); }
        }
        public int ViewState { get; private set; }
        private float percentExpand;
        public float ExpansionTime { get; set; }
        public BlisterUI.WidgetFrame World
        {
            get
            {
                return world.World;
            }
            set
            {
                world.World = value;
            }
        }
        public WidgetFrame WorldExpanded { get; set; }
        public WidgetFrame WorldHidden { get; set; }
        public bool IsVisible
        {
            get { return world.IsVisible; }
        }
        public Vector2 FrameSize { get { return backFrame.Size; } }

        public AvailableActionsWidget(SpriteFont font, WidgetFrame f, WidgetFrame fe, float te, Vector2 backSize, Vector2 buttonSize, Color cBack, Color cFore)
        {
            backFrame = new Frame();
            toggleButton = new HoverFrame();
            text = new SimpleText(font);
            text.Color = cFore;
            text.setText("");

            Frame.create<Frame>(ref backFrame, WidgetFrame.Identity, backSize, cBack);
            HoverFrame.create<HoverFrame>(ref toggleButton, WidgetFrame.Identity, buttonSize, cBack, cFore);

            world = new MTransVisibleList(
                new MTVLBinding(backFrame, WidgetFrame.Identity),
                new MTVLBinding(toggleButton, new WidgetFrame(new Vector2(backFrame.Size.X, 0), 0)),
                new MTVLBinding(text, new WidgetFrame(new Vector2(3, 3), 0))
                );
            WorldHidden = f;
            WorldExpanded = fe;
            World = WorldHidden;
            world.setVisible(false);
            ViewState = 0;
            ExpansionTime = te;
            percentExpand = 0f;
        }

        public void setVisible(bool b)
        {
            world.setVisible(b);
        }

        private void MouseEventDispatcher_OnMousePress(Vector2 location, MOUSE_BUTTON b)
        {
            if (toggleButton.IsHovered)
            {
                switch (ViewState)
                {
                    case 0:
                    case -1:
                        ViewState = 1; return;
                    case 2:
                    case 1:
                        ViewState = -1; return;
                }
            }
        }
        public void hook()
        {
            toggleButton.hook();
            MouseEventDispatcher.OnMousePress += MouseEventDispatcher_OnMousePress;
        }
        public void unhook()
        {
            toggleButton.unhook();
            MouseEventDispatcher.OnMousePress -= MouseEventDispatcher_OnMousePress;
        }

        public void update(float dt)
        {
            switch (ViewState)
            {
                case 0:
                case 2: return;
                case -1:
                    percentExpand -= dt / ExpansionTime;
                    if (percentExpand <= 0) { ViewState = 0; percentExpand = 0; }
                    World = new WidgetFrame(
                        Vector2.Lerp(WorldHidden.Position, WorldExpanded.Position, percentExpand),
                        MathHelper.Lerp(WorldHidden.Depth, WorldExpanded.Depth, percentExpand),
                        Vector2.Lerp(WorldHidden.Scaling, WorldExpanded.Scaling, percentExpand),
                        MathHelper.Lerp(WorldHidden.Rotation, WorldExpanded.Rotation, percentExpand)
                        );
                    return;
                case 1:
                    percentExpand += dt / ExpansionTime;
                    if (percentExpand >= 1) { ViewState = 2; percentExpand = 1; }
                    World = new WidgetFrame(
                        Vector2.Lerp(WorldHidden.Position, WorldExpanded.Position, percentExpand),
                        MathHelper.Lerp(WorldHidden.Depth, WorldExpanded.Depth, percentExpand),
                        Vector2.Lerp(WorldHidden.Scaling, WorldExpanded.Scaling, percentExpand),
                        MathHelper.Lerp(WorldHidden.Rotation, WorldExpanded.Rotation, percentExpand)
                        );
                    return;
            }
        }

        public void draw(SpriteBatch batch)
        {
            backFrame.draw(batch);
            toggleButton.draw(batch);
            text.draw(batch);
        }
    }
}
