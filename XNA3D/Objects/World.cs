using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA3D.Objects
{
    public static class World
    {
        public static Matrix WorldMatrix { get; set; }

        static World()
        {
            //Set The World To A Default Position
            WorldMatrix = Matrix.Identity;
        }

        public static void reset()
        {
            WorldMatrix = Matrix.Identity;
        }
        public static void translate(Vector3 v)
        {
            WorldMatrix = WorldMatrix * Matrix.CreateTranslation(v);
        }
        public static void rotate(Vector3 rotation)
        {
            WorldMatrix = WorldMatrix * Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        }
        public static void rotateX(float amount)
        {
            WorldMatrix = WorldMatrix * Matrix.CreateRotationX(amount);
        }
        public static void rotateY(float amount)
        {
            WorldMatrix = WorldMatrix * Matrix.CreateRotationY(amount);
        }
        public static void rotateZ(float amount)
        {
            WorldMatrix = WorldMatrix * Matrix.CreateRotationZ(amount);
        }
    }
}
