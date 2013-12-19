using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA2D.Framework.Collision;

namespace XNA2D.Interfaces.Collision
{
    public interface ICollisionContainer
    {
        COLLISION_TYPE CollisionType { get; }
    }
}
