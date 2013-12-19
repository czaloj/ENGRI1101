using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BlisterUI
{
    public class FalseFirstScreen : GameScreen
    {
        protected int nS;
        protected bool doNext;
        public override int Next
        {
            get
            {
                if (doNext) { return Index; }
                doNext = true;
                return nS;
            }
            protected set { nS = value; }
        }
        public override int Previous { get; protected set; }

        public FalseFirstScreen(int nextScreen)
        {
            doNext = false;
            Next = nextScreen;
            Previous = ScreenList.NoScreen;
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
            return;
        }
        public override void draw(GameTime gameTime)
        {
            game.GraphicsDevice.Clear(Color.Black);
        }
    }
}
