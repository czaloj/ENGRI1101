using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA3D.Objects;

namespace XNA3D.Cameras
{
    public class ConstrainedCamera : FreeCamera
    {
        GameObject target;

        protected Matrix oRotation;
        protected Matrix oPosition;

        public void setConstraints(Vector3 off, Vector3 pyr)
        {
            oPosition = Matrix.CreateTranslation(off);
            oRotation = Matrix.CreateFromYawPitchRoll(pyr.Y, pyr.X, pyr.Z);
        }

        public void setTarget(GameObject target)
        {
            this.target = target;
        }

        public void constrain()
        {
            rotation = oRotation * Matrix.CreateFromQuaternion(target.Transform.Rotation);
            refreshRotation();
            location = Vector3.Transform(Vector3.Zero, oPosition * target.World);
            refreshView();
        }
    }
}
