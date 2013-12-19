using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZLibrary.Math
{
    public interface IFunction
    {
        void setPoint(int p, float value);
        float getValue(float x);
    }
}
