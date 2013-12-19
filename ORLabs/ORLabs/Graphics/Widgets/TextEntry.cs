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
    public class TextEntry : IMTransVisible
    {
        public delegate void Entry(string s);

        public WidgetFrame World
        {
            get { return mtvl.World; }
            set
            {
                mtvl.World = value;
            }
        }
        public bool IsVisible
        {
            get { return mtvl.IsVisible; }
        }
        
        MTransVisibleList mtvl;

        public HoverFrame backFrame;
        public SimpleText textFrame;

        bool writing;
        public bool IsWriting
        {
            get { return writing; }
            set
            {
                if (writing != value)
                {
                    writing = value;
                    if (writing)
                    {
                        KeyboardEventDispatcher.ReceiveChar += onCharEntry;
                        KeyboardEventDispatcher.ReceiveCommand += onCommand;
                        KeyboardEventDispatcher.OnKeyPressed += onPress;
                        backFrame.setColors(cw, cw);
                    }
                    else
                    {
                        KeyboardEventDispatcher.ReceiveChar -= onCharEntry;
                        KeyboardEventDispatcher.ReceiveCommand -= onCommand;
                        KeyboardEventDispatcher.OnKeyPressed -= onPress;
                        backFrame.setColors(cnw, ch);
                        if (string.IsNullOrWhiteSpace(Text))
                        {
                            textFrame.setText(GhostText);
                        }
                    }
                }
            }
        }

        public string Text
        {
            get { return textFrame.Text; }
            set { textFrame.setText(value); }
        }

        public event Entry OnTextEntry;

        Color cw, cnw, ch, tc;
        string ghostText;
        public string GhostText
        {
            get { return ghostText; }
            set
            {
                ghostText = value;
                textFrame.setText(ghostText);
            }
        }

        public TextEntry(SpriteFont f, WidgetFrame frame, Vector2 textOffset, HoverFrame.BuildOptions fOpt, Color writeColor, Color textColor)
        {
            cnw = fOpt.FrameOptions.Base;
            cw = writeColor;
            ch = fOpt.Hover;
            tc = textColor;
            textFrame = new SimpleText(f);
            textFrame.Color = tc;
            GhostText = "Enter Text Here";
            textFrame.setText(GhostText);
            backFrame = new HoverFrame();
            HoverFrame.create<HoverFrame>(ref backFrame, frame, fOpt.FrameOptions.Size, cnw, ch);

            mtvl = new MTransVisibleList(
                new MTVLBinding(textFrame, new WidgetFrame(new Vector2(textOffset.X, textOffset.Y), 0)),
                new MTVLBinding(backFrame, WidgetFrame.Identity)
                );
            writing = false;
        }

        void onPress(object sender, KeyEventArgs args)
        {
            switch (args.KeyCode)
            {
                case Keys.Enter:
                    if (IsWriting)
                    {
                        IsWriting = false;
                        if (!string.IsNullOrEmpty(Text) && !Text.Equals(GhostText) && OnTextEntry != null)
                        {
                            OnTextEntry(Text);
                        }
                    }
                    break;
                case Keys.Back:
                    backspace();
                    break;
            }
        }
        void onCommand(object sender, CharacterEventArgs args)
        {
            switch (args.Character)
            {
                case ControlCharacters.CtrlV:
                    add(KeyboardEventDispatcher.getNewClipboard());
                    break;
                case ControlCharacters.CtrlC:
                    KeyboardEventDispatcher.setToClipboard(textFrame.Text);
                    break;
            }
        }
        void onCharEntry(object sender, CharacterEventArgs args)
        {
            add(args.Character);
        }
        void onMousePress(Vector2 location, MOUSE_BUTTON b)
        {
            if (b == MOUSE_BUTTON.LEFT_BUTTON && backFrame.IsHovered)
            {
                IsWriting = !IsWriting;
            }
        }

        public void add(string s)
        {
            if (Text.Equals(GhostText)) { textFrame.setText(s); return; }
            textFrame.setText(textFrame.Text + s);
        }
        public void add(char c)
        {
            if (Text.Equals(GhostText)) { textFrame.setText(new string(c, 1)); return; }
            textFrame.setText(textFrame.Text + c);
        }
        public void backspace()
        {
            if (string.IsNullOrEmpty(Text)) { return; }
            else if (Text.Equals(GhostText)) { return; }
            string s = textFrame.Text.Substring(0, textFrame.Text.Length - 1);
            if (s.Length <= 0) { textFrame.setText(GhostText); }
            else { textFrame.setText(s); }
        }

        public void setVisible(bool b)
        {
            if (b != IsVisible)
            {
                mtvl.setVisible(b);
                if (IsVisible)
                {
                    MouseEventDispatcher.OnMousePress += onMousePress;
                    backFrame.hook();
                }
                else
                {
                    MouseEventDispatcher.OnMousePress -= onMousePress;
                    backFrame.unhook();
                }
            }
        }
    }
}
