using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ZLibrary.Operators.Noise
{
    public class ZimplexNoise : SimplexNoise
    {
        //Spread Of Getting Values
        Vector3 spread = Vector3.One;

        //The Range Of Values To Return
        float min = 0f;
        float max = 1f;
        float range = 1f;

        public ZimplexNoise(int seed = 0)
            : base(seed)
        {
        }

        public virtual void setSpread(float xSpread, float ySpread, float zSpread)
        {
            spread.X = xSpread;
            spread.Y = ySpread;
            spread.Z = zSpread;
        }

        public virtual void setRange(float min, float max)
        {
            this.min = min;
            this.max = max;
            range = (max - min) / 2f;
        }

        public override float noise(float xin, float yin)
        {
            return (base.noise(xin / spread.X, yin / spread.Y) + 1) * range + min;
        }

        public override float noise(float xin, float yin, float zin)
        {
            return (base.noise(xin / spread.X, yin / spread.Y, zin / spread.Z) + 1) * range + min;
        }
    }
}
