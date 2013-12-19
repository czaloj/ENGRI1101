using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ZLibrary.Graphics;

namespace ORLabs.Graphics.Graphs
{
    public struct EdgeInfo
    {
        public Vector3 Start, End;
        public float RadiusS, RadiusE;
        public Color ColorS, ColorE, ColorT;

        public EdgeInfo(Vector3 start, Color c1, float r1, Vector3 end, Color c2, float r2, Color ct)
        {
            Start = start;
            End = end;
            RadiusS = r1;
            RadiusE = r2;
            ColorS = c1;
            ColorE = c2;
            ColorT = ct;

            points = TrapezoidBuilder.calc(start, r1, end, r2);
        }

        private TrapezoidBuilder.TrapezoidPoints points;
        public Vector3 TopLeft
        {
            get { return points.P1; }
        }
        public Vector3 TopRight
        {
            get { return points.P2; }
        }
        public Vector3 BottomLeft
        {
            get { return points.P4; }
        }
        public Vector3 BottomRight
        {
            get { return points.P3; }
        }

        public Vector3 StartCR
        {
            get { return new Vector3(Start.X, Start.Y, RadiusS); }
        }
        public Vector3 EndCR
        {
            get { return new Vector3(End.X, End.Y, RadiusE); }
        }

        public static readonly Vector2 CornerTL = new Vector2(0, 1);
        public static readonly Vector2 CornerTR = new Vector2(1, 1);
        public static readonly Vector2 CornerBL = new Vector2(0, -1);
        public static readonly Vector2 CornerBR = new Vector2(1, -1);
    }

    public class EdgeList : BatchList<EdgeInfo, EdgeList.Vertex>
    {
        private static IndexBuffer iBuffer;
        private const int IndicesPerRect = 6;
#if HIDEF
        public static readonly IndexElementSize IndexSize = IndexElementSize.ThirtyTwoBits;
        public const int MaxLines = 500000;
#else
        public static readonly IndexElementSize IndexSize = IndexElementSize.SixteenBits;
        public const int MaxLines = 30000;
#endif

        public bool ShouldBuild;

        public EdgeList(GraphicsDevice g, int rectCount)
            : base(g, rectCount < MaxLines ? rectCount : MaxLines, 4)
        {
        }
        public override void resizeList(int maxCount)
        {
            base.resizeList(maxCount < MaxLines ? maxCount : MaxLines);
        }

        protected VertexBuffer vBuffer;
        public override void buildBuffers(GraphicsDevice g)
        {
            if (maxCount == 0) { vBuffer = null; return; }
            if (iBuffer == null || iBuffer.IndexCount < maxCount * IndicesPerRect)
            {
                iBuffer = new IndexBuffer(g, IndexSize, maxCount * IndicesPerRect, BufferUsage.WriteOnly);
#if HIDEF
                int[] ind = new int[maxCount * IndicesPerRect];
                for (int i = 0, ii = 0, vi = 0; i < maxCount; i++, ii += 6, vi += 4)
                {
                    ind[ii] = vi;
                    ind[ii + 1] = vi + 1;
                    ind[ii + 2] = vi + 2;
                    ind[ii + 3] = vi + 2;
                    ind[ii + 4] = vi + 1;
                    ind[ii + 5] = vi + 3;
                }
                iBuffer.SetData<int>(ind);
#elif REACH
                short[] ind = new short[maxCount * IndicesPerRect];
                int i = 0, ii = 0;
                for (short vi = 0; i < maxCount; i++, ii += 6, vi += 4)
                {
                    ind[ii] = vi;
                    ind[ii + 1] = (short)(vi + 1);
                    ind[ii + 2] = (short)(vi + 2);
                    ind[ii + 3] = (short)(vi + 2);
                    ind[ii + 4] = (short)(vi + 1);
                    ind[ii + 5] = (short)(vi + 3);
                }
                iBuffer.SetData<short>(ind);
#endif
            }
            vBuffer = new VertexBuffer(g, Vertex.Declaration, maxCount * vPerInstance, BufferUsage.WriteOnly);
        }

        public override void end()
        {
            if (vBuffer != null)
            {
                vBuffer.SetData<Vertex>(vertices);
                ShouldBuild = false;
            }
        }

        public override void set(GraphicsDevice g)
        {
            g.SetVertexBuffer(vBuffer);
            g.Indices = iBuffer;
        }
        public override void draw(GraphicsDevice g)
        {
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, count * 4, 0, count * 2);
        }

        public override void add(EdgeInfo data)
        {
            set(count, data);
            count++;
        }
        public override void set(int i, EdgeInfo data)
        {
            i = i * vPerInstance;
            vertices[i + 0] = new Vertex(data.TopLeft, 0f, data.StartCR, EdgeInfo.CornerTL, data.ColorS, data.ColorT);
            vertices[i + 1] = new Vertex(data.TopRight, 1f, data.EndCR, EdgeInfo.CornerTR, data.ColorE, data.ColorT);
            vertices[i + 2] = new Vertex(data.BottomLeft, 0f, data.StartCR, EdgeInfo.CornerBL, data.ColorS, data.ColorT);
            vertices[i + 3] = new Vertex(data.BottomRight, 1f, data.EndCR, EdgeInfo.CornerBR, data.ColorE, data.ColorT);
            ShouldBuild = true;
        }

        public void setColor(int i, Color c1, Color c2, Color ct)
        {
            i = i * vPerInstance;
            vertices[i + 0].Color1 = c1; vertices[i + 0].Color2 = ct;
            vertices[i + 1].Color1 = c2; vertices[i + 1].Color2 = ct;
            vertices[i + 2].Color1 = c1; vertices[i + 2].Color2 = ct;
            vertices[i + 3].Color1 = c2; vertices[i + 3].Color2 = ct;
            ShouldBuild = true;
        }

        public struct Vertex : IVertexType
        {
            public Vector4 PosRatio;
            public Vector3 CenRad;
            public Vector2 Corner;
            public Color Color1;
            public Color Color2;

            public Vertex(Vector3 pos, float r, Vector3 cenRad, Vector2 corner, Color c1, Color c2)
            {
                PosRatio = new Vector4(pos, r);
                CenRad = cenRad;
                Corner = corner;
                Color1 = c1;
                Color2 = c2;
            }

            public static readonly VertexDeclaration Declaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
                new VertexElement(sizeof(float) * 7, VertexElementFormat.Vector2, VertexElementUsage.Position, 2),
                new VertexElement(sizeof(float) * 9, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(sizeof(float) * 10, VertexElementFormat.Color, VertexElementUsage.Color, 1)
                );
            public VertexDeclaration VertexDeclaration
            {
                get { return Declaration; }
            }
        }
    }

    public static class TrapezoidBuilder
    {
        private const float RadiansToDegrees = (float)(180 / Math.PI);

        public static TrapezoidPoints calc(Vector3 Center1, float Radius1, Vector3 Center2, float Radius2)
        {
            TrapezoidPoints TrapPoints = new TrapezoidPoints();

            // Get the angle of the line C0-C1 in degrees. This will be used in conjunction with angleA to determine the vector of these points
            float angleRelativeToPositiveXAxis = CalculateAngleRelativeToXAxis(Center1, Center2);

            // Get angleA
            float angleA = CalculateAngleA(Center1, Center2, Radius1, Radius2);

            ////  Calculate P1 and P2 coordinates first
            float positiveAngle = angleRelativeToPositiveXAxis + angleA;
            float cosPositiveAngle = (float)Math.Cos(positiveAngle / RadiansToDegrees);
            float valueToAddToC0X = cosPositiveAngle * Radius1;

            // Set P1's X coordinate
            TrapPoints.P1.X = Center1.X + valueToAddToC0X;
            float valueToAddToC1X = cosPositiveAngle * Radius2;

            // Set P2's X coordinate
            TrapPoints.P2.X = Center2.X + valueToAddToC1X;
            float sinPositiveAngle = (float)Math.Sin(positiveAngle / RadiansToDegrees);
            float valueToAddToC0Y = sinPositiveAngle * Radius1;

            // Set P1's Y coordinate
            TrapPoints.P1.Y = Center1.Y + valueToAddToC0Y;
            float valueToAddToC1Y = sinPositiveAngle * Radius2;

            // Set P2's Y coordinate
            TrapPoints.P2.Y = Center2.Y + valueToAddToC1Y;

            ////  Calculate P3 and P4 coordinates
            float negativeAngle = angleRelativeToPositiveXAxis - angleA;
            float cosNegativeAngle = (float)Math.Cos(negativeAngle / RadiansToDegrees);
            valueToAddToC0X = cosNegativeAngle * Radius1;

            // Set P4's X coordinate
            TrapPoints.P4.X = Center1.X + valueToAddToC0X;
            valueToAddToC1X = cosNegativeAngle * Radius2;

            // Set P3's X coordinate
            TrapPoints.P3.X = Center2.X + valueToAddToC1X;
            float sinNegativeAngle = (float)Math.Sin(negativeAngle / RadiansToDegrees);
            valueToAddToC0Y = sinNegativeAngle * Radius1;

            // Set P4's Y coordinate
            TrapPoints.P4.Y = Center1.Y + valueToAddToC0Y;
            valueToAddToC1Y = sinNegativeAngle * Radius2;

            // Set P3's Y coordinate
            TrapPoints.P3.Y = Center2.Y + valueToAddToC1Y;

            return TrapPoints;
        }

        private static float CalculateAngleA(Vector3 pointC0, Vector3 pointC1, float radius0, float radius1)
        {
            float xDistance = pointC1.X - pointC0.X;
            float yDistance = pointC1.Y - pointC0.Y;

            float distance = (float)Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance));

            float radius2 = radius0 - radius1;

            float cosA = radius2 / distance;

            float angleAInRadians = (float)Math.Acos(cosA);

            float angleAInDegrees = angleAInRadians * RadiansToDegrees;

            return angleAInDegrees;
        }
        private static float CalculateAngleRelativeToXAxis(Vector3 point0, Vector3 point1)
        {
            // So C1x is subtracted from C2x and C1y from C2y. Note that it’s important to subtract 
            // the 1st value from the 2nd to help determine which quadrant the angle is in.
            float x = point1.X - point0.X;
            float y = point1.Y - point0.Y;

            // Get the angle in radians
            float angleInRadians = (float)Math.Atan2(x, y);

            // Convert to degrees
            float angleInDegrees = angleInRadians * RadiansToDegrees;

            // Subtract from 90 to get the angle relative to the positive X-axis
            float relativeAngleInDegrees = 90 - angleInDegrees;

            // Return result
            return relativeAngleInDegrees;
        }

        public struct TrapezoidPoints
        {
            public Vector3 P1;
            public Vector3 P2;
            public Vector3 P3;
            public Vector3 P4;
        }
    }
}
