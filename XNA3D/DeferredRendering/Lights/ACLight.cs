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
    public delegate void OnLightActivation();

    public abstract class ACLight : IDisposable
    {
        public static int TotalLights { get; private set; }
        public static int ActiveLights { get; private set; }

        static ACLight()
        {
            TotalLights = 0;
            ActiveLights = 0;
        }

        private static void newActive() { ActiveLights++; }
        private static void newDeactive() { ActiveLights--; }

        protected bool isActive;
        public bool IsActive
        {
            get
            {
                return isActive;
            }
            protected set
            {
                isActive = value;
            }
        }

        public event OnLightActivation OnLightActivationEvent;
        public event OnLightActivation OnLightDeactivationEvent;

        public ACLight()
        {
            TotalLights++;
            isActive = false;
            OnLightActivationEvent += newActive;
            OnLightDeactivationEvent += newDeactive;
        }
        ~ACLight()
        {
            TotalLights--;
            setActive(false);
            OnLightActivationEvent -= newActive;
            OnLightDeactivationEvent -= newDeactive;
        }
        public virtual void Dispose()
        {
            System.GC.SuppressFinalize(this);
            TotalLights--;
            setActive(false);
            OnLightActivationEvent -= newActive;
            OnLightDeactivationEvent -= newDeactive;
        }

        public void setActive(bool b)
        {
            if (isActive != b)
            {
                isActive = b;
                if (isActive) { OnLightActivationEvent(); }
                else { OnLightDeactivationEvent(); }
            }
        }

        public abstract void setParameters(GraphicsDevice g, ICamera camera, LightEffect e);
    }
}
