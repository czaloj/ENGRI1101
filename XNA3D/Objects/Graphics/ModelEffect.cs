using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNA3D.Cameras;

namespace XNA3D.Objects.Graphics
{
    public class ModelEffect
    {
        //The Effect File
        public Effect effect;

        public ModelEffect(Effect e)
        {
            effect = e;
            effect.CurrentTechnique = effect.Techniques[0];
        }

        //The World Light Color
        public Vector4 ambientLightColor = Color.White.ToVector4();

        //Fog Parameters
        public Vector4 fogColor = Color.WhiteSmoke.ToVector4();
        public float fogNear = 0f;
        public float fogFar = 100f;

        //For Limb Highlighting Use
        public float highlightAmount = 0f;

        public void setPrerequisites(ACCamera camera)
        {
            effect.Parameters["View"].SetValue(camera.View);
            effect.Parameters["Projection"].SetValue(camera.Projection);
            effect.Parameters["CameraPosition"].SetValue(camera.Location);
            effect.Parameters["FogColor"].SetValue(fogColor);
            effect.Parameters["FogNear"].SetValue(fogNear);
            effect.Parameters["FogFar"].SetValue(fogFar);
            effect.Parameters["WorldLightColor"].SetValue(ambientLightColor);
        }
    }
}
