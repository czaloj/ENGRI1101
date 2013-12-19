using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework
{
    public struct M3Info
    {
        public Matrix Scale;
        public Matrix Rotation;
        public Matrix Translation;
        public Matrix World
        {
            get
            {
                return Scale * Rotation * Translation;
            }
        }

        public M3Info(Matrix s, Matrix r, Matrix t)
        {
            Scale = s;
            Rotation = r;
            Translation = t;
        }

        public static M3Info lerp(M3Info m1, M3Info m2, float r)
        {
            return new M3Info(
                Matrix.Lerp(m1.Scale, m2.Scale, r),
                Matrix.Lerp(m1.Rotation, m2.Rotation, r),
                Matrix.Lerp(m1.Translation, m2.Translation, r)
                );
        }
    }
}
