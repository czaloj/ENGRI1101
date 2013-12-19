using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using XNA3D;

namespace XNA3D.Collision
{
    /// <summary>
    /// Specifies How A Collision Face Should Be Built From The Origin
    /// </summary>
    public struct CollisionFaceBuildInfo
    {
        //Offset From The Base Point
        public Vector3 offset;

        //Size Of The Face Out From Offset
        public Vector2 size;

        //Direction The Face Will Collide
        public XYZ_DIRECTION facing;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="offset">Offset From The Base Point</param>
        /// <param name="size">Size Of The Face Out From Offset</param>
        /// <param name="facing">Direction The Face Will Collide</param>
        public CollisionFaceBuildInfo(Vector3 offset, Vector2 size, XYZ_DIRECTION facing)
        {
            this.offset = offset;
            this.size = size;
            this.facing = facing;
        }
    }

    /// <summary>
    /// The Most Primitive Object Used In Collision Detection
    /// </summary>
    public struct CollisionFace
    {
        //The 4 Vertices That Make Up The Face
        public Vector3[] vertices;

        //The Direction That The Face Is Facing
        public XYZ_DIRECTION facing;

        /// <summary>
        /// Builds The Face From The Building Info
        /// </summary>
        /// <param name="info">Info For The Face</param>
        public CollisionFace(CollisionFaceBuildInfo info)
            : this(info.offset, info.size, info.facing)
        {
        }

        /// <summary>
        /// Build A Face From An Offset, Size, And Cardinal Direction
        /// </summary>
        /// <param name="offset">Offset From The Base Point</param>
        /// <param name="size">Size Of The Face Out From Offset</param>
        /// <param name="facing">Direction The Face Will Collide</param>
        public CollisionFace(Vector3 offset, Vector2 size, XYZ_DIRECTION facing)
        {
            //Get The Facing Direction
            this.facing = facing;

            //Build The Vertices In Same Order As Voxel Faces
            vertices = new Vector3[4];
            vertices[0] = offset;
            switch (facing)
            {
                case XYZ_DIRECTION.XP:
                    vertices[1] = new Vector3(offset.X, offset.Y, offset.Z - size.X);
                    vertices[2] = new Vector3(offset.X, offset.Y - size.Y, offset.Z);
                    vertices[3] = new Vector3(offset.X, offset.Y - size.Y, offset.Z - size.X);
                    break;
                case XYZ_DIRECTION.XN:
                    vertices[1] = new Vector3(offset.X, offset.Y, offset.Z + size.X);
                    vertices[2] = new Vector3(offset.X, offset.Y - size.Y, offset.Z);
                    vertices[3] = new Vector3(offset.X, offset.Y - size.Y, offset.Z + size.X);
                    break;
                case XYZ_DIRECTION.YP:
                    vertices[1] = new Vector3(offset.X + size.X, offset.Y, offset.Z);
                    vertices[2] = new Vector3(offset.X, offset.Y, offset.Z + size.Y);
                    vertices[3] = new Vector3(offset.X + size.X, offset.Y, offset.Z + size.Y);
                    break;
                case XYZ_DIRECTION.YN:
                    vertices[1] = new Vector3(offset.X + size.X, offset.Y, offset.Z);
                    vertices[2] = new Vector3(offset.X, offset.Y, offset.Z - size.Y);
                    vertices[3] = new Vector3(offset.X + size.X, offset.Y, offset.Z - size.Y);
                    break;
                case XYZ_DIRECTION.ZP:
                    vertices[1] = new Vector3(offset.X + size.X, offset.Y, offset.Z);
                    vertices[2] = new Vector3(offset.X, offset.Y - size.Y, offset.Z);
                    vertices[3] = new Vector3(offset.X + size.X, offset.Y - size.Y, offset.Z);
                    break;
                case XYZ_DIRECTION.ZN:
                    vertices[1] = new Vector3(offset.X - size.X, offset.Y, offset.Z);
                    vertices[2] = new Vector3(offset.X, offset.Y - size.Y, offset.Z);
                    vertices[3] = new Vector3(offset.X - size.X, offset.Y - size.Y, offset.Z);
                    break;
                default:
                    vertices[1] = new Vector3(offset.X, offset.Y, offset.Z + size.X);
                    vertices[2] = new Vector3(offset.X, offset.Y - size.Y, offset.Z);
                    vertices[3] = new Vector3(offset.X, offset.Y - size.Y, offset.Z + size.X);
                    break;
            }
        }

        /// <summary>
        /// Constructor By Offsetting An Existing Collision Face
        /// </summary>
        /// <param name="face">The Existing Face Data To Be Copied And Offset</param>
        /// <param name="offset">Amount To Offset The Face</param>
        public CollisionFace(CollisionFace face, Vector3 offset)
        {
            //Keep The Same Facing Direction
            facing = face.facing;

            //Move The Vertices By The Offset
            vertices = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                vertices[i] = face.vertices[i] + offset;
            }
        }

        /// <summary>
        /// Calculates A Collision Between This Face And Another
        /// It Is Assumed That The Other Face Is Facing The Correct Direction
        /// </summary>
        /// <param name="otherFace">The Other Face Present In The Collision</param>
        /// <returns>The Depth Of Collision That Was Encountered</returns>
        public float collisionDepth(CollisionFace otherFace)
        {
            //First Check If Faces Overlap In Their Direction
            //Then, Return Depth Between Them Or No Collision (-1f)

            switch (facing)
            {
                case XYZ_DIRECTION.XP:
                    if (overlapXP(otherFace))
                    {
                        return vertices[0].X - otherFace.vertices[0].X;
                    }
                    else
                    {
                        return 0f;
                    }
                case XYZ_DIRECTION.XN:
                    if (overlapXN(otherFace))
                    {
                        return otherFace.vertices[0].X - vertices[0].X;
                    }
                    else
                    {
                        return 0f;
                    }
                case XYZ_DIRECTION.YP:
                    if (overlapYP(otherFace))
                    {
                        return vertices[0].Y - otherFace.vertices[0].Y;
                    }
                    else
                    {
                        return 0f;
                    }
                case XYZ_DIRECTION.YN:
                    if (overlapYN(otherFace))
                    {
                        return otherFace.vertices[0].Y - vertices[0].Y;
                    }
                    else
                    {
                        return 0f;
                    }
                case XYZ_DIRECTION.ZP:
                    if (overlapZP(otherFace))
                    {
                        return vertices[0].Z - otherFace.vertices[0].Z;
                    }
                    else
                    {
                        return 0f;
                    }
                case XYZ_DIRECTION.ZN:
                    if (overlapZN(otherFace))
                    {
                        return otherFace.vertices[0].Z - vertices[0].Z;
                    }
                    else
                    {
                        return 0f;
                    }
                default:
                    return 0f;
            }
        }

        #region Check Whether Faces Can Collide In Specified Directions
        public bool overlapXP(CollisionFace otherFace)
        {
            return
                vertices[0].Z > otherFace.vertices[0].Z &&
                vertices[1].Z < otherFace.vertices[1].Z &&
                vertices[0].Y > otherFace.vertices[2].Y &&
                vertices[2].Y < otherFace.vertices[0].Y
                ;
        }
        public bool overlapXN(CollisionFace otherFace)
        {
            return
                vertices[0].Z < otherFace.vertices[0].Z &&
                vertices[1].Z > otherFace.vertices[1].Z &&
                vertices[0].Y > otherFace.vertices[2].Y &&
                vertices[2].Y < otherFace.vertices[0].Y
                ;
        }
        public bool overlapYP(CollisionFace otherFace)
        {
            return
                vertices[0].X < otherFace.vertices[1].X &&
                vertices[1].X > otherFace.vertices[0].X &&
                vertices[0].Z < otherFace.vertices[0].Z &&
                vertices[2].Z > otherFace.vertices[2].Z
                ;
        }
        public bool overlapYN(CollisionFace otherFace)
        {
            return
                vertices[0].X < otherFace.vertices[1].X &&
                vertices[1].X > otherFace.vertices[0].X &&
                vertices[0].Z > otherFace.vertices[0].Z &&
                vertices[2].Z < otherFace.vertices[2].Z
                ;
        }
        public bool overlapZP(CollisionFace otherFace)
        {
            return
                vertices[0].X < otherFace.vertices[0].X &&
                vertices[1].X > otherFace.vertices[1].X &&
                vertices[0].Y > otherFace.vertices[2].Y &&
                vertices[2].Y < otherFace.vertices[0].Y
                ;
        }
        public bool overlapZN(CollisionFace otherFace)
        {
            return
                vertices[0].X > otherFace.vertices[0].X &&
                vertices[1].X < otherFace.vertices[1].X &&
                vertices[0].Y > otherFace.vertices[2].Y &&
                vertices[2].Y < otherFace.vertices[0].Y
                ;
        }
        #endregion

        public static CollisionFace read(StreamReader s)
        {
            Token t;
            if (!Token.readUntilHeader(s, "face", out t))
            { throw new SerializingException("Expecting To Find A Collision Face"); }
            if (t.ArgumentCount < 6)
            { throw new ArgumentException("6 Arguments Expected"); }
            Vector3 v3 = Vector3.Zero;
            Vector2 v2 = Vector2.Zero;
            int f = 0;
            if (!(
                t.getArg<float>(0, ref v3.X) &&
                t.getArg<float>(1, ref v3.Y) &&
                t.getArg<float>(2, ref v3.Z) &&
                t.getArg<float>(3, ref v2.X) &&
                t.getArg<float>(4, ref v2.Y) &&
                t.getArg<int>(5, ref f)
                ))
            {
                throw new ArgumentException("Incorrect Format");
            }
            return new CollisionFace(v3, v2, (XYZ_DIRECTION)f);
        }
    }
}
