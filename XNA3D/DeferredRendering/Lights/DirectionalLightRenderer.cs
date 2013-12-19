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
    public class DirectionalLightRenderer : ACLightRenderer
    {
        public LinkedList<DirectionalLight> lights;
        protected LightEffect fxLight;

        public DirectionalLightRenderer(LightEffect e)
        {
            lights = new LinkedList<DirectionalLight>();
            fxLight = e;
        }

        public void addLight(DirectionalLight light)
        {
            lights.AddLast(light);
        }
        public void removeLight(DirectionalLight light)
        {
            lights.Remove(light);
        }

        public override void drawAllLights(GraphicsDevice GraphicsDevice, ACCamera Camera, DeferredRenderer renderer)
        {
            //Set the Directional Lights Geometry Buffers
            renderer.fsq.ReadyBuffers(GraphicsDevice);
            fxLight.setCurrentTechnique(LightEffect.TechniqueDirectional);
            foreach (DirectionalLight light in lights)
            {
                light.setParameters(GraphicsDevice, Camera, fxLight);
                fxLight.applyPass(LightEffect.PassLighting);
                renderer.fsq.JustDraw(GraphicsDevice);
            }
        }
    }
}
