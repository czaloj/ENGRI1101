using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework
{
    public struct Vector3i
    {
        public static readonly Vector3i Zero = new Vector3i(0, 0, 0);
        public static readonly Vector3i One = new Vector3i(1, 1, 1);
        public static readonly Vector3i UnitX = new Vector3i(1, 0, 0);
        public static readonly Vector3i UnitY = new Vector3i(0, 1, 0);
        public static readonly Vector3i UnitZ = new Vector3i(0, 0, 1);

        public int X, Y, Z;

        public Vector3i(int x, int y, int z)
        {
            X = x; Y = y; Z = z;
        }
        public Vector3i(float x, float y, float z)
            : this((int)x, (int)y, (int)z) { }
        public Vector3i(Vector3 v)
            : this((int)v.X, (int)v.Y, (int)v.Z) { }

        #region Component-Wise Math
        public static Vector3i operator +(Vector3i v1, Vector3i v2)
        {
            return new Vector3i(
                v1.X + v2.X,
                v1.Y + v2.Y,
                v1.Z + v2.Z
                );
        }
        public static Vector3i operator -(Vector3i v1, Vector3i v2)
        {
            return new Vector3i(
                v1.X - v2.X,
                v1.Y - v2.Y,
                v1.Z - v2.Z
                );
        }
        public static Vector3i operator *(Vector3i v1, Vector3i v2)
        {
            return new Vector3i(
                v1.X * v2.X,
                v1.Y * v2.Y,
                v1.Z * v2.Z
                );
        }
        public static Vector3i operator /(Vector3i v1, Vector3i v2)
        {
            return new Vector3i(
                v1.X / v2.X,
                v1.Y / v2.Y,
                v1.Z / v2.Z
                );
        }
        #endregion

        public static bool operator ==(Vector3i v1, Vector3i v2)
        {
            return
                v1.X == v2.X &&
                v1.Y == v2.Y &&
                v1.Z == v2.Z
                ;
        }
        public static bool operator !=(Vector3i v1, Vector3i v2)
        {
            return
                v1.X != v2.X ||
                v1.Y != v2.Y ||
                v1.Z != v2.Z
                ;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Vector3 Vector3
        {
            get { return new Vector3(X, Y, Z); }
        }

        public override string ToString()
        {
            return X + " " + Y + " " + Z;
        }
    }
}
