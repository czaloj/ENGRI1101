using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace XNA3D.Collision
{
    /// <summary>
    /// Specifies How A Collision Box Should Be Built From The Origin
    /// </summary>
    public struct CollisionBoxBuildInfo
    {
        //All Of The Faces Of The Box
        public CollisionFaceBuildInfo[][] faceInfo;

        /// <summary>
        /// Constructor Using Multiple Arrays Of Faces
        /// </summary>
        /// <param name="XP">Faces Facing X Pos</param>
        /// <param name="XN">Faces Facing X Neg</param>
        /// <param name="YP">Faces Facing Y Pos</param>
        /// <param name="YN">Faces Facing Y Neg</param>
        /// <param name="ZP">Faces Facing Z Pos</param>
        /// <param name="ZN">Faces Facing Z Neg</param>
        public CollisionBoxBuildInfo(
            CollisionFaceBuildInfo[] XP,
            CollisionFaceBuildInfo[] XN,
            CollisionFaceBuildInfo[] YP,
            CollisionFaceBuildInfo[] YN,
            CollisionFaceBuildInfo[] ZP,
            CollisionFaceBuildInfo[] ZN
            )
        {
            faceInfo = new CollisionFaceBuildInfo[6][];
            faceInfo[0] = XP;
            faceInfo[1] = XN;
            faceInfo[2] = YP;
            faceInfo[3] = YN;
            faceInfo[4] = ZP;
            faceInfo[5] = ZN;
        }
    }
    
    /// <summary>
    /// The Main Object Used In Finding 
    /// Collisions Between ICollidable Objects
    /// </summary>
    public struct CollisionBox
    {
        //All Of The Collision Faces Sorted By Direction
        public CollisionFace[][] faces;

        /// <summary>
        /// Constructor For Box From Build Info
        /// </summary>
        /// <param name="info">The Building Information</param>
        public CollisionBox(CollisionBoxBuildInfo info)
        {
            faces = new CollisionFace[6][];
            for (int f = 0; f < 6; f++)
            {
                faces[f] = new CollisionFace[info.faceInfo[f].Length];
                for (int i = 0; i < faces[f].Length; i++)
                {
                    faces[f][i] = new CollisionFace(info.faceInfo[f][i]);
                }
            }
        }

        /// <summary>
        /// Constructor That Offsets An Existing Box
        /// </summary>
        /// <param name="box">The Box To Clone And Offset</param>
        /// <param name="offset">The Amount To Offset The Box</param>
        public CollisionBox(CollisionBox box, Vector3 offset)
        {
            faces = new CollisionFace[6][];
            for (int f = 0; f < 6; f++)
            {
                faces[f] = new CollisionFace[box.faces[f].Length];
                for (int i = 0; i < faces[f].Length; i++)
                {
                    faces[f][i] = new CollisionFace(box.faces[f][i], offset);
                }
            }
        }

        /// <summary>
        /// Finds The Overall Collision Between An Object And Its Surroundings
        /// </summary>
        /// <param name="c1">The Main Collidable Object That Has Not Had Its Box Translated</param>
        /// <param name="c2">An Array Of Collidable Objects Ordered By The Direction Of Collision From The Main Collidable Object</param>
        /// <returns>An Array Of All The Maximum Collision Depths Encountered For Each Surrounding Cardinal Direction</returns>
        public static float[] collisionDepth(ICollidable c1, ICollidable[][] c2)
        {
            float[] maxDepth = new float[] { 0f, 0f, 0f, 0f, 0f, 0f };
            float[] checkedDepth = new float[] { 0f, 0f, 0f, 0f, 0f, 0f };

            //Perform All The Necessary Offsets
            CollisionBox b1 = new CollisionBox(c1.getBox(), c1.getBoxPosition());

            for (int f = 0; f < 6; f++)
            {
                if (c1.isFaceSet(f))
                {
                    if (c2[f] != null && c2[f].Length > 0)
                    {
                        maxDepth[f] = collisionDepth(b1, c2[f], (XYZ_DIRECTION)f);
                    }
                }
            }

            return maxDepth;
        }

        /// <summary>
        /// Get The Collision Depth Between A Box And Another Collidable Object
        /// </summary>
        /// <param name="b1">The Box</param>
        /// <param name="c2">The Collidable Object</param>
        /// <param name="f">The Direction To Check</param>
        /// <returns>The Collision Depth In The Checked Direction Or 0 If None</returns>
        public static float collisionDepth(CollisionBox b1, ICollidable[] c2, XYZ_DIRECTION f)
        {
            float maxDepth = 0f;
            float checkedDepth = 0f;
            foreach (ICollidable collidable in c2)
            {
                checkedDepth = collisionDepth(b1, collidable, f);
                if (checkedDepth > maxDepth && checkedDepth < 0.51f) { maxDepth = checkedDepth; }
            }
            return maxDepth;
        }

        /// <summary>
        /// Returns The Collision Depth Between The Box And Another
        /// Collidable Object
        /// </summary>
        /// <param name="b1">A Box That Has Already Been Translated To Its Necessary Place</param>
        /// <param name="c2">A Fresh Collidable Object That Has Not Had Its Box Translated</param>
        /// <param name="directionTowardsC2">The Direction That The Box Is Colliding With The Fresh Collidable Object</param>
        /// <returns>The Maximum Collision Depth Encountered For All The Faces In The Collision Direction</returns>
        public static float collisionDepth(CollisionBox b1, ICollidable c2, XYZ_DIRECTION directionTowardsC2)
        {
            float maxDepth = 0f;
            float checkedDepth = 0f;
            int c1DirectionToC2 = (int)directionTowardsC2;
            int c2DirectionToC1 = c1DirectionToC2 % 2 == 0
                ? c1DirectionToC2 + 1
                : c1DirectionToC2 - 1
                ;

            if (!c2.isFaceSet(c2DirectionToC1)) { return 0f; }

            //Perform All The Necessary Offsets
            CollisionBox b2 = c2.getBox();
            CollisionFace[] otherFaces = new CollisionFace[b2.faces[c2DirectionToC1].Length];
            Vector3 o2 = c2.getBoxPosition();
            for (int f = 0; f < b2.faces[c2DirectionToC1].Length; f++)
            {
                otherFaces[f] = new CollisionFace(b2.faces[c2DirectionToC1][f], o2);
            }

            foreach (CollisionFace face in b1.faces[c1DirectionToC2])
            {
                foreach (CollisionFace otherFace in otherFaces)
                {
                    checkedDepth = face.collisionDepth(otherFace);
                    if (checkedDepth > maxDepth && checkedDepth < 0.51f) { maxDepth = checkedDepth; }
                }
            }
            return maxDepth;
        }

        /// <summary>
        /// Calculate A Collision Between This Box And Another
        /// </summary>
        /// <param name="box">The Other Box That Is In The Collision</param>
        /// <param name="boxDirection">The Direction That This Box Is Colliding With The Other</param>
        /// <returns>The Maximum Collision Depth Between The Two Boxes</returns>
        public float collisionDepth(CollisionBox box, XYZ_DIRECTION boxDirection)
        {
            float maxDepth = -1f;
            float checkedDepth = 0f;
            int boxDirectionInt = (int)boxDirection;
            int oppositeDirection = (int)boxDirection % 2 == 0
                ? (int)boxDirection + 1
                : (int)boxDirection - 1
                ;

            foreach (CollisionFace face in faces[oppositeDirection])
            {
                foreach (CollisionFace otherFace in faces[boxDirectionInt])
                {
                    checkedDepth = face.collisionDepth(otherFace);
                    if (checkedDepth > maxDepth && checkedDepth < 0.51f) { maxDepth = checkedDepth; }
                }
            }
            return maxDepth;
        }

        public void read(StreamReader s)
        {
            Token t;
            int[] c = new int[6];
            if (!Token.readUntilHeader(s, "count", out t))
            { throw new SerializingException("Expected Count Of All 6 Faces"); }
            if (t.ArgumentCount < 6)
            { throw new ArgumentException("Expected 6 Arguments"); }
            if (!(
                t.getArg<int>(0, ref c[0]) &&
                t.getArg<int>(1, ref c[1]) &&
                t.getArg<int>(2, ref c[2]) &&
                t.getArg<int>(3, ref c[3]) &&
                t.getArg<int>(4, ref c[4]) &&
                t.getArg<int>(5, ref c[5])
                ))
            {
                throw new ArgumentException("Expecting Integers");
            }
            faces = new CollisionFace[6][];
            for (int i = 0; i < 6; i++)
            {
                if (c[i] > 0)
                {
                    faces[i] = new CollisionFace[c[i]];
                    for (int f = 0; f < faces[i].Length; f++)
                    {
                        faces[i][f] = CollisionFace.read(s);
                    }
                }
            }
        }
    }
}
