using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA3D.Cameras;
using XNA3D.DeferredRendering;

namespace XNA3D.DeferredRendering.Lights
{
    public class Sun : ACLight
    {
        public Renderer r;

        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 Up;
        public Vector2 Size;
        public float Distance;

        public Vector4 Color;
        public float Intensity;

        public Matrix World;
        public Matrix View;
        public Matrix Projection;

        //Shadow Information
        public bool IsWithShadows;
        public int ShadowMapResolution;
        public float DepthBias;
        //The Shadow Map Of The Spotlight (If Shadows Are Used)
        public RenderTarget2D ShadowMap;

        public Sun(Renderer renderer,GraphicsDevice g,Info info): base()
        {
            r = renderer;

            Position = info.Position;
            Size = info.Size;
            Distance = info.Distance;
            setDirection(info.Direction, info.Up);
            Color = info.Color;
            Intensity = info.Intensity;
            IsWithShadows = info.IsWithShadows;
            ShadowMapResolution = info.ShadowMapResolution;
            ShadowMap = new RenderTarget2D(
                g,
                ShadowMapResolution,
                ShadowMapResolution,
                false,
                SurfaceFormat.Single,
                DepthFormat.Depth24Stencil8
                );
            DepthBias = info.DepthBias;
            OnLightActivationEvent += addLight;
            OnLightDeactivationEvent += removeLight;
        }
        ~Sun()
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

        public void setDirection(Vector3 direction, Vector3 up)
        {
            Direction = direction;
            Up = up;
            View = Matrix.CreateLookAt(Position, Position + Direction, Up);
            Projection = Matrix.CreateOrthographic(Size.X, Size.Y, 0.5f, Distance);
        }

        public override void setParameters(GraphicsDevice g, ICamera camera, LightEffect e)
        {
            e.Parameters["LightColor"].SetValue(Color);
            e.Parameters["LightIntensity"].SetValue(Intensity);
            
            e.Parameters["LightPosition"].SetValue(Position);
            e.Parameters["LightDirection"].SetValue(Direction);
            e.Parameters["LightDistance"].SetValue(Distance);

            g.Textures[3] = ShadowMap;
            e.Parameters["Shadows"].SetValue(IsWithShadows);
            e.Parameters["ShadowMapSize"].SetValue(ShadowMapResolution);
            e.Parameters["DepthBias"].SetValue(DepthBias);
            e.Parameters["LightViewProjection"].SetValue(View * Projection);

            g.RasterizerState = RasterizerState.CullCounterClockwise;
        }

        public struct Info
        {
            public Vector3 Position;
            public Vector3 Direction;
            public Vector3 Up;
            public Vector2 Size;
            public float Distance;

            public Vector4 Color;
            public float Intensity;

            public bool IsWithShadows;
            public int ShadowMapResolution;
            public float DepthBias;

            public Info(Vector3 pos, Vector3 dir, Vector3 up, Vector2 s, float dist, Vector4 c, float intensity = 1f, bool isWithShadows = false, int shadowRes = 256, float dBias = 1 / 500f)
            {
                Position = pos;
                Direction = dir;
                Up = up;
                Size = s;
                Distance = dist;
                Color = c;
                Intensity = intensity;
                IsWithShadows = isWithShadows;
                ShadowMapResolution = shadowRes;
                DepthBias = dBias;
            }
        }
        public class Renderer : ACLightRenderer<Sun>
        {
            public Renderer(LightEffect e) : base(e) { }

            public override void drawAllLights(GraphicsDevice g, ICamera camera, DeferredRenderer renderer)
            {
                renderer.fsq.ReadyBuffers(g);
                fxLight.setCurrentTechnique(LightEffect.TechniqueSun);
                foreach (Sun light in lights)
                {
                    light.setParameters(g, camera, fxLight);
                    fxLight.applyPass(LightEffect.PassLighting);
                    renderer.fsq.JustDraw(g);
                }
            }
            public void drawAllShadows(GraphicsDevice g, ICamera camera, DeferredRenderer renderer)
            {
                //Set States
                g.BlendState = BlendState.Opaque;
                g.DepthStencilState = DepthStencilState.Default;
                g.RasterizerState = RasterizerState.CullNone;
                fxLight.setCurrentTechnique(LightEffect.TechniqueSun);
                //Foreach SpotLight with Shadows
                foreach (Sun Light in lights)
                {
                    //Draw it's Shadow Map
                    if (Light.IsWithShadows)
                    {
                        g.SetRenderTarget(Light.ShadowMap);
                        g.Clear(Microsoft.Xna.Framework.Color.Transparent);

                        fxLight.Parameters["LightViewProjection"].SetValue(Light.View * Light.Projection);

                        renderer.drawShadowScene(g, null, fxLight.asEffect());
                    }
                }
            }
        }
    }
}
