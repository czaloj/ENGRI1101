using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNA2D.Interfaces.Collision;

namespace XNA2D.Framework.Collision
{
    public abstract class CollisionLink<A, B> : ICollisionCalculator
        where A : ICollisionContainer
        where B : ICollisionContainer
    {
        protected A c1;
        protected B c2;

        public CollisionLink(A c1, B c2)
        {
            this.c1 = c1;
            this.c2 = c2;
        }

        public abstract CollisionInformation collide();
    }

    public static class CollisionLink
    {
        public static ICollisionCalculator getCollisionContainer(ICollisionContainer c1, ICollisionContainer c2)
        {
            switch (c1.CollisionType)
            {
                case COLLISION_TYPE.AABB:
                    switch (c2.CollisionType)
                    {
                        case COLLISION_TYPE.AABB:
                            return new CollisionLinkAABB2((ICollisionAABB)c1, (ICollisionAABB)c2);
                        case COLLISION_TYPE.RADIAL:
                            return new CollisionLinkAABBRadial((ICollisionAABB)c1, (ICollisionRadial)c2);
                        default:
                            return null;
                    }
                case COLLISION_TYPE.RADIAL:
                    switch (c2.CollisionType)
                    {
                        case COLLISION_TYPE.AABB:
                            return new CollisionLinkAABBRadial((ICollisionAABB)c2, (ICollisionRadial)c1);
                        case COLLISION_TYPE.RADIAL:
                            return new CollisionLinkRadial2((ICollisionRadial)c1, (ICollisionRadial)c2);
                        default:
                            return null;
                    }
                default:
                    return null;
            }
        }
    }
}
