using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework
{
    public static class MeshCreator
    {
        public struct Vertex
        {
            public Vector3 Position, Normal;

            public Vertex(Vector3 p, Vector3 n)
            {
                Position = p;
                Normal = n;
            }
            public Vertex(Vertex v, Matrix m)
            {
                Position = Vector3.Transform(v.Position, m);
                Normal = Vector3.Transform(v.Normal, m);
            }
        }

        public struct SphereInfo
        {
            public float[] Phi;
            public float[] Theta;

            public SphereInfo(float[] phiAngles, float[] thetaAngles)
            {
                Phi = new float[phiAngles.Length];
                Array.Copy(phiAngles, Phi, Phi.Length);
                Array.Sort(Phi);

                Theta = new float[thetaAngles.Length];
                Array.Copy(thetaAngles, Theta, Theta.Length);
                Array.Sort(Theta);
            }
            public SphereInfo(Vector3 phiSDA, Vector3 thetaSDA)
            {
                Phi = new float[(int)phiSDA.Z];
                for (int i = 0; i < Phi.Length; i++) { Phi[i] = phiSDA.X + i * phiSDA.Y; }

                Theta = new float[(int)thetaSDA.Z];
                for (int i = 0; i < Phi.Length; i++) { Phi[i] = phiSDA.X + i * phiSDA.Y; }
            }
        }
        public static void buildSphere(SphereInfo info, out Vertex[] verts, out int[] ind)
        {
            Matrix rot;
            Vertex v = new Vertex(Vector3.Right, Vector3.Right);
            int vertexCount = 0;
            int polyCount = 0;
            int eCount = 0, rCount = 0;
            foreach (float a in info.Phi)
            {
                if (a == MathHelper.PiOver2 || a == -MathHelper.PiOver2) { vertexCount++; eCount++; }
                else { vertexCount += info.Theta.Length; rCount++; }
            }
            polyCount = (eCount + 2 * (rCount - 1)) * (info.Theta.Length - 1);
            ind = new int[polyCount * 3];
            verts = new Vertex[vertexCount];

            int ii = 0;
            int priStart = -1;
            int riStart = 0, ri = 0;

            for (int pi = 0; pi < info.Phi.Length; pi++)
            {
                ri = 0;
                if (info.Phi[pi] == MathHelper.PiOver2 || info.Phi[pi] == -MathHelper.PiOver2)
                {
                    rot = Matrix.CreateRotationZ(info.Phi[pi]);
                    verts[riStart + ri] = new Vertex(v, rot);
                    if (priStart != -1 && info.Phi[pi] == MathHelper.PiOver2)
                    {
                        //Top Vertex
                        for (int ir = 0; ir < info.Theta.Length - 1; ir++)
                        {
                            ind[ii++] = riStart;
                            ind[ii++] = priStart + ir + 1;
                            ind[ii++] = priStart + ir;
                        }
                    }
                    priStart = riStart; riStart++;
                }
                else
                {
                    for (int ti = 0; ti < info.Theta.Length; ti++)
                    {
                        rot = Matrix.CreateRotationZ(info.Phi[pi]) * Matrix.CreateRotationY(info.Theta[ti]);
                        verts[riStart + ri] = new Vertex(v, rot);
                        ri++;
                    }
                    if (riStart - priStart != 1)
                    {
                        //Quad
                        for (int ir = 0; ir < info.Theta.Length - 1; ir++)
                        {
                            ind[ii++] = riStart + ir + 1;
                            ind[ii++] = priStart + ir + 1;
                            ind[ii++] = riStart + ir;

                            ind[ii++] = riStart + ir;
                            ind[ii++] = priStart + ir + 1;
                            ind[ii++] = priStart + ir;
                        }
                    }
                    else if (verts[priStart].Position.Y < 0)
                    {
                        //Bottom Vertex
                        for (int ir = 0; ir < info.Theta.Length - 1; ir++)
                        {
                            ind[ii++] = priStart;
                            ind[ii++] = riStart + ir;
                            ind[ii++] = riStart + ir + 1;
                        }
                    }
                    priStart = riStart; riStart += info.Theta.Length;
                }
            }

        }
    }
}
