using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA2D.Interfaces.Collision;

namespace XNA2D.Framework.Collision
{
    public class CollisionLinkAABB2 : CollisionLink<ICollisionAABB, ICollisionAABB>
    {
        public CollisionLinkAABB2(ICollisionAABB c1, ICollisionAABB c2)
            : base(c1, c2)
        {

        }

        public override CollisionInformation collide()
        {
            CollisionInformation info = new CollisionInformation();
            info.Offset = c1.RectangleCenter - c2.RectangleCenter;
            info.Distance = info.Offset.Length();

            Vector2 absOffset = new Vector2(
                info.Offset.X < 0f ? -info.Offset.X : info.Offset.X,
                info.Offset.Y < 0f ? -info.Offset.Y : info.Offset.Y
                );

            Vector2 displace = absOffset - (c1.RectangleHalfSize + c2.RectangleHalfSize);
            float otherDepth;
            if (displace.X < displace.Y)
            {
                info.CollisionDepth = -displace.X;
                otherDepth = -displace.Y;
            }
            else
            {
                info.CollisionDepth = -displace.Y;
                otherDepth = -displace.X;
            }

            if (info.CollisionDepth > 0f && otherDepth >= 0f)
            {
                info.HasCollided = true;
            }
            else
            {
                info.HasCollided = false;
            }
            return info;
        }
    }
}
