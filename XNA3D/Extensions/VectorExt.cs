using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework
{
    public static class Vector3Ext
    {
        public static void absolute(this Vector3 v, out Vector3 abs)
        {
            abs = new Vector3(
                v.X < 0f ? -v.X : v.X,
                v.Y < 0f ? -v.Y : v.Y,
                v.Z < 0f ? -v.Z : v.Z
                );
        }
        public static void absolute(ref Vector3 v)
        {
            if (v.X < 0f) { v.X = -v.X; }
            if (v.Y < 0f) { v.Y = -v.Y; }
            if (v.Z < 0f) { v.Z = -v.Z; }
        }

        public static Vector3 sqrt(Vector3 v)
        {
            return new Vector3(
                (float)Math.Sqrt(v.X),
                (float)Math.Sqrt(v.Y),
                (float)Math.Sqrt(v.Z)
                );
        }
        public static Vector3 max(Vector3 v1, Vector3 v2)
        {
            return new Vector3(
                MathHelper.Max(v1.X, v2.X),
                MathHelper.Max(v1.Y, v2.Y),
                MathHelper.Max(v1.Z, v2.Z)
                );
        }
        public static Vector3 maxAbs(Vector3 v1, Vector3 v2)
        {
            Vector3 v1a, v2a;
            Vector3Ext.absolute(v1, out v1a);
            Vector3Ext.absolute(v2, out v2a);
            return new Vector3(
                v1a.X > v2a.X ? v1.X : v2.X,
                v1a.Y > v2a.Y ? v1.Y : v2.Y,
                v1a.Z > v2a.Z ? v1.Z : v2.Z
                );
        }
        public static Vector3 project(Vector3 v, Vector3 p)
        {
            return Vector3.Dot(v, p) * p;
        }

        public static void calculateTanBin(Vector3[] v, Vector2[] uv, out Vector3 tangent, out Vector3 binormal)
        {
            Vector3 D, E;
            Vector2 F, G;
            D = v[1] - v[0];
            E = v[2] - v[0];
            F = uv[1] - uv[0];
            G = uv[2] - uv[0];

            float det = 1f / (F.X * G.Y - F.Y * G.X);
            float[] mProd = 
            {
                G.X * D.X - F.X * E.X, G.X * D.Y - F.X * E.Y, G.X * D.Z - F.X * E.Z,
                F.Y * D.X - G.Y * E.X, F.Y * D.Y - G.Y * E.Y, F.Y * D.Z - G.Y * E.Z
            };

            Vector3 normal = Vector3.Normalize(Vector3.Cross(D, E));
            binormal = -new Vector3(mProd[0], mProd[1], mProd[2]) * det;
            //tangent = new Vector3(mProd[3], mProd[4], mProd[5]) * det;
            tangent = Vector3.Cross(normal, binormal);
        }
    }
}
