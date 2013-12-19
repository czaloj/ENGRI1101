using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ZLibrary.Operators;

namespace ZLibrary
{
    public struct HSVColor
    {
        byte H;
        byte S;
        byte V;
        byte A;

        public HSVColor(byte h, byte s, byte v)
        {
            H = h;
            S = s;
            V = v;
            A = 255;
        }

        public HSVColor(byte h, byte s, byte v, byte a)
        {
            H = h;
            S = s;
            V = v;
            A = a;
        }

        public static HSVColor fromColor(Color c)
        {
            float r = c.R / 255f;
            float g = c.G / 255f;
            float b = c.B / 255f;

            float minRGB = System.Math.Min(r, System.Math.Min(g, b));
            float maxRGB = System.Math.Max(r, System.Math.Max(g, b));

            byte v = (byte)ZMath.fastFloor(255f * maxRGB);

            if (minRGB == maxRGB)
            {
                return new HSVColor(0, 0, v, c.A);
            }

            float _d = (r == minRGB) ? g - b : ((b == minRGB) ? r - g : b - r);
            float _h = (r == minRGB) ? 3 : ((b == minRGB) ? 1 : 5);

            byte h = (byte)ZMath.fastFloor(255f / 6f * (_h - _d / (maxRGB - minRGB)));
            byte s = (byte)ZMath.fastFloor(255f * (maxRGB - minRGB) / maxRGB);

            return new HSVColor(h, s, v, c.A);
        }
    }
}
