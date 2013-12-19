using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA3D.Physics
{
    public struct CollisionInformation
    {
        public bool Collided;
        public Vector3 Direction;
        public Vector3 Depth;
    }

    public struct EntryExitTime
    {
        public static readonly EntryExitTime Impossible = new EntryExitTime(float.PositiveInfinity, 0f);
        public static readonly EntryExitTime Forever = new EntryExitTime(0f, float.PositiveInfinity);

        public float EntryTime;
        public float ExitTime;

        public bool IsViable
        {
            get { return EntryTime <= ExitTime; }
        }
        public float Duration
        {
            get { return ExitTime - EntryTime; }
        }

        public EntryExitTime(float entry, float exit)
        {
            EntryTime = entry;
            ExitTime = exit;
        }

        public void modifyEntry(float t)
        {
            if (t > EntryTime) { EntryTime = t; }
        }
        public void modifyExit(float t)
        {
            if (t < ExitTime) { ExitTime = t; }
        }
        public void modifyTimes(EntryExitTime other)
        {
            modifyEntry(other.EntryTime);
            modifyExit(other.ExitTime);
        }
    }

    public class AABB
    {
        private static BasicEffect fxBasic;
        private static VertexBuffer vb;
        private static IndexBuffer ib;
        public static void build(GraphicsDevice g)
        {
            vb = new VertexBuffer(g, VertexPositionColor.VertexDeclaration, 8, BufferUsage.WriteOnly);
            vb.SetData<VertexPositionColor>(new VertexPositionColor[8]
            {
                new VertexPositionColor(new Vector3(-1, 1, 1), Color.White),
                new VertexPositionColor(new Vector3(1, 1, 1), Color.White),
                new VertexPositionColor(new Vector3(-1, 1, -1), Color.White),
                new VertexPositionColor(new Vector3(1, 1, -1), Color.White),
                new VertexPositionColor(new Vector3(-1, -1, 1), Color.White),
                new VertexPositionColor(new Vector3(1, -1, 1), Color.White),
                new VertexPositionColor(new Vector3(-1, -1, -1), Color.White),
                new VertexPositionColor(new Vector3(1, -1, -1), Color.White)
            });
            ib = new IndexBuffer(g, IndexElementSize.SixteenBits, 36, BufferUsage.WriteOnly);
            ib.SetData<short>(new short[36]
            {
                0, 1, 4, 4, 1, 5,
                3, 2, 7, 7, 2, 6,
                2, 0, 6, 6, 0, 4,
                1, 3, 5, 5, 3, 7,
                2, 3, 0, 0, 3, 1,
                7, 6, 5, 5, 6, 4
            });
            fxBasic = new BasicEffect(g);
            fxBasic.CurrentTechnique = fxBasic.Techniques[0];
            fxBasic.Alpha = 1;
            fxBasic.LightingEnabled = false;
            fxBasic.TextureEnabled = false;
            fxBasic.VertexColorEnabled = true;
            fxBasic.PreferPerPixelLighting = false;
            fxBasic.FogEnabled = false;
        }

        public Vector3 Center;
        public Vector3 HalfSize;
        public Vector3 Size { get { return HalfSize * 2f; } set { HalfSize = value / 2f; } }

        public float Left { get { return Center.X - HalfSize.X; } }
        public float Right { get { return Center.X + HalfSize.X; } }
        public float Bottom { get { return Center.Y - HalfSize.Y; } }
        public float Top { get { return Center.Y + HalfSize.Y; } }
        public float Front { get { return Center.Z - HalfSize.Z; } }
        public float Back { get { return Center.Z + HalfSize.Z; } }
        public float NX { get { return Center.X - HalfSize.X; } }
        public float PX { get { return Center.X + HalfSize.X; } }
        public float NY { get { return Center.Y - HalfSize.Y; } }
        public float PY { get { return Center.Y + HalfSize.Y; } }
        public float NZ { get { return Center.Z - HalfSize.Z; } }
        public float PZ { get { return Center.Z + HalfSize.Z; } }
        public Vector3 NXNYNZ
        {
            get
            {
                return new Vector3(
                    Center.X - HalfSize.X,
                    Center.Y - HalfSize.Y,
                    Center.Z - HalfSize.Z
                    );
            }
        }
        public Vector3 NXNYPZ
        {
            get
            {
                return new Vector3(
                    Center.X - HalfSize.X,
                    Center.Y - HalfSize.Y,
                    Center.Z + HalfSize.Z
                    );
            }
        }
        public Vector3 NXPYNZ
        {
            get
            {
                return new Vector3(
                    Center.X - HalfSize.X,
                    Center.Y + HalfSize.Y,
                    Center.Z - HalfSize.Z
                    );
            }
        }
        public Vector3 NXPYPZ
        {
            get
            {
                return new Vector3(
                    Center.X - HalfSize.X,
                    Center.Y + HalfSize.Y,
                    Center.Z + HalfSize.Z
                    );
            }
        }
        public Vector3 PXNYNZ
        {
            get
            {
                return new Vector3(
                    Center.X + HalfSize.X,
                    Center.Y - HalfSize.Y,
                    Center.Z - HalfSize.Z
                    );
            }
        }
        public Vector3 PXNYPZ
        {
            get
            {
                return new Vector3(
                    Center.X + HalfSize.X,
                    Center.Y - HalfSize.Y,
                    Center.Z + HalfSize.Z
                    );
            }
        }
        public Vector3 PXPYNZ
        {
            get
            {
                return new Vector3(
                    Center.X + HalfSize.X,
                    Center.Y + HalfSize.Y,
                    Center.Z - HalfSize.Z
                    );
            }
        }
        public Vector3 PXPYPZ
        {
            get
            {
                return new Vector3(
                    Center.X + HalfSize.X,
                    Center.Y + HalfSize.Y,
                    Center.Z + HalfSize.Z
                    );
            }
        }

        public AABB(Vector3 c, Vector3 halfSize)
        {
            Center = c;
            HalfSize = halfSize;
        }
        public AABB(AABB b, Vector3 c)
        {
            Center = c;
            HalfSize = b.HalfSize;
        }

        public Vector3 min()
        {
            return Center - HalfSize;
        }
        public Vector3 max()
        {
            return Center + HalfSize;
        }
        public BoundingBox getBBox()
        {
            return new BoundingBox(min(), max());
        }

        public static void pushX(ref AABB a, ref AABB b)
        {
            if (a.Center.X > b.Center.X)
            { if (b.PX > a.NX) { a.Center.X += b.PX - a.NX; } }
            else
            { if (a.PX > b.NX) { a.Center.X -= a.PX - b.NX; } }
        }
        public static void pushY(ref AABB a, ref AABB b)
        {
            if (a.Center.Y > b.Center.Y)
            { if (b.PY > a.NY) { a.Center.Y += b.PY - a.NY; } }
            else
            { if (a.PY > b.NY) { a.Center.Y -= a.PY - b.NY; } }
        }
        public static void pushZ(ref AABB a, ref AABB b)
        {
            if (a.Center.Z > b.Center.Z)
            { if (b.PZ > a.NZ) { a.Center.Z += b.PZ - a.NZ; } }
            else
            { if (a.PZ > b.NZ) { a.Center.Z -= a.PZ - b.NZ; } }
        }

        public static bool intersects(ref AABB a, ref AABB b)
        {
            Vector3 dis = a.Center - b.Center;
            Vector3Ext.absolute(ref dis);
            Vector3 depth = (a.HalfSize + b.HalfSize) - dis;
            return (depth.X > 0 && depth.Y > 0 && depth.Z > 0);
        }
        public static bool intersects(ref AABB a, ref AABB b, out Vector3 depth)
        {
            Vector3 dis = a.Center - b.Center;
            Vector3Ext.absolute(ref dis);
            depth = (a.HalfSize + b.HalfSize) - dis;
            return (depth.X > 0 && depth.Y > 0 && depth.Z > 0);
        }

        public static void draw(GraphicsDevice g, XNA3D.Cameras.ACCamera camera, params DrawAABB[] boxes)
        {
            fxBasic.View = camera.View;
            fxBasic.Projection = camera.Projection;
            g.SetVertexBuffer(vb);
            g.Indices = ib;
            g.BlendState = BlendState.AlphaBlend;
            foreach (DrawAABB box in boxes)
            {
                fxBasic.Alpha = box.color.A / 255f;
                fxBasic.DiffuseColor = box.color.ToVector3();
                fxBasic.World = Matrix.CreateScale(box.box.HalfSize + Vector3.One * 0.08f) * Matrix.CreateTranslation(box.box.Center);
                fxBasic.CurrentTechnique.Passes[0].Apply();
                g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 12);
            }
        }

        #region Point Getting
        public Vector3[] getAllPoints()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsXP()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsYP()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsZP()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
            };
        }
        public Vector3[] getAllPointsXN()
        {
            return new Vector3[]
            {
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsYN()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsZN()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsXPYP()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
            };
        }
        public Vector3[] getAllPointsXPYN()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsXPZP()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
            };
        }
        public Vector3[] getAllPointsXPZN()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
            };
        }
        public Vector3[] getAllPointsXNYP()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsXNYN()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsXNZP()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsXNZN()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsYPZP()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
            };
        }
        public Vector3[] getAllPointsYPZN()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
            };
        }
        public Vector3[] getAllPointsYNZP()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsYNZN()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsXPYPZP()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsXPYPZN()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsXPYNZP()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsXPYNZN()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsXNYPZP()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsXNYPZN()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsXNYNZP()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        }
        public Vector3[] getAllPointsXNYNZN()
        {
            return new Vector3[]
            {
                new Vector3(Center.X + HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X + HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y + HalfSize.Y,Center.Z - HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z + HalfSize.Z),
                new Vector3(Center.X - HalfSize.X,Center.Y - HalfSize.Y,Center.Z - HalfSize.Z)
            };
        } 
        #endregion
    }

    public struct DrawAABB
    {
        public AABB box;
        public Color color;

        public DrawAABB(AABB b, Color c)
        {
            box = b;
            color = c;
        }
    }
}
