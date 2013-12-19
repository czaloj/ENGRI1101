using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA3D.Cameras
{
    public class FPSCamera : FreeCamera
    {
        public Vector3 orientation = Vector3.Zero;

        public void setOrientation(Vector3 pyr)
        {
            orientation = pyr;
            setRotation(Matrix.CreateFromYawPitchRoll(orientation.Y, orientation.X, orientation.Z));
        }

        public void addPitch(float amount)
        {
            orientation.X += amount;
            setRotation(Matrix.CreateFromYawPitchRoll(orientation.Y, orientation.X, orientation.Z));
        }
        public void addYaw(float amount)
        {
            orientation.Y += amount;
            setRotation(Matrix.CreateFromYawPitchRoll(orientation.Y, orientation.X, orientation.Z));
        }
        public void addRoll(float amount)
        {
            orientation.Z += amount;
            setRotation(Matrix.CreateFromYawPitchRoll(orientation.Y, orientation.X, orientation.Z));
        }
    }
}
