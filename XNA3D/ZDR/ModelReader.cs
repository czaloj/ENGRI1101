using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA3D.ZDR
{
    public static class ModelReader
    {
        public struct VertexData
        {
            public const byte InfoPosition = 0x01;
            public const byte InfoNormal = 0x02;
            public const byte InfoUV = 0x04;
            public const byte InfoBoneBind = 0x08;
            public const byte InfoColor = 0x10;
            public const byte InfoTanBin = 0x20;

            public byte InfoType;

            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 UV;
            public int BoneBind;
            public Vector4 Color;
            public Vector3 Tangent;
            public Vector3 Binormal;
        }
    }
}
