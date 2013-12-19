using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XNA3D.Cameras;
using XNA3D.DeferredRendering;
using XNA3D.DeferredRendering.Lights;

namespace VoxEng.Graphics.Deferred.Lights
{
    public class VoxelSunRenderer : ACLightRenderer
    {
        public LinkedList<VoxelSun> lights;
        protected Effect lightEffect;
        protected Effect depthEffect;

        private static DeferredRenderer renderer;
        public static void setDeferredRenderer(DeferredRenderer _renderer)
        {
            renderer = _renderer;
            renderer.OnShadowMapDrawEvent += VoxelSun.Renderer.DrawAllShadows;
        }

        public VoxelSunRenderer()
        {
            lights = new LinkedList<VoxelSun>();
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

        public void addLight(VoxelSun light)
        {
            lights.AddLast(light);
        }
        public void removeLight(VoxelSun light)
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
            //g.SetVertexBuffer(VoxelSun.vbModel, 0);
            //g.Indices = VoxelSun.ibModel;
            //Draw Spot Lights
            renderer.fsq.ReadyBuffers(g);
            foreach (VoxelSun light in lights)
            {
                light.draw(g, camera, lightEffect);
                renderer.fsq.JustDraw(g);
                ////Draw
                //g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 24, 0, 12);
            }
        }
        public void DrawAllShadows(GraphicsDevice g, ACCamera camera, Effect e)
        {
            //Set States
            g.BlendState = BlendState.Opaque;
            g.DepthStencilState = DepthStencilState.Default;
            g.RasterizerState = RasterizerState.CullCounterClockwise;
            //Foreach SpotLight with Shadows
            foreach (VoxelSun Light in lights)
            {
                //Draw it's Shadow Map
                if (Light.isWithShadows && Light.RefreshShadows)
                {
                    //Light.RefreshShadows = false;

                    //Set Light's Target onto the Graphics Device
                    g.SetRenderTarget(Light.ShadowMap);
                    //Clear Target
                    g.Clear(Color.Transparent);
                    //Set global Effect parameters
                    depthEffect.Parameters["View"].SetValue(Light.View);
                    depthEffect.Parameters["Projection"].SetValue(Light.Projection);
                    //depthEffect.Parameters["LightTopPlane"].SetValue(Light.LightBound.Max.Y);
                    //depthEffect.Parameters["LightFarPlane"].SetValue(Light.LightBound.Radius * 2f);
                    //depthEffect.Parameters["LightDirection"].SetValue(Light.ShineDirection);

                    g.RasterizerState = RasterizerState.CullNone;

                    //Draw Models
                    renderer.drawShadowScene(g, null, depthEffect);

                    //Draw A Horizon Line
                    g.SetVertexBuffer(VoxelSun.vbShadowShield);
                    g.Indices = VoxelSun.ibShadowShield;
                    g.RasterizerState = RasterizerState.CullCounterClockwise;
                    depthEffect.Parameters["World"].SetValue(Matrix.CreateScale(Light.LightSize.Radius) * Matrix.CreateTranslation(Light.LightSize.Center));
                    depthEffect.CurrentTechnique.Passes[0].Apply();
                    g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VoxelSun.vbShadowShield.VertexCount, 0, VoxelSun.ibShadowShield.IndexCount / 3);
                }
            }
        }
    }
}
