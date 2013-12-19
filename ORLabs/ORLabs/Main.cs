using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using BlisterUI;
using ORLabs.Screens;
using ORLabs.Framework;
using ORLabs.Graphics.Widgets;
using ORLabs.Algorithms;

namespace ORLabs
{
    public class Main : MainGame
    {
        public readonly StartScreen ScrStart;

        public readonly Lab2 ScrLab2;
        public readonly Lab3 ScrLab3;
        public readonly Lab4 ScrLab4;
        public readonly Lab5 ScrLab5;
        public readonly MGScreen ScrMG;
        
        public readonly CreationScreen ScrCreation;
        public readonly HelpScreen ScrHelp;


        public Main()
            : base()
        {
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            Window.AllowUserResizing = true;

            // Menu Screen
            ScrStart = new StartScreen();

            //Labs
            ScrLab2 = new Lab2();
            ScrLab3 = new Lab3();
            ScrLab4 = new Lab4();
            ScrLab5 = new Lab5();
            ScrMG = new MGScreen();

            // Helper Screens
            ScrCreation = new CreationScreen();
            ScrHelp = new HelpScreen();
        }

        protected override void FullInitialize()
        {
            IsMouseVisible = true;
        }
        protected override void FullLoad()
        {
            ORLabs.Graphics.Widgets.HoverFrame.build(GraphicsDevice);
            GraphGrid.loadEffect(@"Effects\Grid", Content);
            GraphGrid.createTexture(GraphicsDevice);
            BlisterUI.Input.WMHookInput.Initialize(Window);
        }

        protected override void buildScreenList()
        {
            screenList = new ScreenList(this, 0,
                new FalseFirstScreen(1),
                ScrStart,
                ScrLab2,
                ScrLab3,
                ScrLab4,
                ScrLab5,
                ScrMG,
                ScrCreation,
                ScrHelp
                );
        }

        protected override void FullQuit()
        {
            base.FullQuit();
            GraphScreen.terminateGIO();
        }
    }
}
