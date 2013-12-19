using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA3D.Cameras
{
    public class FreeCamera : ACCamera
    {
        public FreeCamera()
            : base()
        {

        }

        public void pitch(float angle)
        {
            rotate(Matrix.CreateRotationX(angle));
        }
        public void yaw(float angle)
        {
            rotate(Matrix.CreateRotationY(angle));
        }
        public void roll(float angle)
        {
            rotate(Matrix.CreateRotationZ(angle));
        }

        public void move(Vector3 movement)
        {
            location += forward * movement.Z;
            location += up * movement.Y;
            location += right * movement.X;
            refreshView();
        }
        public void moveForward(float amount)
        {
            location += forward * amount;
            refreshView();
        }
        public void moveUp(float amount)
        {
            location += up * amount;
            refreshView();
        }
        public void moveRight(float amount)
        {
            location += right * amount;
            refreshView();
        }
    }
}
