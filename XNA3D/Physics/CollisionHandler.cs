using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA3D.Physics
{
    public class CollisionHandler
    {
        public const byte NoSide = 0x00;
        public const byte SideX = 0x01;
        public const byte SideY = 0x02;
        public const byte SideZ = 0x04;


        public const float MaxCollisionTime = 1000f;
        public const byte CheckXDirection = 0x01;
        public const byte CheckYDirection = 0x02;
        public const byte CheckZDirection = 0x04;

        public static void getCollision(AABB self, AABB other, out CollisionInformation info)
        {
            info = new CollisionInformation()
            {
                //Get The Offset Between Them
                Direction = other.Center - self.Center
            };

            //Get The Separation Between The Two Boxes
            Vector3 absDisp;
            Vector3Ext.absolute(info.Direction, out absDisp);
            info.Depth = (self.HalfSize + other.HalfSize) - absDisp;

            if (info.Depth.X < 0 || info.Depth.Z < 0 || info.Depth.Y < 0)
            {
                //They Have Not Collided
                info.Collided = false;
            }
            else
            {
                info.Collided = true;

                //Figure Out More Collision Information
                info.Direction.Normalize();
            }
        }
        public static float getTimeUntilCollision(AABB B, AABB A, Vector3 vBrA, out Vector3 cFirstTime, out bool[] mod)
        {
            mod = new bool[3];

            //Get Bounding Locations
            Vector3 sMin, sMax, oMin, oMax;
            sMin = A.min(); sMax = A.max();
            oMin = B.min(); oMax = B.max();

            float u0, u1;
            Vector3 v = vBrA;

            cFirstTime = Vector3.Zero;
            Vector3 cLastTime = Vector3.One;

            //CollisionInformation ci;
            //CollisionHandler.getCollision(self, other, out ci);
            //if (ci.Collided)
            //{
            //    u0 = u1 = MaxCollisionTime;
            //    return u0;
            //}

            if (sMax.X < oMin.X && v.X < 0) { cFirstTime.X = (sMax.X - oMin.X) / v.X; mod[0] = true; }
            else if (oMax.X < sMin.X && v.X > 0) { cFirstTime.X = (sMin.X - oMax.X) / v.X; mod[0] = true; }
            if (oMax.X > sMin.X && v.X < 0) { cLastTime.X = (sMin.X - oMax.X) / v.X; mod[0] = true; }
            else if (sMax.X > oMin.X && v.X > 0) { cLastTime.X = (sMax.X - oMin.X) / v.X; mod[0] = true; }
            mod[0] &= cFirstTime.X >= 0;

            if (sMax.Y < oMin.Y && v.Y < 0) { cFirstTime.Y = (sMax.Y - oMin.Y) / v.Y; mod[1] = true; }
            else if (oMax.Y < sMin.Y && v.Y > 0) { cFirstTime.Y = (sMin.Y - oMax.Y) / v.Y; mod[1] = true; }
            if (oMax.Y > sMin.Y && v.Y < 0) { cLastTime.Y = (sMin.Y - oMax.Y) / v.Y; mod[1] = true; }
            else if (sMax.Y > oMin.Y && v.Y > 0) { cLastTime.Y = (sMax.Y - oMin.Y) / v.Y; mod[1] = true; }
            mod[1] &= cFirstTime.Y >= 0;

            if (sMax.Z < oMin.Z && v.Z < 0) { cFirstTime.Z = (sMax.Z - oMin.Z) / v.Z; mod[2] = true; }
            else if (oMax.Z < sMin.Z && v.Z > 0) { cFirstTime.Z = (sMin.Z - oMax.Z) / v.Z; mod[2] = true; }
            if (oMax.Z > sMin.Z && v.Z < 0) { cLastTime.Z = (sMin.Z - oMax.Z) / v.Z; mod[2] = true; }
            else if (sMax.Z > oMin.Z && v.Z > 0) { cLastTime.Z = (sMax.Z - oMin.Z) / v.Z; mod[2] = true; }
            mod[2] &= cFirstTime.Z >= 0;

            u0 = MathHelper.Max(cFirstTime.X, MathHelper.Max(cFirstTime.Y, cFirstTime.Z));
            u1 = MathHelper.Min(cLastTime.X, MathHelper.Min(cLastTime.Y, cLastTime.Z));

            if (u0 <= u1)
            {
                return u0;
            }
            else
            {
                return MaxCollisionTime;
            }

            ////Find Box Between 
            //Vector3 exMin = Vector3.Zero, exMax = Vector3.Zero, exSize;
            //if (self.Center.X > other.Center.X)
            //{ exMin.X = oMax.X; exMax.X = sMin.X; }
            //else
            //{ exMin.X = sMax.X; exMax.X = oMin.X; }
            //if (self.Center.Y > other.Center.Y)
            //{ exMin.Y = oMax.Y; exMax.Y = sMin.Y; }
            //else
            //{ exMin.Y = sMax.Y; exMax.Y = oMin.Y; }
            //if (self.Center.Z > other.Center.Z)
            //{ exMin.Z = oMax.Z; exMax.Z = sMin.Z; }
            //else
            //{ exMin.Z = sMax.Z; exMax.Z = oMin.Z; }
            //exSize = exMax - exMin;

            //if (exSize.X < 0)
            //{ cFirstTime.X = MaxCollisionTime; }
            //else
            //{
            //    cFirstTime.X = exSize.X / vSrO.X;
            //    if (cFirstTime.X < 0f)
            //    {
            //        cFirstTime.X = MaxCollisionTime;
            //    }
            //}
            //float outTime = 0f;
            //Vector3 sSelf, sOther;
            //Vector3 sMod = new Vector3(
            //    vSrO.X >= 0 ? 1 : -1,
            //    vSrO.Y >= 0 ? 1 : -1,
            //    vSrO.Z >= 0 ? 1 : -1
            //    );
            //sSelf = self.Center + sMod * self.HalfSize;
            //sOther = other.Center - sMod * other.HalfSize;

            //if (vSrO.X != 0f) { cFirstTime.X = (sOther.X - sSelf.X) / vSrO.X; }
            //else { cFirstTime.X = -1f; }
            //if (vSrO.Y != 0f) { cFirstTime.Y = (sOther.Y - sSelf.Y) / vSrO.Y; }
            //else { cFirstTime.Y = -1f; }
            //if (vSrO.Z != 0f) { cFirstTime.Z = (sOther.Z - sSelf.Z) / vSrO.Z; }
            //else { cFirstTime.Z = -1f; }
            //if (cFirstTime.X < 0) { cFirstTime.X = -1f; }
            //if (cFirstTime.Y < 0) { cFirstTime.Y = -1f; }
            //if (cFirstTime.Z < 0) { cFirstTime.Z = -1f; }
            //return MathHelper.Max(cFirstTime.X, MathHelper.Max(cFirstTime.Y, cFirstTime.Z));
        }

        public static EntryExitTime getEETimes(AABB A, AABB B, Vector3 vArB, out EntryExitTime[] axes)
        {
            EntryExitTime ee;
            axes = new EntryExitTime[3]
            {
                EntryExitTime.Impossible,
                EntryExitTime.Impossible,
                EntryExitTime.Impossible
            };
            axes[0] = getEETimeX(A, B, vArB);
            if (!axes[0].IsViable) { return EntryExitTime.Impossible; }

            axes[1] = getEETimeY(A, B, vArB);
            if (!axes[1].IsViable) { return EntryExitTime.Impossible; }

            axes[2] = getEETimeZ(A, B, vArB);
            if (!axes[2].IsViable) { return EntryExitTime.Impossible; }

            ee = axes[0];
            ee.modifyTimes(axes[1]);
            ee.modifyTimes(axes[2]);

            return ee;
        }
        public static EntryExitTime getEETimeX(AABB A, AABB B, Vector3 vArB)
        {
            EntryExitTime ee = new EntryExitTime(0f, float.PositiveInfinity);
            if (A.Center.X < B.Center.X)
            {
                if (vArB.X <= 0)
                {
                    ee.modifyExit((B.NX - A.PX) / vArB.X);
                    if (A.PX > B.NX) { ee.modifyEntry(0f); }
                    else { return EntryExitTime.Impossible; }
                }
                else
                {
                    ee.modifyExit((B.PX - A.NX) / vArB.X);
                    ee.modifyEntry((B.NX - A.PX) / vArB.X);
                }
                return ee;
            }
            else
            {
                return getEETimeX(B, A, -vArB);
            }
        }
        public static EntryExitTime getEETimeY(AABB A, AABB B, Vector3 vArB)
        {
            EntryExitTime ee = new EntryExitTime(0f, float.PositiveInfinity);
            if (A.Center.Y <= B.Center.Y)
            {
                if (vArB.Y <= 0)
                {
                    ee.modifyExit((B.NY - A.PY) / vArB.Y);
                    if (A.PY > B.NY) { ee.modifyEntry(0f); }
                    else { return EntryExitTime.Impossible; }
                }
                else
                {
                    ee.modifyExit((B.PY - A.NY) / vArB.Y);
                    ee.modifyEntry((B.NY - A.PY) / vArB.Y);
                }
                return ee;
            }
            else
            {
                return getEETimeY(B, A, -vArB);
            }
        }
        public static EntryExitTime getEETimeZ(AABB A, AABB B, Vector3 vArB)
        {
            EntryExitTime ee = new EntryExitTime(0f, float.PositiveInfinity);
            if (A.Center.Z <= B.Center.Z)
            {
                if (vArB.Z <= 0)
                {
                    ee.modifyExit((B.NZ - A.PZ) / vArB.Z);
                    if (A.PZ > B.NZ) { ee.modifyEntry(0f); }
                    else { return EntryExitTime.Impossible; }
                }
                else
                {
                    ee.modifyExit((B.PZ - A.NZ) / vArB.Z);
                    ee.modifyEntry((B.NZ - A.PZ) / vArB.Z);
                }
                return ee;
            }
            else
            {
                return getEETimeZ(B, A, -vArB);
            }
        }

        public static float getTimeUntilCollision(AABB B, AABB A, Vector3 vBrA, out Vector3 cFirstTime, out bool[] mod, byte dir)
        {
            mod = new bool[3];
            bool[] previouslyTouching = new bool[3];

            //Get Bounding Locations
            Vector3 sMin, sMax, oMin, oMax;
            Vector3 aMin, aMax, bMin, bMax;
            aMin = A.min(); aMax = A.max();
            bMin = B.min(); bMax = B.max();

            sMin = A.min(); sMax = A.max();
            oMin = B.min(); oMax = B.max();

            float u0, u1;
            Vector3 v = vBrA;

            cFirstTime = Vector3.Zero;
            Vector3 cLastTime = Vector3.One;

            if ((dir & CheckXDirection) == CheckXDirection)
            {
                //if (v.X == 0)
                //{
                //    //Check For Touching
                //    if (aMax.X >= bMin.X && bMax.X >= aMin.X) { cFirstTime.X = 0; previouslyTouching[0] = true; }
                //}
                //else
                //{
                //    if (v.X > 0) { cFirstTime.X = (bMin.X - aMax.X) / v.X; }
                //    else { cFirstTime.X = (bMax.X - aMin.X) / v.X; }

                //    if (cFirstTime.X <= 0)
                //    {
                //        //They Were Previously Touching
                //        previouslyTouching[0] = true;
                //    }
                //}
                if (sMax.X < oMin.X && v.X < 0) { cFirstTime.X = (sMax.X - oMin.X) / v.X; mod[0] = true; }
                else if (oMax.X < sMin.X && v.X > 0) { cFirstTime.X = (sMin.X - oMax.X) / v.X; mod[0] = true; }
                if (oMax.X > sMin.X && v.X < 0) { cLastTime.X = (sMin.X - oMax.X) / v.X; mod[0] = true; }
                else if (sMax.X > oMin.X && v.X > 0) { cLastTime.X = (sMax.X - oMin.X) / v.X; mod[0] = true; }
                mod[0] &= cFirstTime.X >= 0;
            }
            else { cFirstTime.X = -MaxCollisionTime; }
            if ((dir & CheckYDirection) == CheckYDirection)
            {
                //if (v.X == 0)
                //{
                //    //Check For Touching
                //    if (aMax.X >= bMin.X && bMax.X >= aMin.X) { cFirstTime.X = 0; previouslyTouching[0] = true; }
                //}
                //else
                //{
                //    if (v.X > 0) { cFirstTime.X = (bMin.X - aMax.X) / v.X; }
                //    else { cFirstTime.X = (bMax.X - aMin.X) / v.X; }

                //    if (cFirstTime.X <= 0)
                //    {
                //        //They Were Previously Touching
                //        previouslyTouching[0] = true;
                //    }
                //}
                if (sMax.Y < oMin.Y && v.Y < 0) { cFirstTime.Y = (sMax.Y - oMin.Y) / v.Y; mod[1] = true; }
                else if (oMax.Y < sMin.Y && v.Y > 0) { cFirstTime.Y = (sMin.Y - oMax.Y) / v.Y; mod[1] = true; }
                if (oMax.Y > sMin.Y && v.Y < 0) { cLastTime.Y = (sMin.Y - oMax.Y) / v.Y; mod[1] = true; }
                else if (sMax.Y > oMin.Y && v.Y > 0) { cLastTime.Y = (sMax.Y - oMin.Y) / v.Y; mod[1] = true; }
                mod[1] &= cFirstTime.Y >= 0;
            }
            else { cFirstTime.Y = -MaxCollisionTime; }
            if ((dir & CheckZDirection) == CheckZDirection)
            {
                //if (v.X == 0)
                //{
                //    //Check For Touching
                //    if (aMax.X >= bMin.X && bMax.X >= aMin.X) { cFirstTime.X = 0; previouslyTouching[0] = true; }
                //}
                //else
                //{
                //    if (v.X > 0) { cFirstTime.X = (bMin.X - aMax.X) / v.X; }
                //    else { cFirstTime.X = (bMax.X - aMin.X) / v.X; }

                //    if (cFirstTime.X <= 0)
                //    {
                //        //They Were Previously Touching
                //        previouslyTouching[0] = true;
                //    }
                //}
                if (sMax.Z < oMin.Z && v.Z < 0) { cFirstTime.Z = (sMax.Z - oMin.Z) / v.Z; mod[2] = true; }
                else if (oMax.Z < sMin.Z && v.Z > 0) { cFirstTime.Z = (sMin.Z - oMax.Z) / v.Z; mod[2] = true; }
                if (oMax.Z > sMin.Z && v.Z < 0) { cLastTime.Z = (sMin.Z - oMax.Z) / v.Z; mod[2] = true; }
                else if (sMax.Z > oMin.Z && v.Z > 0) { cLastTime.Z = (sMax.Z - oMin.Z) / v.Z; mod[2] = true; }
                mod[2] &= cFirstTime.Z >= 0;
            }
            else { cFirstTime.Z = -MaxCollisionTime; }
            u0 = MathHelper.Max(cFirstTime.X, MathHelper.Max(cFirstTime.Y, cFirstTime.Z));
            u1 = MathHelper.Min(cLastTime.X, MathHelper.Min(cLastTime.Y, cLastTime.Z));

            if (u0 <= u1)
            {
                return u0;
            }
            else
            {
                return MaxCollisionTime;
            }
        }

        static Vector3 getWeightedPlane(ISimpleBody b1, ISimpleBody b2)
        {
            //Get Unit Vectors For Calculating Plane
            float sp1 = b1.Phys_Velocity.Length(); float sp2 = b2.Phys_Velocity.Length();
            Vector3 uV1 = b1.Phys_Velocity / sp1; Vector3 uV2 = b2.Phys_Velocity / sp2;
            Vector3 dist = b2.Phys_Position - b1.Phys_Position;
            //Get Vectors Pointing To The Plane
            if (Vector3.Dot(uV1, dist) < 0) { uV1 = -uV1; }
            if (Vector3.Dot(uV2, -dist) < 0) { uV1 = -uV1; }

            //Axial Cross
            Vector3 cr1 = Vector3.Cross(uV1, uV2);

            //Weighted Cross
            float ang = Vector3.Dot(uV1, uV2);
            float nsp = sp1 + sp2;
            float rsp1 = sp1 / nsp;
            Quaternion rot = Quaternion.CreateFromAxisAngle(cr1, rsp1 / ang);
            Vector3 cr2 = Vector3.Transform(Vector3.Normalize(uV1 + uV2), rot);

            return Vector3.Cross(cr1, cr2);
        }
        public static Vector3 getCollisionPlane(AABB b1, AABB b2, CollisionInformation ci)
        {
            Vector3Ext.absolute(ref ci.Depth);
            if (ci.Depth.X < ci.Depth.Y && ci.Depth.X < ci.Depth.Z)
            {
                if (b1.Center.X < b2.Center.X)
                {
                    return Vector3.Left;
                }
                else
                {
                    return Vector3.Right;
                }
            }
            else if (ci.Depth.Y < ci.Depth.Z)
            {
                if (b1.Center.Y < b2.Center.Y)
                {
                    return Vector3.Down;
                }
                else
                {
                    return Vector3.Up;
                }
            }
            else
            {
                if (b1.Center.Z < b2.Center.Z)
                {
                    return Vector3.Forward;
                }
                else
                {
                    return Vector3.Backward;
                }
            }
        }

        public static void getNewVelocities(ISimpleBody b1, ISimpleBody b2, out Vector3 nv1, out Vector3 nv2, Vector3 planeNormal, float elasticity)
        {
            //Vector3 a = Vector3.One * (-(1f + b2.Mass / b1.Mass) / 2f);
            //Vector3 b = (b2.Mass / b1.Mass) * b1.Phys_Velocity + b2.Phys_Velocity;
            //Vector3 hv = b2.Phys_Velocity * b2.Phys_Velocity / 2f;
            //Vector3 c = (-(b2.Mass / b1.Mass) * (qD / b1.Mass + b1.Phys_Velocity * b2.Phys_Velocity - hv) - hv);
            //nv1 = b1.Phys_Velocity + b2.Mass * (b2.Phys_Velocity - nv2) / b1.Mass;
            //nv2 = -b + bounce * Vector3Ext.sqrt(b * b - 4 * a * c);
            //nv2 /= 2 * a;

            //if (Vector3.Dot(b1.Phys_Velocity, b2.Phys_Velocity) < -0.9)
            //{
            //    nv1 = b1.Phys_Velocity;
            //    nv2 = b2.Phys_Velocity;
            //    return;
            //}

            Vector3 pr1 = Vector3Ext.project(b1.Phys_Velocity, -planeNormal);
            Vector3 pr2 = Vector3Ext.project(b2.Phys_Velocity, planeNormal);


            float sp1, sp2;
            sp1 = pr1.Length();
            sp2 = Vector3.Dot(b1.Phys_Velocity, b2.Phys_Velocity) > 0 ? pr2.Length() : -pr2.Length();

            float nsp1, nsp2;
            float r = (b1.Mass * sp1 + b2.Mass * sp2);
            nsp1 = (elasticity * b2.Mass * (sp2 - sp1) + r) / (b1.Mass + b2.Mass);
            nsp2 = (elasticity * b1.Mass * (sp1 - sp2) + r) / (b1.Mass + b2.Mass);

            nv1 = -planeNormal * (nsp1) - b1.Phys_Velocity;
            nv2 = b2.Phys_Velocity - planeNormal * (nsp2);

            return;
        }
    }
}
