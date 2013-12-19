using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA3D.DeferredRendering.Lights
{
    public class LightEffect
    {
        public const int DefaultTechniqueCount = 4;
        public const int TechniqueDirectional = 0;
        public const int TechniqueSpot = 1;
        public const int TechniquePoint = 2;
        public const int TechniqueSun = 3;

        public const int PassLighting = 0;
        public const int PassShadowing = 1;
        public const int PassShadowingSkinned = 2;

        protected int techniqueCount;
        protected Effect fxLight;
        protected EffectTechnique[] techniques;
        public EffectParameterCollection Parameters
        {
            get
            {
                return fxLight.Parameters;
            }
        }

        public LightEffect(Effect e, params string[] extraTechniques)
        {
            techniqueCount = DefaultTechniqueCount + (extraTechniques == null ? 0 : extraTechniques.Length);
            techniques = new EffectTechnique[techniqueCount];

            fxLight = e;
            techniques[TechniqueDirectional] = fxLight.Techniques["Directional"];
            techniques[TechniqueSpot] = fxLight.Techniques["Spot"];
            techniques[TechniquePoint] = fxLight.Techniques["Point"];
            techniques[TechniqueSun] = fxLight.Techniques["Sun"];
            for (int i = DefaultTechniqueCount; i < techniqueCount; i++)
            {
                techniques[i] = fxLight.Techniques[extraTechniques[i - DefaultTechniqueCount]];
            }
        }
        public Effect asEffect()
        {
            return fxLight;
        }

        public void setCurrentTechnique(int index)
        {
            fxLight.CurrentTechnique = techniques[index];
        }
        public void setCurrentTechnique(string name)
        {
            fxLight.CurrentTechnique = fxLight.Techniques[name];
        }
        public void applyPass(int index)
        {
            fxLight.CurrentTechnique.Passes[index].Apply();
        }
    }
}
