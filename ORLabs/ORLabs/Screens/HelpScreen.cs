using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class HelpScreen : GameScreen
    {
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

        Texture2D tex;
        BasicEffect ef;
        VertexPositionColorTexture[] verts;

        public override void build()
        {
            using (var f = System.IO.File.OpenRead(@"Resources\Textures\Help.png"))
            {
                tex = Texture2D.FromStream(game.GraphicsDevice, f);
            }

            ef = new BasicEffect(game.GraphicsDevice);

            verts = new VertexPositionColorTexture[]
            {
                 new VertexPositionColorTexture(new Vector3(-1, 1, -1f), Color.White, new Vector2(0, 0)),
                 new VertexPositionColorTexture(new Vector3(1, 1, -1f), Color.White,new Vector2(1, 0)),
                 new VertexPositionColorTexture(new Vector3(-1, -1, -1f), Color.White, new Vector2(0, 1)),
                 new VertexPositionColorTexture(new Vector3(1, -1, -1f), Color.White, new Vector2(1, 1))
            };
        }
        public override void destroy(GameTime gameTime)
        {
            tex.Dispose();
            ef.Dispose();
        }

        public override void onEntry(GameTime gameTime)
        {
            ef.VertexColorEnabled = false;
            ef.LightingEnabled = false;
            ef.TextureEnabled = true;
            ef.Texture = tex;
            ef.World = Matrix.Identity;
            ef.View = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);
            ef.Projection = Matrix.CreateOrthographic(2, 2, 0, 1);

        }
        public override void onExit(GameTime gameTime)
        {
        }

        public override void update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                State = ScreenState.ChangePrevious;
                //game.previousScreen(gameTime);
                return;
            }
        }
        public override void draw(GameTime gameTime)
        {
            game.GraphicsDevice.Clear(Color.Black);
            game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            game.GraphicsDevice.Textures[0] = tex;
            ef.CurrentTechnique.Passes[0].Apply();
            game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, 2, VertexPositionColorTexture.VertexDeclaration);
        }
    }
}
