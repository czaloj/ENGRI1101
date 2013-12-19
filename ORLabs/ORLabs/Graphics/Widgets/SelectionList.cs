using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;
using ORLabs.Graphics;
using ORLabs.Graphics.Widgets;

namespace ORLabs.Graphics.Widgets
{
    public class SelectionList<T> : IMTransVisible
    {
        public delegate void ChoiceEvent(Choice c);

        public WidgetFrame World
        {
            get { return fullPanel.World; }
            set { fullPanel.World = value; }
        }

        private MTransVisibleList textPanel, framePanel, fullPanel;
        public bool IsVisible { get { return fullPanel.IsVisible; } }

        public Vector2 FullSize
        {
            get { return new Vector2(frameL[0].Size.X, frameL[0].Size.Y * frameL.Length); }
        }

        HoverFrame[] frameL;
        SimpleText[] textL;
        Choice[] choices;
        public Choice this[int i]
        {
            get { return choices[i]; }
        }

        SpriteFont font;

        Choice HoveredChoice;
        public event ChoiceEvent OnChoice;

        public SelectionList(SpriteFont f, GraphicsDevice g, string batchName, Options opt, params Choice[] c)
        {
            font = f;
            choices = c;
            frameL = new HoverFrame[choices.Length];
            textL = new SimpleText[choices.Length];
            int max = 0;
            foreach (Choice ch in choices) { max += ch.MaxLength; }

            for (int i = 0; i < choices.Length; i++)
            {
                frameL[i] = new HoverFrame();
                HoverFrame.create<HoverFrame>(ref frameL[i], WidgetFrame.Identity, opt.FrameSize, opt.CBase, opt.CHovered);
                frameL[i].setVisible(false);
                var ii = i;
                frameL[i].OnHoverUpdate += (s, b) =>
                {
                    if (b)
                    {
                        HoveredChoice = choices[ii];
                    }
                    else if (HoveredChoice.Equals(choices[ii]))
                    {
                        HoveredChoice = Choice.None;
                    }
                };

                textL[i] = new SimpleText(f);
                textL[i].Color = opt.CText;
                textL[i].setText(choices[i].Text);
            }
            MTVLBinding[] b1 = new MTVLBinding[choices.Length], b2 = new MTVLBinding[choices.Length];
            for (int i = 0; i < choices.Length; i++)
            {
                b1[i] = new MTVLBinding(frameL[i], new WidgetFrame(new Vector2(0, opt.FrameSize.Y * i), 0));
            }
            framePanel = new MTransVisibleList(b1);
            for (int i = 0; i < choices.Length; i++)
            {
                b2[i] = new MTVLBinding(textL[i], new WidgetFrame(new Vector2(0, opt.FrameSize.Y * i), 0));
            }
            textPanel = new MTransVisibleList(b2);
            fullPanel = new MTransVisibleList(
                new MTVLBinding(framePanel, WidgetFrame.Identity),
                new MTVLBinding(textPanel, new WidgetFrame(new Vector2(opt.TextDisplace.X, opt.TextDisplace.Y), 0))
                );
        }

        public void setVisible(bool b)
        {
            fullPanel.setVisible(b);
            if (IsVisible) { hook(true); }
            else { hook(false); }
        }

        public void hook(bool b)
        {
            if (b)
            {
                MouseEventDispatcher.OnMousePress += checkChoices;
                foreach (var f in frameL) { f.hook(); }
            }
            else
            {
                MouseEventDispatcher.OnMousePress -= checkChoices;
                foreach (var f in frameL) { f.unhook(); }
            }
        }

        void checkChoices(Vector2 l, MOUSE_BUTTON b)
        {
            if (HoveredChoice.Text != null && !HoveredChoice.Text.Equals("None") && OnChoice != null)
            {
                if (!frameL[HoveredChoice.AIndex].IsHovered) { return; }
                OnChoice(HoveredChoice);
            }
        }

        public void draw(SpriteBatch batch)
        {
            for (int i = 0; i < frameL.Length; i++)
            {
                frameL[i].draw(batch);
                textL[i].draw(batch);
            }
        }

        public struct Choice
        {
            public static readonly Choice None = new Choice(-1, "None", default(T), 4);

            public int AIndex;
            public string Text;
            public int MaxLength;
            public T Data;

            public Choice(int ai, string text, T data, int max = 100)
            {
                AIndex = ai;
                Text = text;
                Data = data;
                MaxLength = max;
            }
        }
        public struct Options
        {
            public Vector2 FrameSize;
            public Color CBase, CHovered, CText;
            public Vector3 TextDisplace;
        }
    }
}
