using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNA3D
{
    /// <summary>
    /// Simple Cardinal Directions
    /// </summary>
    public enum XYZ_DIRECTION : int
    {
        //One Addition
        XP = 0,
        XN = 1,
        YP = 2,
        YN = 3,
        ZP = 4,
        ZN = 5,

        //Two Additions
        XPYP = 6,
        XPYN = 7,
        XNYP = 8,
        XNYN = 9,
        ZPYP = 10,
        ZPYN = 11,
        ZNYP = 12,
        ZNYN = 13,
        XPZP = 14,
        XPZN = 15,
        XNZP = 16,
        XNZN = 17,

        //Three Additions
        XPYPZP = 18,
        XPYPZN = 19,
        XPYNZP = 20,
        XPYNZN = 21,
        XNYPZP = 22,
        XNYPZN = 23,
        XNYNZP = 24,
        XNYNZN = 25
    };
}
