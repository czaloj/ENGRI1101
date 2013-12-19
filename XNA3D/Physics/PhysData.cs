using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA3D.Physics
{
    public struct PhysData
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 Acceleration;

        public PhysData(Vector3 p, Vector3 v, Vector3 a)
        {
            Position = p;
            Velocity = v;
            Acceleration = a;
        }
        public PhysData(Vector3 p, Vector3 v)
            : this(p, v, Vector3.Zero)
        {
        }
        public PhysData(Vector3 p)
            : this(p, Vector3.Zero, Vector3.Zero)
        {
        }

        public static void integrate(PhysData cur, float dt, out PhysData n)
        {
            n = new PhysData(
                cur.Position + cur.Velocity * dt + cur.Acceleration * dt * dt / 2f,
                cur.Velocity + cur.Acceleration * dt,
                cur.Acceleration
                );
        }
    }
}
