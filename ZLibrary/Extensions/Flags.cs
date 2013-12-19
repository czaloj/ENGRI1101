using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class Flags
    {
        public const byte NoFlags = 0x00;
        public const byte AllFlags = 0xff;

        public const byte Bit1 = 0x01;
        public const byte Bit2 = 0x02;
        public const byte Bit3 = 0x04;
        public const byte Bit4 = 0x08;
        public const byte Bit5 = 0x10;
        public const byte Bit6 = 0x20;
        public const byte Bit7 = 0x40;
        public const byte Bit8 = 0x80;
        public const byte TotalBits = 8;

        public const byte InvBit1 = unchecked((byte)~Bit1);
        public const byte InvBit2 = unchecked((byte)~Bit2);
        public const byte InvBit3 = unchecked((byte)~Bit3);
        public const byte InvBit4 = unchecked((byte)~Bit4);
        public const byte InvBit5 = unchecked((byte)~Bit5);
        public const byte InvBit6 = unchecked((byte)~Bit6);
        public const byte InvBit7 = unchecked((byte)~Bit7);
        public const byte InvBit8 = unchecked((byte)~Bit8);
        public static bool hasFlags(this byte b, byte bitflags)
        {
            return (b & bitflags) == bitflags;
        }
        public static bool hasSomeFlags(this byte b, byte bitflags)
        {
            return (b & bitflags) != 0x00;
        }

        public static void addFlags(ref byte b, byte bitflags)
        {
            unchecked
            {
                b |= bitflags;
            }
        }
        public static void removeFlags(ref byte b, byte bitflags)
        {
            unchecked
            {
                b &= (byte)~bitflags;
            }
        }
        public static void removeFlagsByInv(ref byte b, byte invbitflags)
        {
            b &= invbitflags;
        }
        public static void toggleFlags(ref byte b, byte bitflags)
        {
            unchecked
            {
                b ^= bitflags;
            }
        }
    }
}
