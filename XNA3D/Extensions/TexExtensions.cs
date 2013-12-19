using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Graphics
{
    public static class TexExtensions
    {
        public static Texture3D loadTexture3D(GraphicsDevice g, int w, int h, params string[] files)
        {
            Texture3D t = new Texture3D(g, w, h, files.Length, false, SurfaceFormat.Color);
            Color[] c = new Color[files.Length * w * h];
            Texture2D t2;
            for (int i = 0; i < files.Length; i++)
            {
                t2 = Texture2D.FromStream(g, System.IO.File.Open(files[i], System.IO.FileMode.Open));
                t2.GetData<Color>(c, i * w * h, w * h);
                t.SetData<Color>(c);
                t2.Dispose();
            }
            return t;
        }

        public static Color getAlphaMultiplied(this Color c)
        {
            float r = c.A / 255f;
            c.R = (byte)(c.R * r);
            c.G = (byte)(c.G * r);
            c.B = (byte)(c.B * r);
            return c;
        }
        public static Vector4 getAlphaMultiplied(this Vector4 v)
        {
            v.X *= v.W;
            v.Y *= v.W;
            v.Z *= v.W;
            return v;
        }
    }
}
