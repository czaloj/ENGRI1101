using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA3D.Collision.Bodies;

namespace XNA3D.Collision.Interfaces
{
    public interface ICollisionCalculator
    {
        CollisionInformation collide();
    }
}
