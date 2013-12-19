using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XNA3D.Cameras;

namespace XNA3D.DeferredRendering.Lights
{
    public class DirectionalLight : ACLight
    {
        public Renderer renderer;
        
        public Vector3 Direction;
        public Vector4 Color;
        public float Intensity;

        public DirectionalLight(Renderer r, Info info)
            : base()
        {
            renderer = r;

            Direction = Vector3.Normalize(info.Direction);
            Color = info.Color;
            Intensity = info.Intensity;

            OnLightActivationEvent += addLight;
            OnLightDeactivationEvent += removeLight;
        }
        ~DirectionalLight()
        {
            OnLightActivationEvent -= addLight;
            OnLightDeactivationEvent -= removeLight;
        }
        public override void Dispose()
        {
            base.Dispose();
            OnLightActivationEvent -= addLight;
            OnLightDeactivationEvent -= removeLight;
        }

        private void addLight() { renderer.addLight(this); }
        private void removeLight() { renderer.removeLight(this); }

        public override void setParameters(GraphicsDevice g, ICamera camera, LightEffect e)
        {
            //Set Necessary Parameters
            e.Parameters["LightColor"].SetValue(Color);
            e.Parameters["LightIntensity"].SetValue(Intensity);
            
            //Set Directional Light Parameters
            e.Parameters["LightDirection"].SetValue(Direction);
        }

        public struct Info
        {
            public Vector3 Direction;
            public Vector4 Color;
            public float Intensity;

            public Info(Vector3 dir, Vector4 c, float intensity = 1f)
            {
                Direction = dir;
                Color = c;
                Intensity = intensity;
            }
        }
        public class Renderer : ACLightRenderer<DirectionalLight>
        {
            public Renderer(LightEffect e) : base(e) { }

            public override void drawAllLights(GraphicsDevice g, ICamera camera, DeferredRenderer renderer)
            {
                //Set the Directional Lights Geometry Buffers
                renderer.fsq.ReadyBuffers(g);
                fxLight.setCurrentTechnique(LightEffect.TechniqueDirectional);
                foreach (DirectionalLight light in lights)
                {
                    light.setParameters(g, camera, fxLight);
                    fxLight.applyPass(LightEffect.PassLighting);
                    renderer.fsq.JustDraw(g);
                }
            }
        }
    }
}
