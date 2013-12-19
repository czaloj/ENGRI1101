using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA2D.Interfaces.Collision
{
    public interface ICollisionAABB : ICollisionContainer
    {
        Vector2 RectangleCenter { get; }
        Vector2 RectangleHalfSize { get; }
    }
}
