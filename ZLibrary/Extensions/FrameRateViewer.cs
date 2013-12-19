using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace System.Diagnostics
{
    public class FrameRateViewer
    {
        public Effect fxFRV;
        public const double MS_30FPS = 1 / 30.0 * 1000.0;

        protected Matrix wvp;
        public Matrix WVP
        {
            get { return wvp; }
            set
            {
                wvp = value;
                fxFRV.Parameters["WVP"].SetValue(wvp);
            }
        }

        protected GraphicsDevice gDevice;
        protected SpeedLogger log;

        protected FrameRateLine full;
        public FrameRateLine FullLine { get { return full; } }
        protected FrameRateLine[] lines;
        public FrameRateLine this[int index]
        {
            get { return lines[index]; }
        }

        public IEnumerable<FrameRateLine> Lines
        {
            get
            {
                foreach (FrameRateLine l in lines)
                { yield return l; }
                yield return full;
            }
        }
        public IEnumerable<FrameRateLine> VisibleLines
        {
            get
            {
                foreach (var v in Lines)
                {
                    if (v.IsVisible) { yield return v; }
                }
            }
        }

        // The Number Of Frames To Keep Track Of
        protected int vF;
        public int ViewableFrames
        {
            get { return vF; }
            set
            {
                vF = value;
                FrameIndex %= vF;
                foreach (var l in Lines) { l.setFrameLength(vF, gDevice); }
                fxFRV.Parameters["MaxIndex"].SetValue(FrameIndex);
            }
        }

        // The Current Frame In The List
        protected int FrameIndex;

        // The Drawing Size
        protected Vector4 rectangle;
        public Vector2 LowerLeft
        {
            set
            {
                rectangle.X = value.X;
                rectangle.Y = value.Y;
                fxFRV.Parameters["ViewRect"].SetValue(rectangle);
            }
        }
        public Vector2 Size
        {
            set
            {
                rectangle.Z = value.X;
                rectangle.W = value.Y;
                fxFRV.Parameters["ViewRect"].SetValue(rectangle);
            }
        }

        public FrameRateViewer(SpeedLogger l, GraphicsDevice g, Effect fx, int frames = 30)
        {
            fxFRV = fx;
            fxFRV.CurrentTechnique = fx.Techniques[0];

            vF = frames;
            gDevice = g;

            lines = new FrameRateLine[l.Count];
            int i = 0;
            foreach (var c in l)
            {
                lines[i] = new FrameRateLine(g, this, Color.Red, MS_30FPS); ;
                i++;
            }
            full = new FrameRateLine(g, this, Color.Green, MS_30FPS);
            FrameIndex = 0;

            l.OnNewCheckpoint += onListAdd;
            l.OnCheckpointRemoval += onListRemoval;
            l.OnDump += onDump;

            rectangle = new Vector4(0, 0, 300, 80);

            WVP = Matrix.CreateTranslation(-gDevice.Viewport.Width / 2f, -gDevice.Viewport.Height / 2f, -0.5f) *
                Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up) *
                Matrix.CreateOrthographic(gDevice.Viewport.Width, gDevice.Viewport.Height, 0, 1);
        }
        public void onScreenResize(Vector2 s)
        {
            WVP = Matrix.CreateTranslation(-s.X / 2f, -s.Y / 2f, -0.5f) *
                Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up) *
               Matrix.CreateOrthographic(s.X, s.Y, 0, 1);
        }

        // Hooks To The Logger
        private void onListRemoval(SpeedLogger l, SpeedLogger.Checkpoint c)
        {
            FrameRateLine[] old = lines;
            lines = new FrameRateLine[old.Length - 1];
            for (int i = 0; i < c.Index; i++) { lines[i] = old[i]; }
            for (long i = c.Index + 1; i < old.Length; i++) { lines[i - 1] = old[i]; }
        }
        private void onListAdd(SpeedLogger l, SpeedLogger.Checkpoint c)
        {
            Array.Resize<FrameRateLine>(ref lines, lines.Length + 1);
            lines[lines.Length - 1] = new FrameRateLine(gDevice, this, Color.Red);
        }
        private void onDump(SpeedLogger.DumpData data)
        {
            if (data.IsNewFrame) { full.addTime(data.ElapsedMilliseconds); }
            else { lines[data.Index].addTime(data.ElapsedMilliseconds); }
        }

        // Updating And Drawing
        public void update()
        {
            foreach (var l in Lines) { l.setTimeTo(FrameIndex); l.update(); }
            FrameIndex++;
            FrameIndex %= ViewableFrames;
            fxFRV.Parameters["StartIndex"].SetValue(FrameIndex);
        }
        public void draw(GraphicsDevice g)
        {
            fxFRV.CurrentTechnique.Passes[0].Apply();
            foreach (var l in VisibleLines) { l.draw(g); }
        }
    }

    public struct FrameRateVertex : IVertexType
    {
        public Vector2 IndexPercent;
        public float Index
        {
            get { return IndexPercent.X; }
            set { IndexPercent.X = value; }
        }
        public float Percent
        {
            get { return IndexPercent.Y; }
            set { IndexPercent.Y = value; }
        }
        public Color Color;

        public FrameRateVertex(float index, float percent, Color c)
        {
            IndexPercent = new Vector2(index, percent);
            Color = c;
        }

        public static readonly VertexDeclaration Declaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 2, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            );
        public VertexDeclaration VertexDeclaration
        {
            get { return Declaration; }
        }
    }

    public class FrameRateLine
    {
        protected double averageMod, time, max;
        public double Time
        {
            get { return time; }
        }
        public double MaxTime
        {
            get { return max; }
            set
            {
                float f = (float)(value / max);
                max = value;
                for (int i = 0; i < verts.Length; i++)
                {
                    verts[i].Percent *= f;
                }
            }
        }

        protected Color color;
        public Color Color
        {
            get { return color; }
            set
            {
                color = value;
                for (int i = 0; i < verts.Length; i++)
                {
                    verts[i].Color = color;
                }
            }
        }

        protected DynamicVertexBuffer vb;
        protected FrameRateVertex[] verts;

        public bool IsVisible;

        public FrameRateLine(GraphicsDevice g, FrameRateViewer v, Color c, double maxTime = FrameRateViewer.MS_30FPS)
        {
            color = c;
            max = maxTime;
            setFrameLength(v.ViewableFrames, g);
            newFrame();
            IsVisible = false;
        }
        public void setFrameLength(int l, GraphicsDevice g)
        {
            verts = new FrameRateVertex[l + 1];
            for (int i = 0; i < verts.Length; i++)
            { verts[i] = new FrameRateVertex(i, 0, color); }
            buildBuffer(g);
        }

        public void newFrame()
        {
            averageMod = 0;
            time = 0;
        }

        public void addTime(double t)
        {
            averageMod++;
            time = (time * (averageMod - 1) + t) / averageMod;
        }
        public void setTimeTo(int fIndex)
        {
            verts[fIndex].Percent = (float)(time / MaxTime);
            verts[fIndex + 1].Percent = verts[fIndex + 1].Percent;
        }

        public void buildBuffer(GraphicsDevice g)
        {
            vb = new DynamicVertexBuffer(g, FrameRateVertex.Declaration, verts.Length, BufferUsage.WriteOnly);
        }
        public void update()
        {
            vb.SetData<FrameRateVertex>(verts);
        }

        public void draw(GraphicsDevice g)
        {
            g.SetVertexBuffer(vb);
            g.DrawPrimitives(PrimitiveType.LineStrip, 0, verts.Length - 1);
        }
    }
}
