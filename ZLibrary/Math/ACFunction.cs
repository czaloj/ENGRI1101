using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZLibrary.Math
{
    public abstract class ACFunction : IFunction
    {
        public void setPoint(int p, float value)
        {
        }

        public abstract float getValue(float x);
    }
}
