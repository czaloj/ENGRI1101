using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;
using ORLabs.Framework;
using ORLabs.Graphics;
using ORLabs.Graphics.Widgets;

namespace ORLabs.Screens
{
    public class MGScreen : GameScreen
    {
        public static float[] cycleLengths = { 0.5f, 0.6f, 0.8f };

        #region Card Drawing
        Matrix mWorldCard;
        Matrix mBackOff;
        VertexPositionTexture[] Card;
        Texture2D tAce, tKing, tBack;
        BasicEffect fxCard;
        bool DrawAce { get { return trueAce; } }
        bool trueAce;
        bool allowUserAction;

        Matrix[] animationCycleM;
        Quaternion[] animationCycleQ;
        int mCycleLocation;
        float mCycleDuration;
        #endregion

        Texture2D pixel;
        SpriteFont font;
        SelectionList<int> actions;
        float pR = -1;
        int pc = 0;

        Random r;

        SimpleText textResults;
        int money;

        LinkedQueue<int> playerActionHistory;

        public override int Next
        {
            get { return 1; }
            protected set { }
        }
        public override int Previous
        {
            get { return 1; }
            protected set { }
        }

        public override void build()
        {
            pixel = new Texture2D(game.GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });
            font = game.Content.Load<SpriteFont>(@"Fonts\Arial16");

            r = new Random();
            mBackOff = Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(new Vector3(0, 0, -0.1f));
            animationCycleM = new Matrix[]
            {
                Matrix.CreateTranslation(0, -400, 0),
                Matrix.CreateTranslation(0, 0, 0),
                Matrix.CreateTranslation(0, 0, 0),
                Matrix.CreateTranslation(0, -400, 0)
            };
            animationCycleQ = new Quaternion[]
            {
                new Quaternion(0, 1, 0, 0),
                new Quaternion(0, 1, 0, 0),
                Quaternion.Identity,
                Quaternion.Identity
            };
            Card = new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(-35, 50, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(35, 50, 0), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(-35, -50, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(35, -50, 0), new Vector2(1, 1))
            };
            using (FileStream s = File.Open("Resources\\Textures\\Ace.png", FileMode.Open))
            { tAce = Texture2D.FromStream(game.GraphicsDevice, s); }
            using (FileStream s = File.Open("Resources\\Textures\\King.png", FileMode.Open))
            { tKing = Texture2D.FromStream(game.GraphicsDevice, s); }
            using (FileStream s = File.Open("Resources\\Textures\\Back.png", FileMode.Open))
            { tBack = Texture2D.FromStream(game.GraphicsDevice, s); }
            fxCard = new BasicEffect(game.GraphicsDevice);

            fxCard.View = Matrix.CreateLookAt(
                new Vector3(0, 0, 100),
                Vector3.Zero,
                Vector3.Up
                );
            fxCard.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver2,
                game.GraphicsDevice.Viewport.AspectRatio,
                0.1f, 300f
                );
            fxCard.LightingEnabled = false;
            fxCard.TextureEnabled = true;
            fxCard.VertexColorEnabled = false;
            fxCard.FogEnabled = false;
            game.Window.ClientSizeChanged += onWindowResize;


            actions = new SelectionList<int>(
                font, game.GraphicsDevice, "MGActions", new SelectionList<int>.Options()
                {
                    CBase = Color.DarkGray,
                    CHovered = Color.DarkGreen,
                    CText = Color.Black,
                    FrameSize = new Vector2(140, 24),
                    TextDisplace = new Vector3(2, 2, 0)
                },
                new SelectionList<int>.Choice(0, "Bet", 0, 40),
                new SelectionList<int>.Choice(1, "Fold", 1, 40),
                new SelectionList<int>.Choice(2, "Continue", 2, 40),
                new SelectionList<int>.Choice(3, "New Game", 3, 40)
                );
            actions.World = WidgetFrame.Identity;
            actions.setVisible(false);
            actions.OnChoice += onActionSelection;

            playerActionHistory = new LinkedQueue<int>();

            textResults = new SimpleText(font);
            textResults.World = new WidgetFrame(new Vector2(0, game.GraphicsDevice.Viewport.Height - font.LineSpacing), 0);
            textResults.Color = Color.White;
            textResults.setVisible(false);
        }
        public override void destroy(GameTime gameTime)
        {
            game.Window.ClientSizeChanged -= onWindowResize;
            tAce.Dispose();
            tKing.Dispose();
            tBack.Dispose();
            fxCard.Dispose();
        }

        void onWindowResize(object sender, EventArgs args)
        {
            fxCard.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver2,
                game.GraphicsDevice.Viewport.AspectRatio,
                0.1f, 300f
                );
            textResults.World = new WidgetFrame(new Vector2(0, game.GraphicsDevice.Viewport.Height - font.LineSpacing), 0);
        }
        public override void onEntry(GameTime gameTime)
        {
            money = 0;
            trueAce = r.Next(1000) < 500;
            actions.setVisible(true);
            textResults.setVisible(true);

            mCycleLocation = 0;
            mCycleDuration = 0f;
            allowUserAction = false;
            mWorldCard = getAnimMatrix(mCycleLocation, mCycleDuration);
            updateResults();

            KeyboardEventDispatcher.OnKeyPressed += onKeyPress;
        }

        void onKeyPress(object sender, KeyEventArgs args)
        {
            switch (args.KeyCode)
            {
                case Keys.P:
                    State = ScreenState.ChangePrevious;
                    //game.previousScreen(null);
                    break;
            }
        }
        public override void onExit(GameTime gameTime)
        {
            KeyboardEventDispatcher.OnKeyPressed -= onKeyPress;
            textResults.setVisible(false);
            actions.setVisible(false);
        }

        public override void update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!allowUserAction) { mCycleDuration += dt; }
            if (mCycleDuration > cycleLengths[mCycleLocation])
            {
                mCycleDuration = 0f;
                mCycleLocation = (mCycleLocation + 1) % cycleLengths.Length;
                switch (mCycleLocation)
                {
                    case 0:
                        //New Card
                        trueAce = r.Next(1000) < 500;
                        break;
                    case 1:
                        //Waiting For Computer's or Player's Decision
                        allowUserAction = computerStrategy(trueAce); // Computer Folded?
                        if (!allowUserAction)
                        {
                            //On Computer Fold
                            money += 1;
                            updateResults();
                        }
                        break;
                    case 2:
                        allowUserAction = true;
                        break;
                }
            }

            mWorldCard = getAnimMatrix(mCycleLocation, mCycleDuration);
        }
        public override void draw(GameTime gameTime)
        {
            game.GraphicsDevice.Clear(Color.Black);
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Draw The Top Face
            if (DrawAce) { fxCard.Texture = tAce; }
            else { fxCard.Texture = tKing; }
            fxCard.World = mWorldCard;
            fxCard.CurrentTechnique.Passes[0].Apply();
            game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, Card, 0, 2);

            // Draw The Bottom Face
            fxCard.Texture = tBack;
            fxCard.World = mBackOff * mWorldCard;
            fxCard.CurrentTechnique.Passes[0].Apply();
            game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, Card, 0, 2);

            game.SpriteBatch.Begin();
            actions.draw(game.SpriteBatch);
            textResults.draw(game.SpriteBatch);
            game.SpriteBatch.End();
            game.GraphicsDevice.SetVertexBuffers(null);
        }

        // Payoff Matrix
        //          S1      S2
        //
        //  S1      -2       5 
        //
        //  S2      0       -1 
        //
        // 
        /*
         * Min z
         * -2b + 5(1-b) <= z
         * b - 1 <= z
         * 
         * 5 - 7b <= z   b -> 0.75
         * a (5 - 7b) = (1 - a) (b - 1)
         * 5a - 7ab = b - ab + a - 1
         * 4a + 1 = b + 6ab
         * (4a + 1) / (6a + 1) = b
         * 
         * 5a/(1 - a) + 1 = b + 7ab/(1 - a)
         * 5a + 1 - a = b - 8ab
         * 4a + 1 = b - 8ab
         * (4a + 1) / (1 - 8a) = b
         * 
         * Min -8ab + 6b + a - 1
         * 
         * 0 <= a <= 1
         * 0 <= b <= 1
         * 
         */
        // Computer Optimal Mixed   {S1 = 0.8, S2 = 0.0, S3 = 0.0, S4 = 0.2}
        // Human    Optimal Mixed   {S1 = 1.0, S2 = 0.0, S3 = 0.0, S4 = 0.0}

        void onActionSelection(SelectionList<int>.Choice c)
        {
            switch (c.Data)
            {
                case 3:
                    //Clear Results
                    money = 0;
                    updateResults();
                    allowUserAction = false;
                    mCycleLocation = 0;
                    mCycleDuration = 0f;
                    trueAce = r.Next(1000) < 500;
                    return;
            }
            if (!allowUserAction) { return; }
            else
            {
                switch (mCycleLocation)
                {
                    case 1:
                        switch (c.Data)
                        {
                            case 0:
                                //Bet
                                pc++; pR = (pR * (pc - 1) + 1) / pc;

                                money += DrawAce ? -5 : 15;
                                updateResults();
                                allowUserAction = false;
                                break;
                            case 1:
                                //Fold
                                pc++; pR = (pR * (pc - 1)) / pc;

                                money -= 1;
                                updateResults();
                                allowUserAction = false;
                                break;
                        }
                        break;
                    case 2:
                        switch (c.Data)
                        {
                            case 2:
                                //Continue
                                allowUserAction = false;
                                break;
                        }
                        break;
                }
            }
        }
        bool computerStrategy(bool ace)
        {
            // Optimal
            float rat = 0.75f;
            if (pc > 1)
            {
                // Mix Rat
                float rb = MathHelper.Clamp(pc / 20f, 0, 1);
                float dop = pR < 0.125 ? 1 : 0;
                rat = rb * dop + (1 - rb) * rat;
            }


            ComputerStrategy strategy = r.NextDouble() < rat ? ComputerStrategy.Strategy1 : ComputerStrategy.Strategy2;

            return !(!ace && strategy.HasFlag(ComputerStrategy.FoldKing));
        }

        Matrix getAnimMatrix(int ci, float d)
        {
            return
                Matrix.CreateFromQuaternion(Quaternion.Slerp(animationCycleQ[ci], animationCycleQ[ci + 1], d / cycleLengths[ci])) *
                Matrix.Lerp(animationCycleM[ci], animationCycleM[ci + 1], d / cycleLengths[ci]);
        }
        void updateResults()
        {
            textResults.setText(string.Format("Money: {0}", money));
        }

        public struct PlayerAction
        {
            public ComputerStrategy SComputer;
            public PlayerStrategy SPlayer;
        }
        public enum ComputerStrategy : byte
        {
            BetKing = Flags.Bit1,
            FoldKing = Flags.Bit2,
            BetAce = Flags.Bit3,
            FoldAce = Flags.Bit4,

            Strategy1 = FoldKing | BetAce,
            Strategy2 = BetKing | BetAce
        }
        public enum PlayerStrategy : byte
        {
            Bet = Flags.Bit1,
            Fold = Flags.Bit2,

            Strategy1 = Bet,
            Strategy2 = Fold
        }
    }
}
