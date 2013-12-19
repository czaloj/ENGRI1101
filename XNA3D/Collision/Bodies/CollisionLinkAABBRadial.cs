using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XNA3D.Collision.Interfaces;

namespace XNA3D.Collision.Bodies
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

            info.Offset = c1.BoxCenter - c2.CollisionCenter;
            info.Distance = info.Offset.Length();
            //Absolute Value
            Vector3 absOffset = new Vector3(
                info.Offset.X < 0f ? -info.Offset.X : info.Offset.X,
                info.Offset.Y < 0f ? -info.Offset.Y : info.Offset.Y,
                info.Offset.Z < 0f ? -info.Offset.Z : info.Offset.Z
                );

            //Displacement 1
            Vector3 displace = absOffset - c1.BoxHalfSize;
            displace.X -= c2.CollisionRadius;
            displace.Y -= c2.CollisionRadius;
            displace.Z -= c2.CollisionRadius;
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

            if (info.CollisionDepth <= 0f || otherDepth <= 0f || otherDepth2 <= 0f)
            {
                info.HasCollided = false;
            }
            else
            {
                //Displacement 2
                displace = absOffset - c1.BoxHalfSize;
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

                if (info.CollisionDepth > 0f || otherDepth >= 0f)
                {
                    info.HasCollided = true;
                }
                else
                {
                    //Displacement 3
                    float f1 = absOffset.X - c1.BoxHalfSize.X;
                    float f2 = absOffset.Y - c1.BoxHalfSize.Y;
                    float f3 = absOffset.Z - c1.BoxHalfSize.Z;
                    float cornerDistance_sq = f1 * f1 + f2 * f2 + f3 * f3;
                    info.CollisionDepth = (c2.CollisionRadius * c2.CollisionRadius) - cornerDistance_sq;
                    info.HasCollided = info.CollisionDepth > 0f;
                }
            }
            return info;
        }
    }
}
