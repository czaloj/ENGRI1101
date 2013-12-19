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
    public class SpotLightRenderer : ACLightRenderer
    {
        public LinkedList<SpotLight> lights;
        protected LightEffect fxLight;

        private static DeferredRenderer renderer;
        public static void setDeferredRenderer(DeferredRenderer _renderer)
        {
            renderer = _renderer;
            renderer.OnShadowMapDrawEvent += SpotLight.Renderer.drawAllShadows;
        }

        public SpotLightRenderer(LightEffect e)
        {
            lights = new LinkedList<SpotLight>();
            fxLight = e;
        }

        public void addLight(SpotLight light)
        {
            lights.AddLast(light);
        }
        public void removeLight(SpotLight light)
        {
            lights.Remove(light);
        }

        public override void drawAllLights(GraphicsDevice g, ACCamera camera, DeferredRenderer renderer)
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
        public void drawAllShadows(GraphicsDevice g, ACCamera camera, Effect e)
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
                if (Light.isWithShadows)
                {
                    //Set Light's Target onto the Graphics Device
                    g.SetRenderTarget(Light.ShadowMap);
                    //Clear Target
                    g.Clear(Color.Transparent);
                    //Set global Effect parameters
                    fxLight.Parameters["LightViewProjection"].SetValue(Light.ViewMatrix * Light.ProjectionMatrix);

                    //Draw Models
                    renderer.drawShadowScene(g, null, fxLight.asEffect());
                    fxLight.applyPass(LightEffect.PassShadowing);
                }
            }
        }
    }
}
