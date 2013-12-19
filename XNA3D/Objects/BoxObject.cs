using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA3D.Collision.Abstract;

namespace XNA3D.Objects
{
    public class BoxObject : AC3DCollidableAABB
    {
        public override void build()
        {
            base.build();
            VertexPositionTexture[] vert = new VertexPositionTexture[model.Meshes[0].MeshParts[0].VertexBuffer.VertexCount];
            model.Meshes[0].MeshParts[0].VertexBuffer.GetData<VertexPositionTexture>(vert);
            Vector2 mx = new Vector2(float.MaxValue, float.MinValue);
            Vector2 my = mx;
            Vector2 mz = mx;

            foreach (VertexPositionTexture vpt in vert)
            {
                if (vpt.Position.X < mx.X) { mx.X = vpt.Position.X; }
                if (vpt.Position.X > mx.Y) { mx.Y = vpt.Position.X; }
                if (vpt.Position.Y < my.X) { my.X = vpt.Position.Y; }
                if (vpt.Position.Y > my.Y) { my.Y = vpt.Position.Y; }
                if (vpt.Position.Z < mz.X) { mz.X = vpt.Position.Z; }
                if (vpt.Position.Z > mz.Y) { mz.Y = vpt.Position.Z; }
            }

            rectangleHalfSize = new Vector3(
                mx.Y - mx.X,
                my.Y - my.X,
                mz.Y - mz.X
                );
            rectangleHalfSize /= 2f;
            rectCenterOffset = new Vector3(
                (mx.X + mx.Y) / 2f,
                (my.X + my.Y) / 2f,
                (mz.X + mz.Y) / 2f
                );
        }
    }
}
