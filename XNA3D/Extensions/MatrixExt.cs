using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public static class MatrixExt
    {
        public static Matrix CreateSkewXZAlongX(float angle)
        {
            return CreateSkewXZAlongX_Slope((float)Math.Tan(angle));
        }
        public static Matrix CreateSkewXZAlongZ(float angle)
        {
            return CreateSkewXZAlongZ_Slope((float)Math.Tan(angle));
        }
        public static Matrix CreateSkewXYAlongX(float angle)
        {
            return CreateSkewXYAlongX_Slope((float)Math.Tan(angle));
        }
        public static Matrix CreateSkewXYAlongY(float angle)
        {
            return CreateSkewXYAlongY_Slope((float)Math.Tan(angle));
        }
        public static Matrix CreateSkewYZAlongY(float angle)
        {
            return CreateSkewYZAlongY_Slope((float)Math.Tan(angle));
        }
        public static Matrix CreateSkewYZAlongZ(float angle)
        {
            return CreateSkewYZAlongZ_Slope((float)Math.Tan(angle));
        }

        public static Matrix CreateSkewXZAlongX_Slope(float slope)
        {
            return new Matrix(
                1, 0, 0, 0,
                slope, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
                );
        }
        public static Matrix CreateSkewXZAlongZ_Slope(float slope)
        {
            return new Matrix(
                1, 0, 0, 0,
                0, 1, slope, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
                );
        }
        public static Matrix CreateSkewXYAlongX_Slope(float slope)
        {
            return new Matrix(
                1, 0, 0, 0,
                0, 1, 0, 0,
                slope, 0, 1, 0,
                0, 0, 0, 1
                );
        }
        public static Matrix CreateSkewXYAlongY_Slope(float slope)
        {
            return new Matrix(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, slope, 1, 0,
                0, 0, 0, 1
                );
        }
        public static Matrix CreateSkewYZAlongY_Slope(float slope)
        {
            return new Matrix(
                1, slope, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
                );
        }
        public static Matrix CreateSkewYZAlongZ_Slope(float slope)
        {
            return new Matrix(
                1, 0, slope, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
                );
        }

        public static Matrix Create2DViewportView(Viewport port)
        {
            return Create2DViewportView(port.Width, port.Height);
        }
        public static Matrix Create2DViewportView(Vector2 portSize)
        {
            return Create2DViewportView(portSize.X, portSize.Y);
        }
        public static Matrix Create2DViewportView(float portWidth, float portHeight)
        {
            Vector3 lookStart = new Vector3(portWidth / 2f, portHeight / 2f, 0f);
            return Matrix.CreateLookAt(lookStart, lookStart + Vector3.UnitZ, -Vector3.UnitY);
        }
        public static Matrix Create2DViewportProjection(Viewport port)
        {
            return Create2DViewportProjection(port.Width, port.Height);
        }
        public static Matrix Create2DViewportProjection(Vector2 portSize)
        {
            return Create2DViewportProjection(portSize.X, portSize.Y);
        }
        public static Matrix Create2DViewportProjection(float portWidth, float portHeight)
        {
            return Matrix.CreateOrthographic(portWidth, portHeight, 0f, 1f);
        }
        public static void Create2DViewportViewProjection(Viewport port, out Matrix view, out Matrix projection)
        {
            view = Create2DViewportView(port);
            projection = Create2DViewportProjection(port);
        }
        public static void Create2DViewportViewProjection(Vector2 portSize, out Matrix view, out Matrix projection)
        {
            view = Create2DViewportView(portSize);
            projection = Create2DViewportProjection(portSize);
        }
        public static void Create2DViewportViewProjection(float portWidth, float portHeight, out Matrix view, out Matrix projection)
        {
            view = Create2DViewportView(portWidth, portHeight);
            projection = Create2DViewportProjection(portWidth, portHeight);
        }
    }
}
