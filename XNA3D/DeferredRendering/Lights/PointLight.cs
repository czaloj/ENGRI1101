using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA3D.Cameras;

namespace XNA3D.DeferredRendering.Lights
{
    public class PointLight : ACLight
    {
        #region Model Information
        public static VertexBuffer vbModel;
        public static IndexBuffer ibModel;
        protected const int thetaFacets = 8;
        protected const float RadsPerThetaFacet = MathHelper.Pi * 2f / thetaFacets;
        protected const int VerticesPerThetaFacet = (4 * (phiFacets - 2) + 3 * 2);
        protected const int phiFacets = 6;
        protected const float RadsPerPhiFacet = MathHelper.Pi / phiFacets;
        protected const int IndecesPerThetaFacet = (6 * (phiFacets - 2) + 3 * 2);
        public static void buildModel(GraphicsDevice g)
        {
            VertexDeferred[] vertices = new VertexDeferred[thetaFacets * VerticesPerThetaFacet];
            int[] indeces = new int[thetaFacets * IndecesPerThetaFacet];

            VertexDeferred v1, v2, v3, v4;
            v1 = new VertexDeferred()
            {
                Position = new Vector3(0, 0, -1),
                TextureCoordinate = Vector2.Zero,
                Normal = new Vector3(0, 0, -1),
                Tangent = new Vector3(-1, 0, 0),
                Binormal = new Vector3(0, -1, 0)
            };
            v2 = v1; v2.TextureCoordinate = Vector2.UnitX;
            v3 = v1; v3.TextureCoordinate = Vector2.UnitY;
            v4 = v1; v4.TextureCoordinate = Vector2.One;
            Matrix tR1, pR1;
            Matrix tR2, pR2;
            Matrix t1, t2, t3, t4;
            int vi = 0;
            int ii = 0;
            for (int tF = 0; tF < thetaFacets; tF++)
            {
                tR1 = Matrix.CreateRotationY(RadsPerThetaFacet * tF);
                tR2 = Matrix.CreateRotationY(RadsPerThetaFacet * (tF + 1));

                for (int pF = 0; pF < phiFacets; pF++)
                {
                    pR1 = Matrix.CreateRotationX(RadsPerPhiFacet * pF - MathHelper.PiOver2);
                    pR2 = Matrix.CreateRotationX(RadsPerPhiFacet * (pF + 1) - MathHelper.PiOver2);

                    t1 = pR2 * tR1;
                    t2 = pR2 * tR2;
                    t3 = pR1 * tR1;
                    t4 = pR1 * tR2;
                    if (pF == 0 || pF == phiFacets - 1)
                    {
                        if (pF == 0)
                        {
                            vertices[vi + 0] = transformVertex(v1, t1);
                            vertices[vi + 1] = transformVertex(v2, t2);
                            vertices[vi + 2] = transformVertex(v3, t3);
                            indeces[ii++] = vi + 0;
                            indeces[ii++] = vi + 1;
                            indeces[ii++] = vi + 2;
                        }
                        else
                        {
                            vertices[vi + 0] = transformVertex(v1, t1);
                            vertices[vi + 1] = transformVertex(v3, t3);
                            vertices[vi + 2] = transformVertex(v4, t4);
                            indeces[ii++] = vi + 0;
                            indeces[ii++] = vi + 2;
                            indeces[ii++] = vi + 1;
                        }
                        vi += 3;
                    }
                    else
                    {
                        vertices[vi + 0] = transformVertex(v1, t1);
                        vertices[vi + 1] = transformVertex(v2, t2);
                        vertices[vi + 2] = transformVertex(v3, t3);
                        vertices[vi + 3] = transformVertex(v4, t4);
                        indeces[ii++] = vi + 0;
                        indeces[ii++] = vi + 1;
                        indeces[ii++] = vi + 2;
                        indeces[ii++] = vi + 2;
                        indeces[ii++] = vi + 1;
                        indeces[ii++] = vi + 3;
                        vi += 4;
                    }
                }
            }
            vbModel = new VertexBuffer(g, VertexDeferred.Declaration, vertices.Length, BufferUsage.None);
            vbModel.SetData<VertexDeferred>(vertices);
            ibModel = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, indeces.Length, BufferUsage.None);
            ibModel.SetData<int>(indeces);
        }
        protected static VertexDeferred transformVertex(VertexDeferred v, Matrix t)
        {
            v.Position = Vector3.Transform(v.Position, t);
            v.Normal = Vector3.Transform(v.Normal, t);
            v.Tangent = Vector3.Transform(v.Tangent, t);
            v.Binormal = Vector3.Transform(v.Binormal, t);
            return v;
        }
        #endregion

        public Renderer renderer;

        public Vector3 Position;
        public float Radius;

        public Vector4 Color;
        public float Intensity;

        //Keep Track Of Matrices While No Changes Need To Be Made
        protected Matrix World;
        protected Matrix Projection;
        protected Matrix[] ViewProjection;

        //Shadowing Information
        public bool IsWithShadows;
        public int ShadowMapResolution;
        public float DepthBias;
        public RenderTargetCube ShadowMap;

        public PointLight(Renderer r, GraphicsDevice g, Info info)
            : base()
        {
            renderer = r;

            OnLightActivationEvent += addLight;
            OnLightDeactivationEvent += removeLight;

            Position = info.Position;
            Radius = info.Radius;
            ViewProjection = new Matrix[6];
            rebuildMatrices();

            Color = info.Color;
            Intensity = info.Intensity;

            IsWithShadows = info.IsWithShadows;
            ShadowMapResolution = info.ShadowMapResolution;
            DepthBias = info.DepthBias;
            ShadowMap = new RenderTargetCube(
                g, ShadowMapResolution, false,
                SurfaceFormat.Single, DepthFormat.Depth24Stencil8
                );
        }
        ~PointLight()
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
        private void addLight() { renderer.addLight(this); }
        private void removeLight() { renderer.removeLight(this); }

        protected void rebuildMatrices()
        {
            World = Matrix.CreateScale(Radius) * Matrix.CreateTranslation(Position);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 0.05f, Radius);
            ViewProjection[0] = Matrix.CreateLookAt(Position, Position + Vector3.Forward, Vector3.Up) * Projection;
            ViewProjection[1] = Matrix.CreateLookAt(Position, Position + Vector3.Backward, Vector3.Up) * Projection;
            ViewProjection[2] = Matrix.CreateLookAt(Position, Position + Vector3.Left, Vector3.Up) * Projection;
            ViewProjection[3] = Matrix.CreateLookAt(Position, Position + Vector3.Right, Vector3.Up) * Projection;
            ViewProjection[4] = Matrix.CreateLookAt(Position, Position + Vector3.Down, Vector3.Forward) * Projection;
            ViewProjection[5] = Matrix.CreateLookAt(Position, Position + Vector3.Up, Vector3.Backward) * Projection;
        }
        protected void rebuildWorld()
        {
            World = Matrix.CreateScale(Radius) * Matrix.CreateTranslation(Position);
            ViewProjection[0] = Matrix.CreateLookAt(Position, Position + Vector3.Forward, Vector3.Up) * Projection;
            ViewProjection[1] = Matrix.CreateLookAt(Position, Position + Vector3.Backward, Vector3.Up) * Projection;
            ViewProjection[2] = Matrix.CreateLookAt(Position, Position + Vector3.Left, Vector3.Up) * Projection;
            ViewProjection[3] = Matrix.CreateLookAt(Position, Position + Vector3.Right, Vector3.Up) * Projection;
            ViewProjection[4] = Matrix.CreateLookAt(Position, Position + Vector3.Down, Vector3.Forward) * Projection;
            ViewProjection[5] = Matrix.CreateLookAt(Position, Position + Vector3.Up, Vector3.Backward) * Projection;
        }
        protected void rebuildProjection()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 0.05f, Radius);
            ViewProjection[0] = Matrix.CreateLookAt(Position, Position + Vector3.Forward, Vector3.Up) * Projection;
            ViewProjection[1] = Matrix.CreateLookAt(Position, Position + Vector3.Backward, Vector3.Up) * Projection;
            ViewProjection[2] = Matrix.CreateLookAt(Position, Position + Vector3.Left, Vector3.Up) * Projection;
            ViewProjection[3] = Matrix.CreateLookAt(Position, Position + Vector3.Right, Vector3.Up) * Projection;
            ViewProjection[4] = Matrix.CreateLookAt(Position, Position + Vector3.Down, Vector3.Forward) * Projection;
            ViewProjection[5] = Matrix.CreateLookAt(Position, Position + Vector3.Up, Vector3.Backward) * Projection;
        }

        public void setRadius(float r, bool rebuild = true)
        {
            Radius = r;
            if (rebuild) { rebuildProjection(); }
        }
        public void setLocation(Vector3 pos, bool rebuild = true)
        {
            Position = pos;
            if (rebuild) { rebuildWorld(); }
        }
        public void setColor(Vector4 c)
        {
            Color = c;
        }
        public void setIntensity(float i)
        {
            Intensity = i;
        }

        public override void setParameters(GraphicsDevice g, ICamera camera, LightEffect e)
        {
            e.Parameters["World"].SetValue(World);

            e.Parameters["LightColor"].SetValue(Color);
            e.Parameters["LightIntensity"].SetValue(Intensity);

            e.Parameters["LightPosition"].SetValue(Position);
            e.Parameters["LightDistance"].SetValue(Radius);

            g.Textures[3] = ShadowMap;
            e.Parameters["Shadows"].SetValue(IsWithShadows);
            e.Parameters["DepthBias"].SetValue(DepthBias);
            e.Parameters["ShadowMapSize"].SetValue(ShadowMapResolution);

            #region Set Cull Mode
            Vector3 diff = camera.Location - Position;
            float CameraToLight = (float)Math.Sqrt((float)Vector3.Dot(diff, diff));
            if (diff.Length() <= Radius)
            {
                g.RasterizerState = RasterizerState.CullClockwise;
            }
            else
            {
                g.RasterizerState = RasterizerState.CullCounterClockwise;
            }
            #endregion
        }

        public struct Info
        {
            public Vector3 Position;
            public float Radius;

            public Vector4 Color;
            public float Intensity;

            public bool IsWithShadows;
            public int ShadowMapResolution;
            public float DepthBias;

            public Info(Vector3 pos, float r, Vector4 c, float intensity = 1f, bool isWithShadows = false, int shadowRes = 256, float dBias = 1 / 500f)
            {
                Position = pos;
                Radius = r;
                Color = c;
                Intensity = intensity;
                IsWithShadows = isWithShadows;
                ShadowMapResolution = shadowRes;
                DepthBias = dBias;
            }
        }
        public class Renderer : ACLightRenderer<PointLight>
        {
            public Renderer(LightEffect e) : base(e) { }

            public override void drawAllLights(GraphicsDevice g, ICamera camera, DeferredRenderer renderer)
            {
                //Set Point Lights Geometry Buffers
                g.SetVertexBuffer(PointLight.vbModel, 0);
                g.Indices = PointLight.ibModel;
                fxLight.setCurrentTechnique(LightEffect.TechniquePoint);
                foreach (PointLight light in lights)
                {
                    if (light.isActive)
                    {
                        light.setParameters(g, camera, fxLight);
                        fxLight.applyPass(LightEffect.PassLighting);
                        g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, thetaFacets * VerticesPerThetaFacet, 0, (thetaFacets * IndecesPerThetaFacet) / 3);
                    }
                }
            }
            public void drawAllShadows(GraphicsDevice g, ICamera camera, DeferredRenderer renderer)
            {
                //Set States
                g.BlendState = BlendState.Opaque;
                g.DepthStencilState = DepthStencilState.Default;
                g.RasterizerState = RasterizerState.CullCounterClockwise;
                fxLight.setCurrentTechnique(LightEffect.TechniquePoint);
                foreach (PointLight Light in lights)
                {
                    //Draw it's Shadow Map
                    if (Light.isActive && Light.IsWithShadows)
                    {
                        //Set Global Effect Value
                        fxLight.Parameters["LightPosition"].SetValue(Light.Position);
                        fxLight.Parameters["LightDistance"].SetValue(Light.Radius);

                        #region Forward
                        g.SetRenderTarget(Light.ShadowMap, CubeMapFace.PositiveZ);
                        //Clear Target
                        g.Clear(Microsoft.Xna.Framework.Color.Transparent);
                        //Set global Effect parameters
                        fxLight.Parameters["LightViewProjection"].SetValue(Light.ViewProjection[0]);
                        //Draw Models
                        renderer.drawShadowScene(g, null, fxLight.asEffect());
                        #endregion
                        #region Backward
                        g.SetRenderTarget(Light.ShadowMap, CubeMapFace.NegativeZ);
                        //Clear Target
                        g.Clear(Microsoft.Xna.Framework.Color.Transparent);
                        //Set global Effect parameters
                        fxLight.Parameters["LightViewProjection"].SetValue(Light.ViewProjection[1]);
                        //Draw Models
                        renderer.drawShadowScene(g, null, fxLight.asEffect());
                        #endregion

                        #region Left
                        g.SetRenderTarget(Light.ShadowMap, CubeMapFace.NegativeX);
                        //Clear Target
                        g.Clear(Microsoft.Xna.Framework.Color.Transparent);
                        //Set global Effect parameters
                        fxLight.Parameters["LightViewProjection"].SetValue(Light.ViewProjection[2]);
                        //Draw Models
                        renderer.drawShadowScene(g, null, fxLight.asEffect());
                        #endregion
                        #region Right
                        g.SetRenderTarget(Light.ShadowMap, CubeMapFace.PositiveX);
                        //Clear Target
                        g.Clear(Microsoft.Xna.Framework.Color.Transparent);
                        //Set global Effect parameters
                        fxLight.Parameters["LightViewProjection"].SetValue(Light.ViewProjection[3]);
                        //Draw Models
                        renderer.drawShadowScene(g, null, fxLight.asEffect());
                        #endregion

                        #region Down
                        g.SetRenderTarget(Light.ShadowMap, CubeMapFace.NegativeY);
                        //Clear Target
                        g.Clear(Microsoft.Xna.Framework.Color.Transparent);
                        //Set global Effect parameters
                        fxLight.Parameters["LightViewProjection"].SetValue(Light.ViewProjection[4]);
                        //Draw Models
                        renderer.drawShadowScene(g, null, fxLight.asEffect());
                        #endregion
                        #region Up
                        g.SetRenderTarget(Light.ShadowMap, CubeMapFace.PositiveY);
                        //Clear Target
                        g.Clear(Microsoft.Xna.Framework.Color.Transparent);
                        //Set global Effect parameters
                        fxLight.Parameters["LightViewProjection"].SetValue(Light.ViewProjection[5]);
                        //Draw Models
                        renderer.drawShadowScene(g, null, fxLight.asEffect());
                        #endregion
                    }
                }
            }
        }
    }
}
