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
    public class PointLightRenderer : ACLightRenderer
    {
        public LinkedList<PointLight> lights;
        protected Effect lightEffect;
        protected Effect depthEffect;

        public static DeferredRenderer renderer;
        public static void setDeferredRenderer(DeferredRenderer _renderer)
        {
            renderer = _renderer;
            renderer.OnShadowMapDrawEvent += PointLight.Renderer.drawAllShadows;
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

        public PointLightRenderer()
        {
            lights = new LinkedList<PointLight>();
        }

        public void addLight(PointLight light)
        {
            lights.AddLast(light);
        }
        public void removeLight(PointLight light)
        {
            lights.Remove(light);
        }

        public override void drawAllLights(GraphicsDevice g, ACCamera camera, DeferredRenderer renderer)
        {
            //Set Point Lights Geometry Buffers
            g.SetVertexBuffer(PointLight.vbModel, 0);
            g.Indices = PointLight.ibModel;
            //Set Point Lights Globals
            lightEffect.Parameters["inverseView"].SetValue(renderer.LBuffer.InverseView);
            lightEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            lightEffect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            lightEffect.Parameters["InverseViewProjection"].SetValue(renderer.LBuffer.InverseViewProjection);
            lightEffect.Parameters["CameraPosition"].SetValue(camera.Location);
            lightEffect.Parameters["GBufferTextureSize"].SetValue(renderer.GBufferTextureSize);
            foreach (PointLight light in lights)
            {
                light.draw(g, camera, lightEffect);
            }
        }
        public void drawAllShadows(GraphicsDevice g, ACCamera camera, Effect e)
        {
            //Set States
            g.BlendState = BlendState.Opaque;
            g.DepthStencilState = DepthStencilState.Default;
            g.RasterizerState = RasterizerState.CullCounterClockwise;
            foreach (PointLight Light in lights)
            {
                //Draw it's Shadow Map
                if (Light.IsWithShadows)
                {
                    //Initialize View Matrices Array
                    Matrix[] views = new Matrix[6];
                    //Create View Matrices
                    views[0] = Matrix.CreateLookAt(Light.Position, Light.Position + Vector3.Forward, Vector3.Up);
                    views[1] = Matrix.CreateLookAt(Light.Position, Light.Position + Vector3.Backward, Vector3.Up);
                    views[2] = Matrix.CreateLookAt(Light.Position, Light.Position + Vector3.Left, Vector3.Up);
                    views[3] = Matrix.CreateLookAt(Light.Position, Light.Position + Vector3.Right, Vector3.Up);
                    views[4] = Matrix.CreateLookAt(Light.Position, Light.Position + Vector3.Down, Vector3.Forward);
                    views[5] = Matrix.CreateLookAt(Light.Position, Light.Position + Vector3.Up, Vector3.Backward);
                    //Create Projection Matrix
                    Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1.0f, 0.05f, Light.Radius);
                    //Set Global Effect Value
                    depthEffect.Parameters["LightPosition"].SetValue(Light.Position);
                    depthEffect.Parameters["LightFarPlane"].SetValue(Light.Radius);

                    #region Forward
                    g.SetRenderTarget(Light.ShadowMap, CubeMapFace.PositiveZ);
                    //Clear Target
                    g.Clear(Color.Transparent);
                    //Set global Effect parameters
                    depthEffect.Parameters["ViewProjection"].SetValue(views[0] * projection);
                    //Draw Models
                    renderer.drawShadowScene(g, null, depthEffect);
                    #endregion
                    #region Backward
                    g.SetRenderTarget(Light.ShadowMap, CubeMapFace.NegativeZ);
                    //Clear Target
                    g.Clear(Color.Transparent);
                    //Set global Effect parameters
                    depthEffect.Parameters["ViewProjection"].SetValue(views[1] * projection);
                    //Draw Models
                    renderer.drawShadowScene(g, null, depthEffect);
                    #endregion
                    
                    #region Left
                    g.SetRenderTarget(Light.ShadowMap, CubeMapFace.NegativeX);
                    //Clear Target
                    g.Clear(Color.Transparent);
                    //Set global Effect parameters
                    depthEffect.Parameters["ViewProjection"].SetValue(views[2] * projection);
                    //Draw Models
                    renderer.drawShadowScene(g, null, depthEffect);
                    #endregion
                    #region Right
                    g.SetRenderTarget(Light.ShadowMap, CubeMapFace.PositiveX);
                    //Clear Target
                    g.Clear(Color.Transparent);
                    //Set global Effect parameters
                    depthEffect.Parameters["ViewProjection"].SetValue(views[3] * projection);
                    //Draw Models
                    renderer.drawShadowScene(g, null, depthEffect);
                    #endregion
                    
                    #region Down
                    g.SetRenderTarget(Light.ShadowMap, CubeMapFace.NegativeY);
                    //Clear Target
                    g.Clear(Color.Transparent);
                    //Set global Effect parameters
                    depthEffect.Parameters["ViewProjection"].SetValue(views[4] * projection);
                    //Draw Models
                    renderer.drawShadowScene(g, null, depthEffect);
                    #endregion
                    #region Up
                    g.SetRenderTarget(Light.ShadowMap, CubeMapFace.PositiveY);
                    //Clear Target
                    g.Clear(Color.Transparent);
                    //Set global Effect parameters
                    depthEffect.Parameters["ViewProjection"].SetValue(views[5] * projection);
                    //Draw Models
                    renderer.drawShadowScene(g, null, depthEffect);
                    #endregion
                }
            }
        }
    }
}
