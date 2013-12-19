using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework
{
    public struct Vector2i
    {
        public static readonly Vector2i Zero = new Vector2i(0, 0);
        public static readonly Vector2i One = new Vector2i(1, 1);
        public static readonly Vector2i UnitX = new Vector2i(1, 0);
        public static readonly Vector2i UnitY = new Vector2i(0, 1);

        public int X, Y;

        public Vector2i(int x, int y)
        {
            X = x; Y = y;
        }
        public Vector2i(float x, float y)
            : this((int)x, (int)y) { }
        public Vector2i(Vector2 v)
            : this((int)v.X, (int)v.Y) { }

        #region Component-Wise Math
        public static Vector2i operator +(Vector2i v1, Vector2i v2)
        {
            return new Vector2i(
                v1.X + v2.X,
                v1.Y + v2.Y
                );
        }
        public static Vector2i operator -(Vector2i v1, Vector2i v2)
        {
            return new Vector2i(
                v1.X - v2.X,
                v1.Y - v2.Y
                );
        }
        public static Vector2i operator *(Vector2i v1, Vector2i v2)
        {
            return new Vector2i(
                v1.X * v2.X,
                v1.Y * v2.Y
                );
        }
        public static Vector2i operator /(Vector2i v1, Vector2i v2)
        {
            return new Vector2i(
                v1.X / v2.X,
                v1.Y / v2.Y
                );
        }
        #endregion

        public static bool operator ==(Vector2i v1, Vector2i v2)
        {
            return
                v1.X == v2.X &&
                v1.Y == v2.Y
                ;
        }
        public static bool operator !=(Vector2i v1, Vector2i v2)
        {
            return
                v1.X != v2.X ||
                v1.Y != v2.Y
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

        public Vector2 Vector2
        {
            get { return new Vector2(X, Y); }
        }

        public override string ToString()
        {
            return X + " " + Y;
        }
    }
}
