using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA3D.Collision
{
    /// <summary>
    /// Necessary Functions For An Object That Can Be Collided With
    /// </summary>
    public interface ICollidable
    {
        /// <summary>
        /// Gets The Collision Box Of The Object
        /// </summary>
        /// <returns>Object's Collision Box</returns>
        CollisionBox getBox();

        /// <summary>
        /// Get The Position Of Where The Box Is Located
        /// </summary>
        /// <returns>Base Point Of The Box</returns>
        Vector3 getBoxPosition();

        /// <summary>
        /// Tells If An Object's Face Is Available For Collision
        /// </summary>
        /// <param name="f">The Face (Between 0 - 6)</param>
        /// <returns>True If Face Can Be Collided</returns>
        bool isFaceSet(int f);
    }
}
