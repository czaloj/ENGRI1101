using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlisterUI
{
    public enum ScreenState
    {
        Running,
        ExitApplication,
        ChangeNext,
        ChangePrevious
    }

    public interface IGameScreen
    {
        ScreenState State { get; }
        int Index { get; }
        int Next { get; }
        int Previous { get; }
        MainGame ParentGame { get; }

        void setParentGame(MainGame pgame, int index);

        /// <summary>
        /// Called When Screen Is Brought Forth As Main Game Screen
        /// </summary>
        void build();
        /// <summary>
        /// Called When New Screen Must Be Brought Up
        /// </summary>
        void destroy(GameTime gameTime);

        void setRunning();
        void onEntry(GameTime gameTime);
        void onExit(GameTime gameTime);

        /// <summary>
        /// Called During Game Update
        /// </summary>
        /// <param name="gameTime">Game Time</param>
        void update(GameTime gameTime);
        /// <summary>
        /// Called During Game Draw
        /// </summary>
        /// <param name="gameTime"></param>
        void draw(GameTime gameTime);
    }
    public abstract class GameScreen : IGameScreen
    {
        public ScreenState State { get; protected set; }
        public int Index { get; private set; }
        public abstract int Next { get; protected set; }
        public abstract int Previous { get; protected set; }
        public MainGame ParentGame
        {
            get { return game; }
        }
        protected MainGame game;

        public void setParentGame(MainGame pgame, int index)
        {
            game = pgame;
            Index = index;
        }

        public abstract void build();
        public abstract void destroy(GameTime gameTime);

        public void setRunning()
        {
            State = ScreenState.Running;
        }
        public abstract void onEntry(GameTime gameTime);
        public abstract void onExit(GameTime gameTime);

        public abstract void update(GameTime gameTime);
        public abstract void draw(GameTime gameTime);
    }

    #region Type-Clarified Game
    public interface IGameScreen<T> : IGameScreen
        where T : MainGame
    {
        new T ParentGame { get; }
        void setParentGame(T pgame, int index);
    }
    public abstract class GameScreen<T> : IGameScreen<T>
        where T : MainGame
    {
        public ScreenState State { get; protected set; }
        public int Index { get; private set; }
        public abstract int Next { get; protected set; }
        public abstract int Previous { get; protected set; }
        MainGame IGameScreen.ParentGame
        {
            get { throw new NotImplementedException(); }
        }
        public T ParentGame
        {
            get { return game; }
        }
        protected T game;

        public void setParentGame(T pgame, int index)
        {
            game = pgame;
            Index = index;
        }
        void IGameScreen.setParentGame(MainGame pgame, int index)
        {
            game = pgame as T;
            Index = index;
        }

        public abstract void build();
        public abstract void destroy(GameTime gameTime);

        public void setRunning()
        {
            State = ScreenState.Running;
        }
        public abstract void onEntry(GameTime gameTime);
        public abstract void onExit(GameTime gameTime);

        public abstract void update(GameTime gameTime);
        public abstract void draw(GameTime gameTime);
    }
    #endregion
}
