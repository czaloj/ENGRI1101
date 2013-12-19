using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA3D.DeferredRendering;

namespace XNA3D.ZDR
{
    public static class Importer
    {
        public static void getSkinnedMesh(string file, GraphicsDevice g, out VertexBuffer vb, out IndexBuffer ib)
        {
            VertexDeferredSkinned[] vert;
            int[] ind;

            Token.HeaderValueBinding[] hStart = new Token.HeaderValueBinding[]
            {
                new Token.HeaderValueBinding("Faces"),
                new Token.HeaderValueBinding("Vertices"),
                new Token.HeaderValueBinding("Indeces")
            };
            Token.HeaderValueBinding[] hVertex = new Token.HeaderValueBinding[]
            {
                new Token.HeaderValueBinding("v"),
                new Token.HeaderValueBinding("vt"),
                new Token.HeaderValueBinding("vn"),
                new Token.HeaderValueBinding("vb")
            };
            Token[] tA;


            int l = 0;
            using (FileStream fs = File.Open(file, FileMode.Open))
            {
                using (StreamReader s = new StreamReader(fs))
                {
                    // TODO: TokenStream
                    vb = null; ib = null;
                    //if (!Token.readUntilAllHeaders(s, out tA, hStart))
                    //{ throw new ArgumentException("Could Not Find Header"); }
                    //tA[1].getArg<int>(0, ref l);
                    //vert = new VertexDeferredSkinned[l];
                    //tA[2].getArg<int>(0, ref l);
                    //ind = new int[l];

                    //vb = new VertexBuffer(g, VertexDeferredSkinned.Declaration, l, BufferUsage.None);
                    //ib = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, l, BufferUsage.None);

                    //Vector3[] pos = new Vector3[3], norm = new Vector3[3];
                    //Vector2[] uv = new Vector2[3];
                    //Vector3[] tpos = new Vector3[3], tnorm = new Vector3[3];
                    //Vector3 tan, bin;
                    //int[] b = new int[3];

                    //for (int i = 0; i < l; )
                    //{
                    //    for (int iv = 0; iv < 3; iv++)
                    //    {
                    //        if (!Token.readUntilAllHeaders(s, out tA, hVertex))
                    //        { throw new ArgumentException("Could Not Find Vertex"); }
                    //        tA[0].getArgVector3(0, ref pos[iv]);
                    //        tA[1].getArgVector2(0, ref uv[iv]);
                    //        tA[2].getArgVector3(0, ref norm[iv]);
                    //        tA[3].getArg<int>(0, ref b[iv]);

                    //    }

                    //    Matrix r = Matrix.Identity;// Matrix.CreateRotationX(-MathHelper.PiOver2);
                    //    Vector3.Transform(tpos, ref r, pos);
                    //    Vector3.Transform(tnorm, ref r, norm);

                    //    XNA3D.Extensions.Vector3Ext.calculateTanBin(pos, uv, out tan, out bin);

                    //    for (int iv = 0; iv < 3; iv++, i++)
                    //    {
                    //        vert[i] = new VertexDeferredSkinned(pos[iv], norm[iv], uv[iv], tan, bin, b[iv]);
                    //        ind[i] = i;
                    //    }
                    //    int swap = ind[i - 1];
                    //    ind[i - 1] = ind[i - 3];
                    //    ind[i - 3] = swap;
                    //}
                }
            }
            //vb.SetData<VertexDeferredSkinned>(vert);
            //ib.SetData<int>(ind);
        }
    }
}
