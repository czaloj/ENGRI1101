using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ZLibrary.Operators;

namespace ZLibrary.Math
{
    /// <summary>
    /// A Function Taking X Input Between 0 - 1
    /// And Returning Hopefully Between 0 - 1
    /// Dependent On Value Points Settings
    /// </summary>
    public class UnitXFunction : IFunction
    {
        //Equidistant Points
        float[] points;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UnitXFunction()
        {
            points = new float[11];
            points[0] = 0f;
            points[10] = 1f;
        }

        /// <summary>
        /// Set The Valued Points In The Function
        /// </summary>
        /// <param name="n">The Number Of Points Excluding Endpoints 0 and 1</param>
        public virtual void setNumberValuePoints(int n)
        {
            points = new float[n + 2];
            points[0] = 0f;
            points[n + 1] = 1f;
        }

        /// <summary>
        /// Set A Point To Value
        /// </summary>
        /// <param name="p">A Point Index</param>
        /// <param name="value">Value For It To Be Set</param>
        public virtual void setPoint(int p, float value)
        {
            points[p] = value;
        }

        /// <summary>
        /// Returns The Function Y Value For The X Value
        /// </summary>
        /// <param name="x">Between 0 and 1</param>
        /// <returns>The Lerped Function Y Value</returns>
        public virtual float getValue(float x)
        {
            if (x == 1f) { return points[points.Length - 1]; }
            float rx = x * (points.Length - 1);
            int n = ZMath.fastFloor(rx);
            return MathHelper.Lerp(points[n], points[n + 1], rx - n);
        }
    }
}
