using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ORLabs.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAGUI.Input.Hooks;
using XNAGUI.Input.Keyboard;
using XNAGUI.Input.Mouse;
using XNAGUI.Graphics;
using ORLabs.Screens;

using System.Windows.Forms;
using System.Threading;
using KeyEventArgs = XNAGUI.Input.Hooks.KeyEventArgs;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace ORLabs.Graphics.Widgets
{
    public class GraphIOWidget : IMTransVisible
    {
        TextEntry[] fileEntries;
        HoverFrame[] entrySearchButtons;
        Thread tOpenFile;
        void openFile()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.ShowDialog();
                fopened = ofd.FileName;
            }
        }
        string fopened;

        public bool IsActive
        {
            get
            {
                return
                    fileEntries[0].IsWriting ||
                    fileEntries[1].IsWriting ||
                    fileEntries[2].IsWriting
                    ;
            }
        }

        HoverFrame actionListButton;
        SelectionList<int> graphActions;
        MTransVisibleList actionList;

        IORGraphHolder graphHolder;

        private ORGraph graph;
        public ORGraph Graph
        {
            get { return graph; }
        }

        public event Action<ORGraph> OnGraphLoad;
        public event Action OnButtonClick;

        public GraphIOWidget(IORGraphHolder gh, string name, string font, int fs, GraphicsDevice g, Vector2 size)
        {
            graphHolder = gh;
            graph = null;
            build(BMFont.fromFamily(font, g, fs, BMFont.DefaultCharRegion, 20, BMFont.DefaultColor), name, g, size);
        }
        public void hook()
        {
            graphActions.hook(true);
            actionListButton.hook();
            entrySearchButtons[0].hook();
            entrySearchButtons[1].hook();
            entrySearchButtons[2].hook();
            KeyboardEventDispatcher.OnKeyPressed += onKeyPress;
            MouseEventDispatcher.OnMousePress += onMousePress;
        }
        public void unhook()
        {
            graphActions.hook(false);
            actionListButton.unhook();
            entrySearchButtons[0].unhook();
            entrySearchButtons[1].unhook();
            entrySearchButtons[2].unhook();
            KeyboardEventDispatcher.OnKeyPressed -= onKeyPress;
            MouseEventDispatcher.OnMousePress -= onMousePress;
        }

        void build(BMFont font, string name, GraphicsDevice g, Vector2 size)
        {
            Color
                cHiddenBase = Color.Black,
                cHighlight = new Color(12, 12, 12, 255),
                cText = new Color(0, 180, 180, 255),
                cSelected = new Color(20, 30, 30, 255)
                ;

            size.Y = 2 * font.Size;
            graphActions = new SelectionList<int>(font, g, name + " - Actions",
                new SelectionList<int>.Options()
                {
                    CBase = cHiddenBase,
                    CHovered = cHighlight,
                    CText = cText,
                    FrameSize = size,
                    TextDisplace = new Vector3(3, 0, 0)
                },
                new SelectionList<int>.Choice("Save Graph", 0, 20),
                new SelectionList<int>.Choice("Load Graph", 1, 20)
                );
            actionListButton = new HoverFrame();
            HoverFrame.create<HoverFrame>(
                g, ref actionListButton,
                Vector2.Zero, new Vector2(12, graphActions.FullSize.Y),
                Matrix.Identity, cHiddenBase, cText
                );

            HoverFrame.BuildOptions hbo = 
                new HoverFrame.BuildOptions(
                    Vector2.Zero, 
                    new Vector2(graphActions.FullSize.X, font.Size),
                    cHiddenBase, cHighlight
                );
            fileEntries = new TextEntry[3];
            fileEntries[0] = 
                new TextEntry(font, name + " - Action File Node", 100,
                    g, new Vector2(2, 0), hbo,
                    cSelected, cText, Matrix.Identity
                    );
            fileEntries[0].GhostText = "Node File:";
            fileEntries[1] =
                new TextEntry(font, name + " - Action File Edge", 100,
                    g, new Vector2(2, 0), hbo,
                    cSelected, cText, Matrix.Identity
                    );
            fileEntries[1].GhostText = "Edge File:";
            fileEntries[2] =
                new TextEntry(font, name + " - Action File Grid", 100,
                    g, new Vector2(2, 0), hbo,
                    cSelected, cText, Matrix.Identity
                    );
            fileEntries[2].GhostText = "Grid File:";
            entrySearchButtons = new HoverFrame[3];
            entrySearchButtons[0] = new HoverFrame();
            HoverFrame.create<HoverFrame>(
                g, ref entrySearchButtons[0],
                new Vector2(graphActions.FullSize.X, 0), new Vector2(12, font.Size),
                Matrix.Identity, cHiddenBase, cText
                );
            entrySearchButtons[1] = new HoverFrame();
            HoverFrame.create<HoverFrame>(
                g, ref entrySearchButtons[1],
                new Vector2(graphActions.FullSize.X, 0), new Vector2(12, font.Size),
                Matrix.Identity, cHiddenBase, cText
                );
            entrySearchButtons[2] = new HoverFrame();
            HoverFrame.create<HoverFrame>(
                g, ref entrySearchButtons[2],
                new Vector2(graphActions.FullSize.X, 0), new Vector2(12, font.Size),
                Matrix.Identity, cHiddenBase, cText
                );

            actionList = new MTransVisibleList(
                new MTVLBinding(graphActions, Matrix.Identity),
                new MTVLBinding(actionListButton, Matrix.CreateTranslation(graphActions.FullSize.X, 0, 0)),
                new MTVLBinding(fileEntries[0], Matrix.CreateTranslation(0, graphActions.FullSize.Y + font.Size * 0, 0)),
                new MTVLBinding(fileEntries[1], Matrix.CreateTranslation(0, graphActions.FullSize.Y + font.Size * 1, 0)),
                new MTVLBinding(fileEntries[2], Matrix.CreateTranslation(0, graphActions.FullSize.Y + font.Size * 2, 0)),
                new MTVLBinding(entrySearchButtons[0], Matrix.CreateTranslation(0, graphActions.FullSize.Y + font.Size * 0, 0)),
                new MTVLBinding(entrySearchButtons[1], Matrix.CreateTranslation(0, graphActions.FullSize.Y + font.Size * 1, 0)),
                new MTVLBinding(entrySearchButtons[2], Matrix.CreateTranslation(0, graphActions.FullSize.Y + font.Size * 2, 0))
                );
            actionList.setVisible(false);

            graphActions.OnChoice += updateChoice;
        }

        void updateChoice(SelectionList<int>.Choice c)
        {
            ORGraphFile glf;
            switch (c.Data)
            {
                case 0: // Saving
                    glf = new ORGraphFile(
                        fileEntries[0].Text,
                        fileEntries[1].Text,
                        fileEntries[2].Text,
                        graphHolder.Graph
                        );
                    deactivate();
                    try
                    {
                        glf.write();
                    }
                    catch (Exception)
                    {
                    }
                    break;
                case 1: // Loading
                    glf = new ORGraphFile(
                        fileEntries[0].Text,
                        fileEntries[1].Text,
                        fileEntries[2].Text
                        );
                    try
                    {
                        //Load From The File
                        glf.read();
                        graph = glf.Graph;
                        if (OnGraphLoad != null) { OnGraphLoad(graph); }
                        deactivate();
                    }
                    catch (Exception)
                    {

                    }
                    break;
            }
        }

        public void onKeyPress(object sender, KeyEventArgs args)
        {
            switch (args.KeyCode)
            {
                case Keys.Enter:
                    //if (ioLoc == 2 && IsActive && entryNode.Text.Equals(entryNode.GhostText))
                    //{
                    //    updateText(null);
                    //}
                    break;
            }
        }
        public void onMousePress(Vector2 loc, MOUSE_BUTTON button)
        {
            if (actionListButton.IsHovered && OnButtonClick != null)
            {
                OnButtonClick();
            }
            if (entrySearchButtons[0].IsHovered)
            {
                tOpenFile = new Thread(openFile);
                tOpenFile.TrySetApartmentState(ApartmentState.STA);
                tOpenFile.Start();
                while (tOpenFile.IsAlive) { Thread.Sleep(100); }
                if (fopened != null) { fileEntries[0].Text = fopened; }
            }
            if (entrySearchButtons[1].IsHovered)
            {
                tOpenFile = new Thread(openFile);
                tOpenFile.TrySetApartmentState(ApartmentState.STA);
                tOpenFile.Start();
                while (tOpenFile.IsAlive) { Thread.Sleep(100); }
                if (fopened != null) { fileEntries[1].Text = fopened; }
            }
            if (entrySearchButtons[2].IsHovered)
            {
                tOpenFile = new Thread(openFile);
                tOpenFile.TrySetApartmentState(ApartmentState.STA);
                tOpenFile.Start();
                while (tOpenFile.IsAlive) { Thread.Sleep(100); }
                if (fopened != null) { fileEntries[2].Text = fopened; }
            }
        }

        public void deactivate()
        {
            //ioLoc = 0;
            //ioFlag = Flags.NoFlags;
            //entryNode.GhostText = "Choose IO Function";
            //entryNode.IsWriting = false;
        }

        public Matrix World
        {
            get
            {
                return actionList.World;
            }
            set
            {
                actionList.World = value;
            }
        }
        public bool IsVisible
        {
            get { return actionList.IsVisible; }
        }
        public void setVisible(bool b)
        {
            actionList.setVisible(b);
        }
    }
}
