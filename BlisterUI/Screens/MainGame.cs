using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BlisterUI
{
    public abstract class MainGame : Game
    {
        protected GraphicsDeviceManager graphics;
        public GraphicsDeviceManager Graphics { get { return graphics; } }
        protected SpriteBatch spriteBatch;
        public SpriteBatch SpriteBatch { get { return spriteBatch; } }

        protected IGameScreen screen;
        protected ScreenList screenList;
        private GameTime lastTime;

        public MainGame()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            Exiting += (s, e) => { FullQuit(); };
        }

        protected abstract void buildScreenList();
        protected abstract void FullInitialize();
        protected abstract void FullLoad();

        protected override void Initialize()
        {
            base.Initialize();
            FullInitialize();
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            FullLoad();

            buildScreenList();
            screen = screenList.Current;
        }
        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            lastTime = gameTime;
            if (screen != null)
            {
                switch (screen.State)
                {
                    case ScreenState.Running:
                        screen.update(gameTime);
                        break;
                    case ScreenState.ChangeNext:
                        screen.onExit(gameTime);
                        screen = screenList.Next;
                        if (screen != null)
                        {
                            screen.setRunning();
                            screen.onEntry(gameTime);
                        }
                        break;
                    case ScreenState.ChangePrevious:
                        screen.onExit(gameTime);
                        screen = screenList.Previous;
                        if (screen != null)
                        {
                            screen.setRunning();
                            screen.onEntry(gameTime);
                        }
                        break;
                    case ScreenState.ExitApplication:
                        FullQuit();
                        return;
                }
                base.Update(gameTime);
            }
            else
            {
                FullQuit();
            }

        }
        protected override void Draw(GameTime gameTime)
        {
            if (screen != null && screen.State == ScreenState.Running)
            {
                screen.draw(gameTime);
            }
            base.Draw(gameTime);
        }

        protected virtual void FullQuit()
        {
            if (screen != null)
            {
                screen.onExit(lastTime);
            }
            screenList.destroy(lastTime);
            Exit();
        }
    }
}
