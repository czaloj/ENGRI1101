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
    public class SunlightRenderer : ACLightRenderer
    {
        public LinkedList<Sunlight> lights;
        protected Effect lightEffect;
        protected Effect depthEffect;

        private static DeferredRenderer renderer;
        public static void setDeferredRenderer(DeferredRenderer _renderer)
        {
            renderer = _renderer;
            renderer.OnShadowMapDrawEvent += Sunlight.Renderer.DrawAllShadows;
        }

        public SunlightRenderer()
        {
            lights = new LinkedList<Sunlight>();
        }

        public void loadEffect(string file, ContentManager content)
        {
            setEffect(content.Load<Effect>(file));
        }
        public void setEffect(Effect e)
        {
            lightEffect = e;
            lightEffect.CurrentTechnique = lightEffect.Techniques[0];
        }
        public void loadShadowEffect(string file, ContentManager content)
        {
            setShadowEffect(content.Load<Effect>(file));
        }
        public void setShadowEffect(Effect e)
        {
            depthEffect = e;
            depthEffect.CurrentTechnique = depthEffect.Techniques[0];
        }

        public void addLight(Sunlight light)
        {
            lights.AddLast(light);
        }
        public void removeLight(Sunlight light)
        {
            lights.Remove(light);
        }

        public override void drawAllLights(GraphicsDevice g, ACCamera camera, DeferredRenderer renderer)
        {
            //Set Spot Lights Globals
            lightEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            lightEffect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            lightEffect.Parameters["InverseView"].SetValue(renderer.LBuffer.InverseView);
            lightEffect.Parameters["InverseViewProjection"].SetValue(renderer.LBuffer.InverseViewProjection);
            lightEffect.Parameters["CameraPosition"].SetValue(camera.Location);
            lightEffect.Parameters["GBufferTextureSize"].SetValue(renderer.GBufferTextureSize);
            //Set Spot Lights Geometry Buffers
            g.SetVertexBuffer(Sunlight.vbModel, 0);
            g.Indices = Sunlight.ibModel;
            //Draw Spot Lights
            foreach (Sunlight light in lights)
            {
                light.draw(g, camera, lightEffect);
                //Draw
                g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);
            }
        }
        public void DrawAllShadows(GraphicsDevice GraphicsDevice, ACCamera Camera, Effect e)
        {
            //Set States
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            //Foreach SpotLight with Shadows
            foreach (Sunlight Light in lights)
            {
                //Draw it's Shadow Map
                if (Light.isWithShadows)
                {
                        //Set Light's Target onto the Graphics Device
                        GraphicsDevice.SetRenderTarget(Light.ShadowMap);
                        //Clear Target
                        GraphicsDevice.Clear(Color.Transparent);
                        //Set global Effect parameters
                        depthEffect.Parameters["ViewProjection"].SetValue(Light.View * Light.Projection);
                        depthEffect.Parameters["LightTopPlane"].SetValue(Light.LightBound.Max.Y);
                        depthEffect.Parameters["LightFarPlane"].SetValue(Light.BoxSize.Y);
                        depthEffect.Parameters["LightDirection"].SetValue(Light.ShineDirection);

                        //Draw Models
                        renderer.drawShadowScene(GraphicsDevice, null, depthEffect);
                }
            }
        }
    }
}
