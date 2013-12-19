using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA3D.Cameras;
using xColor = Microsoft.Xna.Framework.Color;

namespace XNA3D.DeferredRendering.Lights
{
    public class Sunlight : ACLight
    {
        public static SunlightRenderer Renderer;

        static Sunlight()
        {
            Renderer = new SunlightRenderer();
        }

        public static VertexBuffer vbModel;
        public static IndexBuffer ibModel;

        public static void buildModel(GraphicsDevice g)
        {
            VertexDeferred[] v = new VertexDeferred[6 * 4]
            {
                new VertexDeferred(new Vector3(1,-1,1), Vector3.Right,
                    Vector2.UnitY, Vector3.Forward, Vector3.Down
                    ),
                new VertexDeferred(new Vector3(1,1,1), Vector3.Right,
                    Vector2.Zero, Vector3.Forward, Vector3.Down
                    ),
                new VertexDeferred(new Vector3(1,1,0), Vector3.Right,
                    Vector2.UnitX, Vector3.Forward, Vector3.Down
                    ),
                new VertexDeferred(new Vector3(1,-1,0), Vector3.Right,
                    Vector2.One, Vector3.Forward, Vector3.Down
                    ),

                new VertexDeferred(new Vector3(-1,-1,0), Vector3.Left,
                    Vector2.UnitY, Vector3.Backward, Vector3.Down
                    ),
                new VertexDeferred(new Vector3(-1,1,0), Vector3.Left,
                    Vector2.Zero, Vector3.Backward, Vector3.Down
                    ),
                new VertexDeferred(new Vector3(-1,1,1), Vector3.Left,
                    Vector2.UnitX, Vector3.Backward, Vector3.Down
                    ),
                new VertexDeferred(new Vector3(-1,-1,1), Vector3.Left,
                    Vector2.One, Vector3.Backward, Vector3.Down
                    ),

                new VertexDeferred(new Vector3(-1,1,1), Vector3.Up,
                    Vector2.UnitY, Vector3.Left, Vector3.Backward
                    ),
                new VertexDeferred(new Vector3(-1,1,0), Vector3.Up,
                    Vector2.Zero, Vector3.Left, Vector3.Backward
                    ),
                new VertexDeferred(new Vector3(1,1,0), Vector3.Up,
                    Vector2.UnitX, Vector3.Left, Vector3.Backward
                    ),
                new VertexDeferred(new Vector3(1,1,1), Vector3.Up,
                    Vector2.One, Vector3.Left, Vector3.Backward
                    ),

                new VertexDeferred(new Vector3(1,-1,1), Vector3.Down,
                    Vector2.UnitY, Vector3.Right, Vector3.Backward
                    ),
                new VertexDeferred(new Vector3(1,-1,0), Vector3.Down,
                    Vector2.Zero, Vector3.Right, Vector3.Backward
                    ),
                new VertexDeferred(new Vector3(-1,-1,0), Vector3.Down,
                    Vector2.UnitX, Vector3.Right, Vector3.Backward
                    ),
                new VertexDeferred(new Vector3(-1,-1,1), Vector3.Down,
                    Vector2.One, Vector3.Right, Vector3.Backward
                    ),

                new VertexDeferred(new Vector3(-1,-1,1), Vector3.Backward,
                    Vector2.UnitY, Vector3.Right, Vector3.Down
                    ),
                new VertexDeferred(new Vector3(-1,1,1), Vector3.Backward,
                    Vector2.Zero, Vector3.Right, Vector3.Down
                    ),
                new VertexDeferred(new Vector3(1,1,1), Vector3.Backward,
                    Vector2.UnitX, Vector3.Right, Vector3.Down
                    ),
                new VertexDeferred(new Vector3(1,-1,1), Vector3.Backward,
                    Vector2.One, Vector3.Right, Vector3.Down
                    ),

                new VertexDeferred(new Vector3(1,-1,0), Vector3.Forward,
                    Vector2.UnitY, Vector3.Left, Vector3.Down
                    ),
                new VertexDeferred(new Vector3(1,1,0), Vector3.Forward,
                    Vector2.Zero, Vector3.Left, Vector3.Down
                    ),
                new VertexDeferred(new Vector3(-1,1,0), Vector3.Forward,
                    Vector2.UnitX, Vector3.Left, Vector3.Down
                    ),
                new VertexDeferred(new Vector3(-1,-1,0), Vector3.Forward,
                    Vector2.One, Vector3.Left, Vector3.Down
                    )
            };

            int[] i = new int[3 * 6 * 2];
            for (int f = 0; f < 6; f++)
            {
                i[f * 6 + 0] = f * 4 + 0;
                i[f * 6 + 1] = f * 4 + 1;
                i[f * 6 + 2] = f * 4 + 2;
                i[f * 6 + 3] = f * 4 + 0;
                i[f * 6 + 4] = f * 4 + 2;
                i[f * 6 + 5] = f * 4 + 3;
            }
            vbModel = new VertexBuffer(g, VertexDeferred.Declaration, v.Length, BufferUsage.None);
            vbModel.SetData<VertexDeferred>(v);
            ibModel = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, i.Length, BufferUsage.None);
            ibModel.SetData<int>(i);
        }

        public BoundingBox LightBound;
        public Vector3 BoxSize;
        public Vector3 ShineDirection;
        public Vector4 Color;

        public Matrix World;
        public Matrix View;
        public Matrix Projection;

        //Shadow Information
        public bool isWithShadows;
        public int ShadowMapResolution;
        public float DepthBias;
        //The Shadow Map Of The Spotlight (If Shadows Are Used)
        public RenderTarget2D ShadowMap;

        public Sunlight(
            BoundingBox bounds,
            Vector3 direction,
            Vector4 color,
            GraphicsDevice g,
            bool isWithShadows,
            int shadowMapResolution,
            float depthBias = 1f / 500f
            )
            : base()
        {
            LightBound = bounds;
            BoxSize = LightBound.Max - LightBound.Min;
            Projection = Matrix.Identity;
            setDirection(direction);
            Color = color;
            this.isWithShadows = isWithShadows;
            this.ShadowMapResolution = shadowMapResolution;
            ShadowMap = new RenderTarget2D(
                g,
                ShadowMapResolution,
                ShadowMapResolution,
                false,
                SurfaceFormat.Single,
                DepthFormat.Depth24Stencil8
                );
            DepthBias = depthBias;
            OnLightActivationEvent += addLight;
            OnLightDeactivationEvent += removeLight;
        }
        ~Sunlight()
        {
            OnLightActivationEvent -= addLight;
            OnLightDeactivationEvent -= removeLight;
            ShadowMap.Dispose();
        }
        public override void Dispose()
        {
            base.Dispose();
            OnLightActivationEvent -= addLight;
            OnLightDeactivationEvent -= removeLight;
            ShadowMap.Dispose();
        }
        private void addLight() { Renderer.addLight(this); }
        private void removeLight() { Renderer.removeLight(this); }

        public void setDirection(Vector3 direction)
        {
            ShineDirection = direction;
            Vector2 skew = getXZSkew(ShineDirection);
            Vector2 absskew = skew;
            if (absskew.X < 0f) { absskew.X = -absskew.X; }
            if (absskew.Y < 0f) { absskew.Y = -absskew.Y; }
            Vector3 scale = getScale(absskew, BoxSize);
            Vector3 trans = getTrans(skew, LightBound);
            Vector3 trueScale = new Vector3(
                scale.X / 2f,
                scale.Y,
                scale.Z / 2f
                );
            Vector3 c = LightBound.Max + LightBound.Min;
            c /= 2f;
            View = Matrix.CreateLookAt(
                new Vector3(c.X, LightBound.Max.Y, c.Z),
                new Vector3(c.X, LightBound.Max.Y - 1, c.Z),
                Vector3.Forward
                );
            Projection = Matrix.CreateOrthographic(
                scale.X, scale.Z, 1f, BoxSize.Y
                ) *
                Matrix.CreateTranslation(Vector3.Forward * 0.5f) *
                MatrixExt.CreateSkewXYAlongX_Slope(skew.X) *
                MatrixExt.CreateSkewXYAlongY_Slope(-skew.Y) *
                Matrix.CreateTranslation(Vector3.Backward * 0.5f)
                ;
            World = Matrix.Invert(View * Projection);
            //World =
            //    //Matrix.CreateRotationZ(-MathHelper.PiOver2) *
            //    Matrix.CreateRotationX(-MathHelper.PiOver2) *
            //    Matrix.CreateScale(trueScale) *
            //    MatrixExt.CreateSkewXZAlongX_Slope(skew.X) *
            //    MatrixExt.CreateSkewXZAlongZ_Slope(skew.Y) *
            //    Matrix.CreateTranslation(trans)
            //    ;
            //View = Matrix.Invert(World);
        }

        private static Vector2 getXZSkew(Vector3 shineDirection)
        {
            shineDirection /= shineDirection.Y;
            return new Vector2(shineDirection.X, shineDirection.Z);
        }
        private static Vector3 getScale(Vector2 absskew, Vector3 size)
        {
            return new Vector3(size.X * (1 + absskew.X), size.Y, size.Z * (1 + absskew.Y));
        }
        private static Vector3 getTrans(Vector2 skew, BoundingBox bb)
        {
            Vector3 size = bb.Max - bb.Min;
            Vector3 c = (bb.Max + bb.Min) / 2f;
            Vector2 sizeAdd = new Vector2(
                (skew.X > 0) ? 1 : -1,
                (skew.Y > 0) ? 1 : -1
                );
            if (skew.X == 0) { sizeAdd.X = 0; }
            if (skew.Y == 0) { sizeAdd.Y = 0; }
            return new Vector3(
                c.X - (size.X * (skew.X + sizeAdd.X)) / 2f,
                bb.Min.Y,
                c.Z - (size.Z * (skew.Y + sizeAdd.Y)) / 2f
                );
        }

        public override void draw(GraphicsDevice g, ACCamera camera, Effect e)
        {
            //Set Attenuation Cookie Texture and SamplerState
            g.Textures[3] = ShadowMap;
            //Set Spot Light Parameters
            e.Parameters["World"].SetValue(World);
            e.Parameters["LightViewProjection"].SetValue(View * Projection);
            e.Parameters["LightTopPlane"].SetValue(LightBound.Max.Y);
            e.Parameters["LightColor"].SetValue(Color);
            e.Parameters["LightDirection"].SetValue(ShineDirection);
            e.Parameters["LightDistance"].SetValue(BoxSize.Y);
            e.Parameters["Shadows"].SetValue(isWithShadows);
            e.Parameters["shadowMapSize"].SetValue(ShadowMapResolution);
            e.Parameters["DepthBias"].SetValue(DepthBias);

            #region Set Cull Mode
            //Vector3 L = Vector3.Transform(camera.Location, View * Projection);
            //if (
            //    L.X >= -1f && L.X <= 1f &&
            //    L.Y >= -1f && L.Y <= 1f &&
            //    L.Z >= -1f && L.Z <= 1f
            //    )
            //{
            //    g.RasterizerState = RasterizerState.CullClockwise;
            //}
            //else
            //{
            //    g.RasterizerState = RasterizerState.CullCounterClockwise;
            //}
            g.RasterizerState = RasterizerState.CullCounterClockwise;
            #endregion

            //Apply
            e.CurrentTechnique.Passes[0].Apply();

            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);
        }
        public void drawModel(GraphicsDevice g, ACCamera camera, Effect e)
        {
            g.SetVertexBuffer(Sunlight.vbModel, 0);
            g.Indices = Sunlight.ibModel;

            e.Parameters["World"].SetValue(World);
            e.Parameters["WorldViewIT"].SetValue(Matrix.Invert(Matrix.Transpose(World * camera.ViewMatrix)));

            e.CurrentTechnique.Passes[0].Apply();

            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);
        }
    }
}
