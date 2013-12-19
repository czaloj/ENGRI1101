using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA3D.ZDR
{
    public class VCPositionNormalTexture : IVertexConverter<VertexPositionNormalTexture>
    {
        public static VCPositionNormalTexture Instance { get { return new VCPositionNormalTexture(); } }

        public const int ArgCount = 8;

        public void read(string[] vArgs, out VertexPositionNormalTexture v)
        {
            if (vArgs.Length < ArgCount) { throw new ArgumentException("Expecting 8 Args: Position, Normal, UV"); }
            Vector3 p, n;
            Vector2 uv;
            if (!float.TryParse(vArgs[0], out p.X) ||
                !float.TryParse(vArgs[1], out p.Y) ||
                !float.TryParse(vArgs[2], out p.Z) ||
                !float.TryParse(vArgs[3], out n.X) ||
                !float.TryParse(vArgs[4], out n.Y) ||
                !float.TryParse(vArgs[5], out n.Z) ||
                !float.TryParse(vArgs[6], out uv.X) ||
                !float.TryParse(vArgs[7], out uv.Y)
                )
            { throw new ArgumentException("Args Expected To Be Float"); }
            v = new VertexPositionNormalTexture(p, n, uv);
        }
        public void convert(VertexPositionNormalTexture v, out Token t)
        {
            t = Token.fromHeaderArgs("V",
                v.Position.X,
                v.Position.Y,
                v.Position.Z,
                v.Normal.X,
                v.Normal.Y,
                v.Normal.Z,
                v.TextureCoordinate.X,
                v.TextureCoordinate.Y
                );
        }
    }
    public class VCPositionColor : IVertexConverter<VertexPositionColor>
    {
        public static VCPositionColor Instance { get { return new VCPositionColor(); } }

        public const int ArgCount = 7;

        public void read(string[] vArgs, out VertexPositionColor v)
        {
            if (vArgs.Length < ArgCount) { throw new ArgumentException("Expecting 8 Args: Position, Normal, UV"); }
            Vector3 p;
            Vector4 c;
            if (!float.TryParse(vArgs[0], out p.X) ||
                !float.TryParse(vArgs[1], out p.Y) ||
                !float.TryParse(vArgs[2], out p.Z) ||
                !float.TryParse(vArgs[3], out c.X) ||
                !float.TryParse(vArgs[4], out c.Y) ||
                !float.TryParse(vArgs[5], out c.Z) ||
                !float.TryParse(vArgs[6], out c.W)
                )
            { throw new ArgumentException("Args Expected To Be Float"); }
            v = new VertexPositionColor(p, new Color(c));
        }
        public void convert(VertexPositionColor v, out Token t)
        {
            t = Token.fromHeaderArgs("V",
                v.Position.X,
                v.Position.Y,
                v.Position.Z,
                v.Color.R / 255f,
                v.Color.G / 255f,
                v.Color.B / 255f,
                v.Color.A / 255f
                );
        }
    }
}
