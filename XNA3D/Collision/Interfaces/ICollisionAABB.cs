using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA3D.Collision.Interfaces
{
    public interface ICollisionAABB : ICollisionContainer
    {
        Vector3 BoxCenter { get; }
        Vector3 BoxHalfSize { get; }
    }
}
