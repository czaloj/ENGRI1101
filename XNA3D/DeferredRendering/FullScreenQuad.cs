using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA3D.DeferredRendering
{
    public class FullScreenQuad
    {
        //Vertex Buffer
        VertexBuffer vb;
        //Index Buffer
        IndexBuffer ib;

        //Constructor
        public FullScreenQuad(GraphicsDevice GraphicsDevice)
        {
            //Vertices
            VertexPositionTexture[] vertices =
            {
                new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0))
            };
            //Make Vertex Buffer
            vb = new VertexBuffer(GraphicsDevice, VertexPositionTexture.VertexDeclaration,
            vertices.Length, BufferUsage.None);
            vb.SetData<VertexPositionTexture>(vertices);
            //Indices
            ushort[] indices = { 0, 1, 2, 2, 3, 0 };
            //Make Index Buffer
            ib = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits,
            indices.Length, BufferUsage.None);
            ib.SetData<ushort>(indices);
        }

        //Draw and Set Buffers
        public void Draw(GraphicsDevice GraphicsDevice)
        {
            ReadyBuffers(GraphicsDevice);
            JustDraw(GraphicsDevice);
        }

        //Set Buffers Onto GPU
        public void ReadyBuffers(GraphicsDevice GraphicsDevice)
        {
            //Set Vertex Buffer
            GraphicsDevice.SetVertexBuffer(vb);
            //Set Index Buffer
            GraphicsDevice.Indices = ib;
        }

        //Draw without Setting Buffers
        public void JustDraw(GraphicsDevice GraphicsDevice)
        {
            //Draw Quad
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        }
    }
}
