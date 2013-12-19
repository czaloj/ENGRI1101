using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using XNA3D.ZDR;

namespace XNA3D.Connected
{
    public class Skeleton
    {
        public string Name;
        protected Bone[] bones;
        protected Bone rootBone;
        public int BoneCount
        {
            get
            {
                return bones.Length;
            }
        }

        public Skeleton(string file)
        {
            using (FileStream fs = File.Open(file, FileMode.Open))
            {
                using (StreamReader s = new StreamReader(fs))
                {
                    read(s);
                }
            }
        }

        public Bone getLimb(int index)
        {
            foreach (Bone bone in bones)
            {
                if (bone.Index == index) { return bone; }
            }
            return null;
        }
        public Bone getLimb(string name)
        {
            foreach (Bone bone in bones)
            {
                if (bone.Name.Equals(name)) { return bone; }
            }
            return null;
        }

        public void read(StreamReader s)
        {
            // TODO: TokenStream




            //Token.HeaderValueBinding[] hSkel = new Token.HeaderValueBinding[]
            //{
            //    new Token.HeaderValueBinding("Skeleton"),
            //    new Token.HeaderValueBinding("Limbs")
            //};
            //Token[] tA;
            //if (!Token.readUntilAllHeaders(s, out tA, hSkel))
            //{ throw new ArgumentException("Could Not Read Skeleton Header"); }
            //Name = tA[0].OriginalSplit[1];
            //int c = 0;
            //if (!tA[1].getArg<int>(0, ref c))
            //{ throw new ArgumentException("Expected Integer Count For Bones"); }
            //bones = new Bone[c];
            //Bone.Info[] info = new Bone.Info[c];
            //for (int i = 0; i < c; i++)
            //{
            //    info[i] = Bone.Info.read(s);
            //    bones[i] = new Bone(info[i]);
            //}
            //buildParenting(info);
            //for (int i = 0; i < c; i++)
            //{
            //    bones[i].build(info[i], this);
            //}
        }
        public void buildParenting(Bone.Info[] info)
        {
            int p;
            int[] pc = new int[info.Length];
            for (int i = 0; i < info.Length; i++)
            {
                p = info[i].ParentIndex;
                if (p != -1)
                {
                    info[p].ChildrenIndex[pc[p]] = info[i].Index;
                    pc[p]++;
                }
            }
        }

        public class Bone
        {
            public string Name;
            public int Index;

            public Info info;

            public Matrix mRest;
            public Matrix mInvRest;
            public Matrix mOffset;//Set From Animation
            public Matrix mWorld;//Built From Offset And Parents' World

            public Bone parent;
            public Bone[] children;

            public Bone(Info info)
            {
                Name = info.Name;
                Index = info.Index;
                this.info = info;

                mOffset = info.mOffset;
            }
            public void build(Info info, Skeleton s)
            {
                if (info.ParentIndex != -1)
                {
                    parent = s.bones[info.ParentIndex];
                }
                if (info.ChildrenIndex != null)
                {
                    children = new Bone[info.ChildrenIndex.Length];
                    for (int i = 0; i < children.Length; i++)
                    {
                        children[i] = s.bones[info.ChildrenIndex[i]];
                    }
                }

                if (parent != null)
                {
                    mRest = info.mOffset * parent.mRest;
                }
                else
                {
                    s.rootBone = this;
                    mRest = info.mOffset;
                }
                mInvRest = Matrix.Invert(mRest);
                mWorld = mRest;
            }

            public void calcWorld()
            {
                mWorld = mOffset * parent.mWorld;
                if (children != null)
                {
                    foreach (Bone child in children)
                    {
                        child.calcWorld();
                    }
                }
            }

            public struct Info
            {
                public static Token.HeaderValueBinding[] hInfo;
                static Info()
                {
                    hInfo = new Token.HeaderValueBinding[]
                    {
                        new Token.HeaderValueBinding("Limb"),
                        new Token.HeaderValueBinding("Parent"),
                        new Token.HeaderValueBinding("Children"),
                        new Token.HeaderValueBinding("Scale"),
                        new Token.HeaderValueBinding("Rot"),
                        new Token.HeaderValueBinding("Trans")
                    };
                }

                public string Name;
                public int Index;

                public int ParentIndex;
                public int[] ChildrenIndex;

                public Vector3 Scale;
                public Vector3 Rotation_XYZ;
                public Vector3 Translation;

                public Matrix mOffset;

                public Info(string name, int index, int parent, int cCount,
                    Vector3[] vInfo)
                {
                    Name = name;
                    Index = index;
                    ParentIndex = parent;
                    if (cCount > 0)
                    {
                        ChildrenIndex = new int[cCount];
                    }
                    else
                    {
                        ChildrenIndex = null;
                    }

                    Scale = vInfo[0];
                    Rotation_XYZ = vInfo[1];
                    Translation = vInfo[2];
                    mOffset = 
                        Matrix.CreateScale(Scale) *
                        Matrix.CreateRotationX(Rotation_XYZ.X) *
                        Matrix.CreateRotationY(Rotation_XYZ.X) *
                        Matrix.CreateRotationZ(Rotation_XYZ.Z) *
                        Matrix.CreateTranslation(Translation);
                    ;
                }
                public static Info read(StreamReader s)
                {
                    // TODO: TokenStream
                    return new Info();
                    //string n = null;
                    //int i = 0, p = 0, cc = 0;
                    //Vector4[] m = new Vector4[4];
                    //Token[] tA;
                    //if (!Token.readUntilAllHeaders(s, out tA, hInfo))
                    //{ throw new ArgumentException("Could Not Read Bone - Missing/Incorrect Information"); }
                    //if (!tA[0].getArg<int>(0, ref i))
                    //{ throw new ArgumentException("Expected Integer Limb Index For Limb"); }
                    //if (!tA[1].getArg<int>(0, ref p))
                    //{ throw new ArgumentException("Expected Integer Limb Index For Parent"); }
                    //if (!tA[2].getArg<int>(0, ref cc))
                    //{ throw new ArgumentException("Expected Integer Count For Children"); }
                    //Vector3[] v = new Vector3[3];
                    //for (int vi = 0; vi < 3; vi++)
                    //{
                    //    if (!tA[vi + 3].getArgVector3(0, ref v[vi]))
                    //    { throw new ArgumentException(string.Format("Expected Vector3 For Row {0}", vi)); }
                    //}
                    //return new Info(n, i, p, cc, v);
                }
            }
        }

        public class BoneList
        {
            public Matrix[] mBones;
            protected Animation rest;
            public Animation.Player plRest;

            public LinkedList<Animation.Player> plAnims;

            public int BoneCount
            {
                get
                {
                    return mBones.Length;
                }
            }

            public BoneList(Skeleton s, Animation rest)
            {
                if (rest.BoneCount != s.BoneCount)
                { throw new ArgumentException(string.Format("Rest Animation Has Incorrect Bone Count {0}[Anim] != {1}[Skeleton]", rest.BoneCount, s.BoneCount)); }
                this.rest = rest;
                mBones = new Matrix[s.BoneCount];

                plRest = new Animation.Player(this.rest);
                plRest.update(0f);
                plRest.simpleSetToBoneList(mBones);
                plAnims = new LinkedList<Animation.Player>();
            }

            public void addAnimation(Animation.Player pl)
            {
                plAnims.AddLast(pl);
            }
            public void removeAnimation(Animation a)
            {
                LinkedListNode<Animation.Player> node = plAnims.First;
                while (node != null)
                {
                    if (node.Value.usesAnimation(a))
                    {
                        plAnims.Remove(node);
                        return;
                    }
                    node = node.Next;
                }
            }

            public void update(float dt, params CustomBone[] custom)
            {
                plRest.simpleSetToBoneList(mBones);
                if (custom != null)
                {
                    foreach (CustomBone cb in custom)
                    {
                        mBones[cb.Index] = mBones[cb.Index];
                    }
                }
                foreach (Animation.Player pl in plAnims)
                {
                    pl.update(dt);
                    pl.setToBoneList(mBones);
                }
            }

            public Matrix[] getMatrices(Skeleton s, Matrix mRootTransform)
            {
                Matrix[] m = new Matrix[BoneCount];
                Bone b = s.rootBone;
                m[b.Index] = mBones[b.Index] * mRootTransform;
                if (b.children != null)
                {
                    foreach (Bone c in b.children)
                    {
                        getMatrices(c, m);
                    }
                }
                for (int i = 0; i < BoneCount; i++)
                {
                    m[i] = s.bones[i].mInvRest * m[i];
                }
                return m;
            }
            void getMatrices(Bone b, Matrix[] m)
            {
                m[b.Index] = mBones[b.Index] * m[b.parent.Index];
                if (b.children != null)
                {
                    foreach (Bone c in b.children)
                    {
                        getMatrices(c, m);
                    }
                }
            }
        }

        public struct CustomBone
        {
            public int Index;
            public Matrix mOffset;

            public CustomBone(int i, Matrix m)
            {
                Index = i;
                mOffset = m;
            }
        }
    }
}