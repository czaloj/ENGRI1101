using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA3D.Collision.Interfaces
{
    public interface ICollisionRadial : ICollisionContainer
    {
        Vector3 CollisionCenter { get; }
        float CollisionRadius { get; }
    }
}
