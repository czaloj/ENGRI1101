using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA3D.Collision.Interfaces;

namespace XNA3D.Collision.Bodies
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
            info.Offset = c1.BoxCenter - c2.BoxCenter;
            info.Distance = info.Offset.Length();

            Vector3 absOffset = new Vector3(
                info.Offset.X < 0f ? -info.Offset.X : info.Offset.X,
                info.Offset.Y < 0f ? -info.Offset.Y : info.Offset.Y,
                info.Offset.Z < 0f ? -info.Offset.Z : info.Offset.Z
                );

            Vector3 displace = absOffset - (c1.BoxHalfSize + c2.BoxHalfSize);
            float otherDepth;
            float otherDepth2;
            if (displace.X < displace.Y)
            {
                if (displace.X < displace.Z)
                {
                    info.CollisionDepth = -displace.X;
                    otherDepth = -displace.Y;
                    otherDepth2 = -displace.Z;
                }
                else
                {
                    info.CollisionDepth = -displace.Z;
                    otherDepth = -displace.Y;
                    otherDepth2 = -displace.X;
                }
            }
            else
            {
                if (displace.Y < displace.Z)
                {
                    info.CollisionDepth = -displace.Y;
                    otherDepth = -displace.X;
                    otherDepth2 = -displace.Z;
                }
                else
                {
                    info.CollisionDepth = -displace.Z;
                    otherDepth = -displace.Y;
                    otherDepth2 = -displace.X;
                }
            }

            if (info.CollisionDepth > 0f && otherDepth >= 0f && otherDepth2 >= 0f)
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
