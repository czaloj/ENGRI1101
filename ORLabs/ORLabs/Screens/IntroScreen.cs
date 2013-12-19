using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BlisterUI;

namespace ORLabs.Screens
{
    public class IntroScreen : GameScreen
    {
        int nextScreen;
        int[] labScreenIndices;
        public override int Next
        {
            get { return nextScreen; }
            protected set { }
        }
        public override int Previous
        {
            get { return 0; }
            protected set { }
        }

        Color backgroundColor;

        public IntroScreen(params int[] labScreens)
        {
            backgroundColor = new Color(12, 12, 0, 255);

            labScreenIndices = new int[labScreens.Length];
            Array.Copy(labScreens, labScreenIndices, labScreens.Length);
            nextScreen = labScreenIndices[0];
        }

        public override void build()
        {
        }
        public override void destroy(GameTime gameTime)
        {
        }

        public override void onEntry(GameTime gameTime)
        {
        }
        public override void onExit(GameTime gameTime)
        {
        }

        public override void update(GameTime gameTime)
        {
            State = ScreenState.ChangeNext;
            //game.nextScreen(gameTime);
        }
        public override void draw(GameTime gameTime)
        {
            game.GraphicsDevice.Clear(backgroundColor);
        }

    }
}
