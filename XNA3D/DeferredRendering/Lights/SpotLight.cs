using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA3D.Cameras;

namespace XNA3D.DeferredRendering.Lights
{
    public class SpotLight : ACLight
    {
        public static VertexBuffer vbModel;
        public static IndexBuffer ibModel;
        public const int Faces = 12;
        public static void buildModel(GraphicsDevice g)
        {
            VertexDeferred[] vertices = new VertexDeferred[6 * Faces];
            int[] indeces = new int[Faces * 6];

            Vector3 nCirc = new Vector3(0, 0, -1);
            Vector3 nFace = new Vector3(0, 1, 1); nFace.Normalize();
            Vector3 vPoint = new Vector3(0, 1, -1);
            Vector3 v1, v2, n, tan, binorm;
            Vector3 tip = Vector3.Zero;
            Vector3 cBase = Vector3.Forward;
            float rAngle = (MathHelper.Pi * 2f) / Faces;
            Matrix mRotate = Matrix.CreateRotationZ(rAngle);
            Matrix mTrans = mRotate;
            v2 = vPoint;
            for (int i = 0; i < Faces; i++)
            {
                v1 = Vector3.Transform(vPoint, mTrans);
                n = Vector3.Transform(nFace, mTrans);

                tan = Vector3.Normalize(v1 - tip);
                binorm = Vector3.Normalize(v2 - v1);
                vertices[i * 6 + 0] = new VertexDeferred(v1, n, Vector2.UnitX, tan, binorm);
                vertices[i * 6 + 1] = new VertexDeferred(tip, n, Vector2.Zero, tan, binorm);
                vertices[i * 6 + 2] = new VertexDeferred(v2, n, Vector2.UnitY, tan, binorm);
                tan = Vector3.Normalize(v1 - cBase);
                binorm = Vector3.Normalize(v2 - v1);
                vertices[i * 6 + 3] = new VertexDeferred(v1, nCirc, Vector2.UnitX, tan, binorm);
                vertices[i * 6 + 4] = new VertexDeferred(cBase, nCirc, Vector2.Zero, tan, binorm);
                vertices[i * 6 + 5] = new VertexDeferred(v2, nCirc, Vector2.UnitY, tan, binorm);

                v2 = v1;
                mTrans = mRotate * mTrans;
            }
            int f = 0;
            for (int i = 0; i < Faces * 6; )
            {
                indeces[i++] = f * 6 + 2;
                indeces[i++] = f * 6 + 1;
                indeces[i++] = f * 6 + 0;
                indeces[i++] = f * 6 + 3;
                indeces[i++] = f * 6 + 4;
                indeces[i++] = f * 6 + 5;
                f++;
            }
            vbModel = new VertexBuffer(g, VertexDeferred.Declaration, vertices.Length, BufferUsage.None);
            vbModel.SetData<VertexDeferred>(vertices);
            ibModel = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, indeces.Length, BufferUsage.None);
            ibModel.SetData<int>(indeces);
        }

        public Renderer r;

        //Most Important Information About Spotlight
        public Vector3 Location;
        public Vector3 Direction;

        //Color Of The Light
        public Vector4 Color;
        public float Intensity;

        //Clipping Information
        public float NearClip;
        public float FarClip;

        //Angle Information About The Spot Light
        public float OuterAngle;
        public float InnerAngle;
        protected float DeltaAngle;

        //Cosine Of Outer Angle For Quick Calculation In HLSL
        public float LightAngleCos;

        //Shadowing  Information
        public float DepthBias;
        public bool IsWithShadows;
        public int ShadowMapResolution;
        public RenderTarget2D ShadowMap;

        //Matrices Calculated For The Spotlight
        public Matrix World;
        public Matrix View;
        public Matrix Projection;
        public Matrix ViewProjection;

        public SpotLight(Renderer renderer, GraphicsDevice g, Info info)
        {
            r = renderer;

            Location = info.Position;
            Direction = Vector3.Normalize(info.Direction);
            OuterAngle = info.OuterAngle;
            InnerAngle = info.InnerAngle;
            DeltaAngle = OuterAngle - InnerAngle;
            LightAngleCos = (float)Math.Cos(OuterAngle);
            NearClip = info.NearPlane;
            FarClip = info.FarPlane;

            Color = info.Color;
            Intensity = info.Intensity;

            IsWithShadows = info.IsWithShadows;
            ShadowMapResolution = info.ShadowMapResolution;
            DepthBias = info.DepthBias;

            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.Min(MathHelper.Pi - 0.1f, OuterAngle * 2f), 1.0f, NearClip, FarClip);

            ShadowMap = new RenderTarget2D(
                g,
                ShadowMapResolution,
                ShadowMapResolution,
                false,
                SurfaceFormat.Single,
                DepthFormat.Depth24Stencil8
                );
            //Create View and World
            update();

            OnLightActivationEvent += addLight;
            OnLightDeactivationEvent += removeLight;
        }
        ~SpotLight()
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
        private void addLight() { r.addLight(this); }
        private void removeLight() { r.removeLight(this); }

        //Update If Location Or Direction Changes
        protected void getDirectionPitchYaw(Vector3 dir, out float pitch, out float yaw)
        {
            yaw = (float)Math.Atan2(-dir.X, -dir.Z);
            pitch = (float)Math.Atan2(dir.Y, Math.Sqrt(dir.Z * dir.Z + dir.X * dir.X));
        }
        public void update()
        {
            float pitch, yaw;
            getDirectionPitchYaw(Direction, out pitch, out yaw);

            float sPhi = (float)Math.Tan(OuterAngle);
            Matrix Scaling = Matrix.CreateScale(sPhi, sPhi, 1) * Matrix.CreateScale(FarClip);
            Matrix Translation = Matrix.CreateTranslation(Location.X, Location.Y, Location.Z);
            Matrix Rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, 0);

            //Make World
            World = Scaling * Rotation * Translation;
            //ReMake View
            View = Matrix.CreateLookAt(Location, Location + Direction, Vector3.Transform(Vector3.Up, Rotation));
            ViewProjection = View * Projection;
        }
        public void refreshProjection()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.Min(MathHelper.Pi, OuterAngle * 2f), 1.0f, NearClip, FarClip);
            ViewProjection = View * Projection;
        }

        public void setOuterAngle(float angle)
        {
            OuterAngle = angle;
            LightAngleCos = (float)Math.Cos(OuterAngle);
            DeltaAngle = OuterAngle - InnerAngle;
            refreshProjection();
        }

        public override void setParameters(GraphicsDevice g, ICamera camera, LightEffect e)
        {
            e.Parameters["World"].SetValue(World);

            e.Parameters["LightColor"].SetValue(Color);
            e.Parameters["LightIntensity"].SetValue(Intensity);

            e.Parameters["LightPosition"].SetValue(Location);
            e.Parameters["LightDirection"].SetValue(Direction);

            g.Textures[3] = ShadowMap;
            e.Parameters["Shadows"].SetValue(IsWithShadows);
            e.Parameters["ShadowMapSize"].SetValue(ShadowMapResolution);
            e.Parameters["DepthBias"].SetValue(DepthBias);

            e.Parameters["LightViewProjection"].SetValue(ViewProjection);
            e.Parameters["LightAngleCos"].SetValue(LightAngleCos);
            e.Parameters["OuterAngle"].SetValue(OuterAngle);
            e.Parameters["DeltaAngle"].SetValue(DeltaAngle);
            e.Parameters["LightDistance"].SetValue(FarClip);

            #region Set Cull Mode
            Vector3 L = camera.Location - Location;
            L.Normalize();
            float SL = Vector3.Dot(L, Direction);
            if (SL >= LightAngleCos)
                g.RasterizerState = RasterizerState.CullClockwise;
            else
                g.RasterizerState = RasterizerState.CullCounterClockwise;
            #endregion
        }

        public struct Info
        {
            public Vector3 Position;
            public Vector3 Direction;
            public float OuterAngle;
            public float InnerAngle;
            public float NearPlane;
            public float FarPlane;

            public Vector4 Color;
            public float Intensity;

            public bool IsWithShadows;
            public int ShadowMapResolution;
            public float DepthBias;

            public Info(
                Vector3 pos, Vector3 dir, Vector4 c, float intensity = 1f, float outerAng = MathHelper.PiOver4, float innerAng = 0.1f,
                float near = 0.1f, float far = 100f, bool isWithShadows = false, int shadowRes = 256, float dBias = 1f / 500f)
            {
                Position = pos;
                Direction = dir;
                OuterAngle = outerAng;
                InnerAngle = innerAng;
                NearPlane = near;
                FarPlane = far;

                Color = c;
                Intensity = intensity;
                IsWithShadows = isWithShadows;
                ShadowMapResolution = shadowRes;
                DepthBias = dBias;
            }
        }
        public class Renderer : ACLightRenderer<SpotLight>
        {
            public Renderer(LightEffect e) : base(e) { }

            public override void drawAllLights(GraphicsDevice g, ICamera camera, DeferredRenderer renderer)
            {
                //Set Spot Lights Geometry Buffers
                g.SetVertexBuffer(SpotLight.vbModel, 0);
                g.Indices = SpotLight.ibModel;
                fxLight.setCurrentTechnique(LightEffect.TechniqueSpot);
                //Draw Spot Lights
                foreach (SpotLight light in lights)
                {
                    light.setParameters(g, camera, fxLight);
                    fxLight.applyPass(LightEffect.PassLighting);
                    g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, SpotLight.Faces * 6, 0, SpotLight.Faces * 2);
                }
            }
            public void drawAllShadows(GraphicsDevice g, ICamera camera, DeferredRenderer renderer)
            {
                //Set States
                g.BlendState = BlendState.Opaque;
                g.DepthStencilState = DepthStencilState.Default;
                g.RasterizerState = RasterizerState.CullCounterClockwise;
                fxLight.setCurrentTechnique(LightEffect.TechniqueSpot);
                //Foreach SpotLight with Shadows
                foreach (SpotLight Light in lights)
                {
                    //Draw it's Shadow Map
                    if (Light.IsWithShadows)
                    {
                        //Set Light's Target onto the Graphics Device
                        g.SetRenderTarget(Light.ShadowMap);
                        g.Clear(Microsoft.Xna.Framework.Color.Transparent);

                        //Set global Effect parameters
                        fxLight.Parameters["LightViewProjection"].SetValue(Light.View * Light.Projection);

                        //Draw Models
                        renderer.drawShadowScene(g, null, fxLight.asEffect());
                        fxLight.applyPass(LightEffect.PassShadowing);
                    }
                }
            }
        }
    }
}
