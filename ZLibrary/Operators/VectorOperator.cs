using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace ZLibrary.Operators
{
    public static class VectorOperator
    {
        /// <summary>
        /// Calculates On Offset From A Rotated Object
        /// </summary>
        /// <param name="node">The True Position To Be Rotating Around</param>
        /// <param name="offset">The Local Offset That Is Desired</param>
        /// <param name="rotation">The "Full Quaternion" Representing Where The Offset Should Be Transformed And Then Added</param>
        /// <returns>A True Position From The Node Based On Offset And Rotation</returns>
        public static Vector3 getOffset(Vector3 node, Vector3 offset, Quaternion rotation)
        {
            return node + Vector3.Transform(offset, rotation);
        }
        /// <summary>
        /// Calculates The Unit Vector Representing Where The Object Is Looking At
        /// </summary>
        /// <param name="position">Position Of Object</param>
        /// <param name="rotation">Rotation Of Object</param>
        /// <returns>A Unit Vector</returns>
        public static Vector3 getLookingAt(Vector3 position, Quaternion rotation)
        {
            return position + getLocalForward(rotation);
        }
        /// <summary>
        /// Calculates Which Direction Is Forward
        /// </summary>
        /// <param name="rotation">Rotation To Be Calculated From</param>
        /// <returns>A Unit Vector Representing The Local Forward</returns>
        public static Vector3 getLocalForward(Quaternion rotation)
        {
            return Vector3.Transform(Vector3.Forward, rotation);
        }
        /// <summary>
        /// Calculates Which Direction Is Up
        /// </summary>
        /// <param name="rotation">Rotation To Be Calculated From</param>
        /// <returns>A Unit Vector Representing The Local Up</returns>
        public static Vector3 getLocalUp(Quaternion rotation)
        {
            return Vector3.Transform(Vector3.Up, rotation);
        }
        /// <summary>
        /// Calculates Which Direction Is Right
        /// </summary>
        /// <param name="rotation">Rotation To Be Calculated From</param>
        /// <returns>A Unit Vector Representing The Local Right</returns>
        public static Vector3 getLocalRight(Quaternion rotation)
        {
            return Vector3.Transform(Vector3.Right, rotation);
        }
        /// <summary>
        /// Calculates The Direction From One Position To Another
        /// </summary>
        /// <param name="position">The Starting Point</param>
        /// <param name="target">The End Point</param>
        /// <returns>A Unit Vector Representing Direction</returns>
        public static Vector3 directionToTarget(Vector3 position, Vector3 target)
        {
            return Vector3.Normalize(target - position);
        }
    }
}
