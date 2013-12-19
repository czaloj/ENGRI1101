using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XNA3D.Cameras;
using XNA3D.DeferredRendering.Lights;
using XNA3D.ZDR;

namespace XNA3D.DeferredRendering
{
    public delegate void OnGeometryDraw(GraphicsDevice g, ICamera camera, Effect e);

    public class DeferredRenderer
    {
        //Deferred Effects
        Effect fxCompose;

        //The Drawing Buffers (Batches)
        public GeometryBuffer GBuffer;
        public LightBuffer LBuffer;

        //GBuffer Texture Size
        public Vector2 GBufferTextureSize { get { return GBuffer.TextureSize; } }
        public Vector2 LightMapTextureSize { get { return LBuffer.TextureSize; } }

        //Fullscreen Quad
        public FullScreenQuad fsq;

        //Events For Model Linking
        public event OnLightDraw OnShadowMapDrawEvent;
        public event OnGeometryDraw OnShadowDepthDrawEvent;
        public event OnGeometryDraw OnPreComposeDrawEvent;

        //Constructor
        public DeferredRenderer(GraphicsDevice g, ContentManager Content, InitInfo info, LightEffect e, params OnLightDraw[] lightRenderers)
        {
            //Check That All Information Is Provided
            if (info == null || g == null || Content == null || !info.AllSet)
            {
                throw new ArgumentException("Missing Initialization Information");
            }

            //Load Composition Shader
            fxCompose = Content.Load<Effect>(info.FX_Composition);
            fxCompose.CurrentTechnique = fxCompose.Techniques[0];

            //Create Fullscreen Quad
            fsq = new FullScreenQuad(g);

            //Make The Geometry Buffer
            GBuffer = new GeometryBuffer(
                info.GBufferWidth,
                info.GBufferHeight,
                Content.Load<Effect>(info.FX_Clear),
                Content.Load<Effect>(info.FX_GBuffer),
                fsq
                );
            GBuffer.build(g);

            //Make The Light Buffer
            LBuffer = new LightBuffer(
                info.LightMapWidth,
                info.LightMapHeight,
                fsq,
                this,
                e,
                lightRenderers
                );
            LBuffer.build(g);
        }

        public void drawScene(GraphicsDevice g, ICamera camera, Effect e)
        {
            GBuffer.justDraw(g, camera);
        }
        public void drawShadowScene(GraphicsDevice g, ICamera camera, Effect e)
        {
            if (OnShadowDepthDrawEvent != null)
            {
                OnShadowDepthDrawEvent(g, camera, e);
            }
        }

        public void ClearGBuffer(GraphicsDevice g)
        {
            GBuffer.begin(g);
        }
        public void MakeGBuffer(GraphicsDevice g, ICamera camera)
        {
            GBuffer.draw(g, camera);
            GBuffer.end(g);
        }
        public void MakeLightMap(GraphicsDevice GraphicsDevice, ICamera Camera)
        {
            LBuffer.begin(GraphicsDevice, Camera);
            LBuffer.draw(GraphicsDevice, Camera);
            LBuffer.end(GraphicsDevice);
        }

        public void MakeShadowMap(GraphicsDevice GraphicsDevice, ICamera Camera)
        {
            if (OnShadowMapDrawEvent != null)
            {
                OnShadowMapDrawEvent(GraphicsDevice, Camera, this);
            }
        }
        public void PreDraw(GraphicsDevice g, ICamera camera)
        {
            //Make Shadows
            MakeShadowMap(g, camera);

            //Set States
            g.BlendState = BlendState.Opaque;
            g.DepthStencilState = DepthStencilState.Default;
            g.RasterizerState = RasterizerState.CullCounterClockwise;

            //Do G-Buffer
            GBuffer.begin(g);
            GBuffer.draw(g, camera);
            GBuffer.end(g);

            //Do L-Buffer
            LBuffer.begin(g, camera);
            LBuffer.draw(g, camera);
            LBuffer.end(g);
        }
        public void MakeFinal(GraphicsDevice g, ICamera camera, RenderTarget2D output)
        {

            //Set Composition Target
            g.SetRenderTarget(output);
            g.Clear(Color.Black);
            if (OnPreComposeDrawEvent != null)
            {
                OnPreComposeDrawEvent(g, camera, null);
            }
            //Set Textures
            g.Textures[0] = GBuffer.ColorTarget.RenderTarget;
            g.SamplerStates[0] = SamplerState.PointClamp;
            g.Textures[1] = LBuffer.LightTarget.RenderTarget;
            g.SamplerStates[1] = SamplerState.LinearClamp;
            g.Textures[2] = GBuffer.DepthTarget.RenderTarget;
            g.SamplerStates[2] = SamplerState.PointClamp;
            //Apply
            fxCompose.CurrentTechnique.Passes[0].Apply();
            //Draw
            fsq.ReadyBuffers(g);
            fsq.Draw(g);
        }

        public void Debug(GraphicsDevice g, SpriteBatch spriteBatch)
        {
            //Begin SpriteBatch
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);
            int width = g.Viewport.Width / 5;
            int height = g.Viewport.Height / 5;
            //Set up Drawing Rectangle
            Rectangle rect = new Rectangle(0, 0, width, height);
            //Draw GBuffer 0
            spriteBatch.Draw((Texture2D)GBuffer.ColorTarget.RenderTarget, rect, Color.White);
            //Draw GBuffer 1
            rect.X += width;
            spriteBatch.Draw((Texture2D)GBuffer.NormalTarget.RenderTarget, rect, Color.White);
            //Draw GBuffer 2
            rect.X += width;
            spriteBatch.Draw((Texture2D)GBuffer.DepthTarget.RenderTarget, rect, Color.White);
            //Draw Light
            rect.X += width;
            spriteBatch.Draw((Texture2D)LBuffer.LightTarget.RenderTarget, rect, Color.White);
            //End SpriteBatch
            spriteBatch.End();
        }
        public void Debug(GraphicsDevice g, SpriteBatch batch, int n)
        {
            batch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);
            Rectangle rect = new Rectangle(0, 0, g.Viewport.Width, g.Viewport.Height);
            if (n >= 0 && n < 4)
            {
                Texture rt = null;
                switch (n)
                {
                    case 0: rt = GBuffer.ColorTarget.RenderTarget; break;
                    case 1: rt = GBuffer.NormalTarget.RenderTarget; break;
                    case 2: rt = GBuffer.DepthTarget.RenderTarget; break;
                    case 3: rt = LBuffer.LightTarget.RenderTarget; break;
                }
                batch.Draw((Texture2D)rt, rect, Color.White);
            }
            batch.End();
        }
        public void Draw(GraphicsDevice g, ICamera camera, RenderTarget2D output)
        {
            PreDraw(g, camera);
            MakeFinal(g, camera, output);
        }

        #region Initialization Information
        public class InitInfo
        {
            public bool AllSet
            {
                get
                {
                    return
                        HasAllEffects;
                }
            }

            protected string[] fxFiles;
            public string FX_Clear { get { return fxFiles[0]; } }
            public string FX_GBuffer { get { return fxFiles[1]; } }
            public string FX_Composition { get { return fxFiles[2]; } }
            public bool HasAllEffects
            {
                get
                {
                    if (fxFiles == null ||
                        fxFiles.Length < 3
                        )
                    {
                        return false;
                    }
                    foreach (string s in fxFiles)
                    {
                        if (s == null)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }

            public int GBufferWidth, GBufferHeight;
            public int LightMapWidth, LightMapHeight;

            public InitInfo()
            {
                GBufferWidth = 800;
                GBufferHeight = 600;
                LightMapWidth = 400;
                LightMapHeight = 300;
            }

            public void setEffects(
                string clear,
                string gBuffer,
                string composition
                )
            {
                fxFiles = new string[]
                {
                    clear,
                    gBuffer,
                    composition
                };
            }
            public void setRenderSizes(
                int gW, int gH,
                int lW, int lH
                )
            {
                GBufferWidth = gW;
                GBufferHeight = gH;
                LightMapWidth = lW;
                LightMapHeight = lH;
            }
        }
        #endregion

        #region G-Buffer
        public class GeometryBuffer
        {
            public const int TargetCount = 3;
            public const int ColorTargetIndex = 0;
            public const int NormalTargetIndex = 1;
            public const int DepthTargetIndex = 2;

            //Render Targets
            protected RenderTargetBinding[] renderTargets;
            public RenderTargetBinding ColorTarget
            {
                get
                {
                    return renderTargets[ColorTargetIndex];
                }
            }
            public RenderTargetBinding NormalTarget
            {
                get
                {
                    return renderTargets[NormalTargetIndex];
                }
            }
            public RenderTargetBinding DepthTarget
            {
                get
                {
                    return renderTargets[DepthTargetIndex];
                }
            }

            //Size Of Buffer
            public int Width, Height;
            public Vector2 TextureSize;

            //Clearing Effect
            private FullScreenQuad fsQuad;
            protected Effect fxClear;

            //Drawing Batches
            private LinkedList<DrawBatch> Batches;
            public DrawBatch DefaultBatch { get { return Batches.First.Value; } }

            public GeometryBuffer(int w, int h, Effect clear, Effect defaultGeometry, FullScreenQuad fsq)
            {
                Width = w; Height = h;
                TextureSize = new Vector2(Width, Height);

                fxClear = clear;
                fxClear.CurrentTechnique = fxClear.Techniques[0];
                fsQuad = fsq;

                //Make The Default Batch
                Batches = new LinkedList<DrawBatch>();
                DrawBatch db = new DefaultDrawBatch(defaultGeometry);
                Batches.AddLast(db);
            }

            public void build(GraphicsDevice g)
            {
                //Create The Render Targets
                renderTargets = new RenderTargetBinding[TargetCount];
                //Color
                renderTargets[ColorTargetIndex] = new RenderTargetBinding(new RenderTarget2D(g,
                Width, Height, false,
                SurfaceFormat.Rgba64,
                DepthFormat.Depth24Stencil8));
                //Normal
                renderTargets[NormalTargetIndex] = new RenderTargetBinding(new RenderTarget2D(g,
                Width, Height, false,
                SurfaceFormat.Rgba64,
                DepthFormat.Depth24Stencil8));
                //Depth
                renderTargets[DepthTargetIndex] = new RenderTargetBinding(new RenderTarget2D(g,
                Width, Height, false,
                SurfaceFormat.Vector2,
                DepthFormat.Depth24Stencil8));
            }
            public void addBatch(DrawBatch db)
            {
                Batches.AddLast(db);
            }

            public void begin(GraphicsDevice g)
            {
                //Set Targets
                g.SetRenderTargets(
                    renderTargets[ColorTargetIndex],
                    renderTargets[NormalTargetIndex],
                    renderTargets[DepthTargetIndex]
                );

                //Allow For Depth Modifying During Clear
                g.DepthStencilState = DepthStencilState.Default;

                //Clear The Targets
                fxClear.CurrentTechnique.Passes[0].Apply();
                fsQuad.Draw(g);
            }
            public void draw(GraphicsDevice g, ICamera camera)
            {
                foreach (DrawBatch db in Batches)
                {
                    if (db.HasDrawers)
                    {
                        db.setupEffect(g, camera);
                        db.draw(g, camera);
                    }
                }
            }
            public void justDraw(GraphicsDevice g, ICamera camera)
            {
                foreach (DrawBatch db in Batches)
                {
                    if (db.HasDrawers)
                    {
                        db.draw(g, camera);
                    }
                }
            }
            public void end(GraphicsDevice g)
            {

            }

            public void setTextures(GraphicsDevice g)
            {
                g.Textures[0] = renderTargets[ColorTargetIndex].RenderTarget;
                g.SamplerStates[0] = SamplerState.LinearClamp;
                g.Textures[1] = renderTargets[NormalTargetIndex].RenderTarget;
                g.SamplerStates[1] = SamplerState.LinearClamp;
                g.Textures[2] = renderTargets[DepthTargetIndex].RenderTarget;
                g.SamplerStates[2] = SamplerState.PointClamp;
            }

            public abstract class DrawBatch
            {
                public event OnGeometryDraw OnDrawEvent;

                public Effect fxGeometry;

                public bool HasDrawers
                {
                    get
                    {
                        return OnDrawEvent != null;
                    }
                }

                public DrawBatch(Effect e)
                {
                    fxGeometry = e;
                    fxGeometry.CurrentTechnique = fxGeometry.Techniques[0];
                }

                public abstract void setupEffect(GraphicsDevice g, ICamera camera);
                public void draw(GraphicsDevice g, ICamera camera)
                {
                    OnDrawEvent(g, camera, fxGeometry);
                }
            }
            public class DefaultDrawBatch : DrawBatch
            {
                public DefaultDrawBatch(Effect e)
                    : base(e)
                {

                }

                public override void setupEffect(GraphicsDevice g, ICamera camera)
                {
                    fxGeometry.Parameters["View"].SetValue(camera.View);
                    fxGeometry.Parameters["Projection"].SetValue(camera.Projection);

                    //World And Trans(Inv(World * View)) Must Be Set
                }
            }
        }
        #endregion

        #region L-Buffer
        public class LightBuffer
        {
            //Light Specs
            public int Width, Height;
            public Vector2 TextureSize;
            public RenderTargetBinding LightTarget;

            public LightEffect fxLight;
            protected BlendState LightBlend;

            //Light Renderers
            event OnLightDraw OnLightDrawEvent;
            DeferredRenderer dR;

            //Draw Calculation Storage
            public Matrix InverseView, InverseViewProjection;
            private FullScreenQuad fsQuad;

            public LightBuffer(int w, int h, FullScreenQuad fsq, DeferredRenderer dr, LightEffect e, OnLightDraw[] lightRenderers)
            {
                Width = w;
                Height = h;
                TextureSize = new Vector2(Width, Height);

                fsQuad = fsq;
                dR = dr;
                foreach (OnLightDraw f in lightRenderers)
                {
                    OnLightDrawEvent += f;
                }

                fxLight = e;
            }

            public void build(GraphicsDevice g)
            {
                LightBlend = new BlendState();
                LightBlend.ColorSourceBlend = Blend.One;
                LightBlend.ColorDestinationBlend = Blend.One;
                LightBlend.ColorBlendFunction = BlendFunction.Add;
                LightBlend.AlphaSourceBlend = Blend.One;
                LightBlend.AlphaDestinationBlend = Blend.One;
                LightBlend.AlphaBlendFunction = BlendFunction.Add;

                LightTarget = new RenderTargetBinding(new RenderTarget2D(g,
                Width, Height, false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8));
            }

            public void begin(GraphicsDevice g, ICamera camera)
            {
                //Set LightMap Target
                g.SetRenderTargets(LightTarget);

                g.DepthStencilState = DepthStencilState.None;
                //Clear to Transperant Black
                g.Clear(Color.Transparent);
                //Set States
                g.BlendState = LightBlend;
                g.DepthStencilState = DepthStencilState.DepthRead;

                //Preset Information
                fxLight.Parameters["View"].SetValue(camera.View);
                fxLight.Parameters["Projection"].SetValue(camera.Projection);
                fxLight.Parameters["CameraPosition"].SetValue(camera.Location);
                fxLight.Parameters["InverseViewProjection"].SetValue(Matrix.Invert(camera.View * camera.Projection));

                //Set Textures And Samplers
                dR.GBuffer.setTextures(g);
                g.SamplerStates[3] = SamplerState.PointClamp;
            }
            public void draw(GraphicsDevice g, ICamera camera)
            {
                OnLightDrawEvent(g, camera, dR);
            }
            public void end(GraphicsDevice g)
            {
                //Set States Off
                g.BlendState = BlendState.Opaque;
                g.RasterizerState = RasterizerState.CullCounterClockwise;
                g.DepthStencilState = DepthStencilState.Default;
            }
        }
        #endregion
    }
}
