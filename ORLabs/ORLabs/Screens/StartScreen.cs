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

namespace ORLabs.Screens
{
    public class StartScreen : GameScreen<Main>
    {
        public override int Next
        {
            get { return nextScreen; }
            protected set { }
        }
        public override int Previous
        {
            get { return -1; }
            protected set { }
        }

        SelectionList<int> sl;
        SpriteFont font;
        int nextScreen;

        public StartScreen()
        {
            nextScreen = -1;
        }

        public override void build()
        {
            font = game.Content.Load<SpriteFont>(@"Fonts\FontStartMenu");
            sl = new SelectionList<int>(font, game.GraphicsDevice, "StartList",
                new SelectionList<int>.Options()
                {
                    FrameSize = new Vector2(1600, font.LineSpacing * 3 / 2),
                    CText = Color.White,
                    CBase = Color.Transparent,
                    CHovered = new Color(12, 12, 12, 255),
                    TextDisplace = new Vector3(70, 0, 0)
                },
                new SelectionList<int>.Choice(0, "Lab 2", game.ScrLab2.Index, 10),
                new SelectionList<int>.Choice(1, "Lab 3", game.ScrLab3.Index, 10),
                new SelectionList<int>.Choice(2, "Lab 4", game.ScrLab4.Index, 10),
                new SelectionList<int>.Choice(3, "Lab 5", game.ScrLab5.Index, 10),
                new SelectionList<int>.Choice(4, "Lab 13", game.ScrMG.Index, 10),
                new SelectionList<int>.Choice(5, "Builder", game.ScrCreation.Index, 10),
                new SelectionList<int>.Choice(6, "Options", game.ScrHelp.Index, 10)
                );
            sl.World = new WidgetFrame(new Vector2(-60, 40), 0.5f, -0.2f);
            sl.OnChoice += (c) =>
            {
                nextScreen = c.Data;
                State = ScreenState.ChangeNext;
            };
        }
        public override void destroy(GameTime gameTime)
        {
        }

        public override void onEntry(GameTime gameTime)
        {
            nextScreen = -1;
            sl.setVisible(true);
            sl.hook(true);
        }
        public override void onExit(GameTime gameTime)
        {
            sl.setVisible(false);
            sl.hook(false);
        }

        public override void update(GameTime gameTime)
        {
        }
        public override void draw(GameTime gameTime)
        {
            game.GraphicsDevice.Clear(Color.Black);
            game.SpriteBatch.Begin();
            sl.draw(game.SpriteBatch);
            game.SpriteBatch.End();
        }
    }
}
