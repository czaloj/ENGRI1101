using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using XNA3D.ZDR;

namespace Microsoft.Xna.Framework.Graphics
{
    public struct VertexDeferred : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector3 Tangent;
        public Vector3 Binormal;

        public VertexDeferred(Vector3 position, Vector3 normal, Vector2 textureCoordinate, Vector3 tangent)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
            Tangent = tangent;
            Binormal = Vector3.Cross(Tangent, Normal);
        }
        public VertexDeferred(Vector3 position, Vector3 normal, Vector2 textureCoordinate, Vector3 tangent, Vector3 binormal)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
            Tangent = tangent;
            Binormal = binormal;
        }

        public override String ToString()
        {
            return "(" + Position + "),(" + Normal + "),(" + TextureCoordinate + ")";
        }

        //Vertex Information
        public const int SizeInBytes = 14;
        public static readonly VertexDeclaration Declaration = new VertexDeclaration(
            new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
            new VertexElement(sizeof(float) * 11, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0)
            );
        public VertexDeclaration VertexDeclaration
        {
            get { return Declaration; }
        }
    }
    public class VCDeferred : IVertexConverter<VertexDeferred>, IVertexCalculator<VertexDeferred>
    {
        public static VCDeferred Instance { get { return new VCDeferred(); } }

        public const int ArgCount = 8;

        public void read(string[] vArgs, out VertexDeferred v)
        {
            if (vArgs.Length < ArgCount) { throw new ArgumentException("Expecting 8 Args: Position, Normal, UV"); }
            Vector3 p;
            Vector3 n;
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
            v = new VertexDeferred(p, n, uv, Vector3.Zero, Vector3.Zero);
        }
        public void convert(VertexDeferred v, out Token t)
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

        public void calculateTriangle(ref VertexDeferred v1, ref VertexDeferred v2, ref VertexDeferred v3)
        {
            Vector3 t, b;
            Vector3Ext.calculateTanBin(
                new Vector3[] { v1.Position, v2.Position, v3.Position },
                new Vector2[] { v1.TextureCoordinate, v2.TextureCoordinate, v3.TextureCoordinate },
                out t, out b
            );
            v1.Tangent = t; v1.Binormal = b;
            v2.Tangent = t; v2.Binormal = b;
            v3.Tangent = t; v3.Binormal = b;
        }
    }

    public struct VertexDeferredSkinned : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector3 Tangent;
        public Vector3 Binormal;
        public float BoneIndex;

        public VertexDeferredSkinned(Vector3 position, Vector3 normal, Vector2 textureCoordinate, Vector3 tangent, int bone)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
            Tangent = tangent;
            Binormal = Vector3.Cross(Tangent, Normal);
            BoneIndex = bone;
        }
        public VertexDeferredSkinned(Vector3 position, Vector3 normal, Vector2 textureCoordinate, Vector3 tangent, Vector3 binormal, int bone)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
            Tangent = tangent;
            Binormal = binormal;
            BoneIndex = bone;
        }

        public override String ToString()
        {
            return "(" + Position + "),(" + Normal + "),(" + TextureCoordinate + "),(" + BoneIndex + ")";
        }

        //Vertex Information
        public const int SizeInBytes = 15;
        public static VertexDeclaration Declaration = new VertexDeclaration(
            new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
            new VertexElement(sizeof(float) * 11, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
            new VertexElement(sizeof(float) * 14, VertexElementFormat.Single, VertexElementUsage.BlendIndices, 0)
            );
        public VertexDeclaration VertexDeclaration
        {
            get { return Declaration; }
        }
    }
    public class VCDeferredSkinned : IVertexConverter<VertexDeferredSkinned>, IVertexCalculator<VertexDeferredSkinned>
    {
        public static VCDeferredSkinned Instance { get { return new VCDeferredSkinned(); } }

        public const int ArgCount = 9;

        public void read(string[] vArgs, out VertexDeferredSkinned v)
        {
            if (vArgs.Length < ArgCount) { throw new ArgumentException("Expecting 9 Args: Position, Normal, UV, BoneIndex"); }
            Vector3 p;
            Vector3 n;
            Vector2 uv;
            int bi;
            if (!float.TryParse(vArgs[0], out p.X) ||
                !float.TryParse(vArgs[1], out p.Y) ||
                !float.TryParse(vArgs[2], out p.Z) ||
                !float.TryParse(vArgs[3], out n.X) ||
                !float.TryParse(vArgs[4], out n.Y) ||
                !float.TryParse(vArgs[5], out n.Z) ||
                !float.TryParse(vArgs[6], out uv.X) ||
                !float.TryParse(vArgs[7], out uv.Y) ||
                !int.TryParse(vArgs[8], out bi)
                )
            { throw new ArgumentException("Args Expected To Be Float"); }
            v = new VertexDeferredSkinned(p, n, uv, Vector3.Zero, Vector3.Zero, bi);
        }
        public void convert(VertexDeferredSkinned v, out Token t)
        {
            t = Token.fromHeaderArgs("V",
                v.Position.X,
                v.Position.Y,
                v.Position.Z,
                v.Normal.X,
                v.Normal.Y,
                v.Normal.Z,
                v.TextureCoordinate.X,
                v.TextureCoordinate.Y,
                v.BoneIndex
                );
        }

        public void calculateTriangle(ref VertexDeferredSkinned v1, ref VertexDeferredSkinned v2, ref VertexDeferredSkinned v3)
        {
            Vector3 t, b;
            Vector3Ext.calculateTanBin(
                new Vector3[] { v1.Position, v2.Position, v3.Position },
                new Vector2[] { v1.TextureCoordinate, v2.TextureCoordinate, v3.TextureCoordinate },
                out t, out b
            );
            v1.Tangent = t; v1.Binormal = b;
            v2.Tangent = t; v2.Binormal = b;
            v3.Tangent = t; v3.Binormal = b;
        }
    }
}
