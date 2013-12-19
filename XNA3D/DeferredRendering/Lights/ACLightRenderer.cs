using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using XNA3D.Cameras;

namespace XNA3D.DeferredRendering.Lights
{
    public delegate void OnLightDraw(GraphicsDevice g, ICamera camera, DeferredRenderer renderer);

    public abstract class ACLightRenderer<T> where T : ACLight
    {
        public LinkedList<T> lights;
        protected LightEffect fxLight;

        public ACLightRenderer(LightEffect e)
        {
            lights = new LinkedList<T>();
            fxLight = e;
        }

        public void addLight(T light)
        {
            lights.AddLast(light);
        }
        public void removeLight(T light)
        {
            lights.Remove(light);
        }

        public abstract void drawAllLights(GraphicsDevice g, ICamera camera, DeferredRenderer renderer);
    }
}
