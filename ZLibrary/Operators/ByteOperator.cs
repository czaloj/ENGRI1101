using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZLibrary.Operators
{
    public static class ByteOperator
    {
        /// Bytes:
        /// 0000 - 0
        /// 0001 - 1
        /// 0010 - 2
        /// 0011 - 3
        /// 0100 - 4
        /// 0101 - 5
        /// 0110 - 6
        /// 0111 - 7
        /// 1000 - 8
        /// 1001 - 9
        /// 1010 - a
        /// 1011 - b
        /// 1100 - c
        /// 1101 - d
        /// 1110 - e
        /// 1111 - f

        #region Ands
        public static byte[] ands = new byte[]{
            0x7f,
            0xbf,
            0xdf,
            0xef,
            0xf7,
            0xfb,
            0xfd,
            0xfe
        };
        #endregion

        #region Ors
        public static byte[] ors = new byte[]{
            0x80,
            0x40,
            0x20,
            0x10,
            0x08,
            0x04,
            0x02,
            0x01
        };
        #endregion

        /// <summary>
        /// Set A Bit True In A Specified Byte
        /// </summary>
        /// <param name="b">The Byte</param>
        /// <param name="location">Location In Byte From Start (1 - 8)</param>
        /// <returns>The Modified Byte</returns>
        public static byte setBitTrue(byte b, int location)
        {
            b |= ors[location - 1];
            return b;
        }
        /// <summary>
        /// Set A Bit False In A Specified Byte
        /// </summary>
        /// <param name="b">The Byte</param>
        /// <param name="location">Location In Byte From Start (1 - 8)</param>
        /// <returns>The Modified Byte</returns>
        public static byte setBitFalse(byte b, int location)
        {
            b &= ands[location - 1];
            return b;
        }
        /// <summary>
        /// Checks If A Bit Is True In A Specified Byte
        /// </summary>
        /// <param name="b">The Byte</param>
        /// <param name="location">Location In Byte From Start (1 - 8)</param>
        /// <returns>True If Bit Is Set In That Location</returns>
        public static bool isBitTrue(byte b, int location)
        {
            b = (byte)(b << (location - 1));
            b |= ands[0];
            return b == 0xff;
        }
    }
}
