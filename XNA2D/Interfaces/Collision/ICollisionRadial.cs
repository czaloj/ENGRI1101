using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA2D.Interfaces.Collision
{
    public interface ICollisionRadial : ICollisionContainer
    {
        Vector2 CollisionCenter { get; }
        float CollisionRadius { get; }
    }
}
