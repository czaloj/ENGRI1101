using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA2D.Interfaces.Collision;

namespace XNA2D.Framework.Collision
{
    public class CollisionLinkAABBRadial : CollisionLink<ICollisionAABB, ICollisionRadial>
    {
        public CollisionLinkAABBRadial(ICollisionAABB c1, ICollisionRadial c2)
            : base(c1, c2)
        {

        }

        public override CollisionInformation collide()
        {
            CollisionInformation info = new CollisionInformation();

            info.Offset = c1.RectangleCenter - c2.CollisionCenter;
            info.Distance = info.Offset.Length();
            //Absolute Value
            Vector2 absOffset = new Vector2(
                info.Offset.X < 0f ? -info.Offset.X : info.Offset.X,
                info.Offset.Y < 0f ? -info.Offset.Y : info.Offset.Y
                );

            //Displacement 1
            Vector2 displace = absOffset - c1.RectangleHalfSize;
            displace.X -= c2.CollisionRadius;
            displace.Y -= c2.CollisionRadius;
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
            if (info.CollisionDepth <= 0f || otherDepth <= 0f)
            {
                info.HasCollided = false;
            }
            else
            {
                //Displacement 2
                displace = absOffset - c1.RectangleHalfSize;
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

                if (info.CollisionDepth > 0f || otherDepth >= 0f)
                {
                    info.HasCollided = true;
                }
                else
                {
                    //Displacement 3
                    float f1 = absOffset.X - c1.RectangleHalfSize.X;
                    float f2 = absOffset.Y - c1.RectangleHalfSize.Y;
                    float cornerDistance_sq = f1 * f1 + f2 * f2;
                    info.CollisionDepth = (c2.CollisionRadius * c2.CollisionRadius) - cornerDistance_sq;
                    info.HasCollided = info.CollisionDepth > 0f;
                }
            }
            return info;
        }
    }
}
