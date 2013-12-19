using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using ZLibrary.Operators;

using XNA3D.Objects;
using XNA3D.Objects.Graphics;
using XNA3D.Cameras;
using XNA3D.ZDR;
using XNA3D.DeferredRendering.Lights;

//TODO 

namespace XNA3D.Connected
{
    public delegate void OnLimbUpdate();
    public delegate void OnLimbDraw(GraphicsDevice g, ACCamera camera, Effect e);

    public class Body : PhysicalObject
    {
        protected Skeleton skeleton;
        protected Skeleton.BoneList mBoneList;

        protected Matrix[] mBones;

        protected VertexBuffer vb;
        protected IndexBuffer ib;
        public Body()
            : base()
        {

        }
        public Body(string skelFile, string animRestFile, string skinFile, GraphicsDevice g)
        {
            skeleton = new Skeleton(skelFile);
            Animation animRest = new Animation(animRestFile);
            mBones = new Matrix[skeleton.BoneCount];
            mBoneList = new Skeleton.BoneList(skeleton, animRest);
            Importer.getSkinnedMesh(skinFile, g, out vb, out ib);
        }
        public Body(Skeleton s, Animation rest, VertexBuffer vb, IndexBuffer ib)
        {
            skeleton = s;
            mBones = new Matrix[skeleton.BoneCount];
            mBoneList = new Skeleton.BoneList(s, rest);
            this.vb = vb;
            this.ib = ib;
        }
        public void build(ContentManager content, GraphicsDevice g)
        {
            //using (FileStream fs = File.Open(textureName, FileMode.Open))
            //{
            //    setModelTextures(Texture2D.FromStream(g, fs));
            //}
            //foreach (Limb limb in limbs)
            //{
            //    limb.build(content);
            //}
        }

        public override void update(float dt)
        {
            base.update(dt);
            //mBoneList.update(dt);
            //mBones = mBoneList.getMatrices(skeleton, World);
        }
        public void updateCustomTransforms(float dt, params Skeleton.CustomBone[] custom)
        {
            base.update(dt);
            //mBoneList.update(dt, custom);
            //mBones = mBoneList.getMatrices(skeleton, World);
        }

        public void addAnimationPlayer(Animation.Player a)
        {
            mBoneList.addAnimation(a);
        }
        public void removeAnimation(Animation a)
        {
            mBoneList.removeAnimation(a);
        }

        //public override void draw(GraphicsDevice g, ACCamera camera, Effect e)
        //{
        //    e.CurrentTechnique = e.Techniques["Skinned"];

        //    g.SetVertexBuffer(vb, 0);
        //    g.Indices = ib;

        //    e.Parameters["BoneMatrices"].SetValue(mBones);

        //    e.Parameters["Texture"].SetValue(texture);
        //    e.Parameters["NormalMap"].SetValue(normalTexture);
        //    e.Parameters["SpecularMap"].SetValue(specularTexture);

        //    e.CurrentTechnique.Passes[0].Apply();
        //    g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vb.VertexCount, 0, ib.IndexCount / 3);

        //    e.CurrentTechnique = e.Techniques["Default"];
        //}
        //public override void drawShadowDepth(GraphicsDevice g, ACCamera camera, Effect e)
        //{
        //    g.SetVertexBuffer(vb, 0);
        //    g.Indices = ib;

        //    e.Parameters["BoneMatrices"].SetValue(mBones);

        //    e.CurrentTechnique.Passes[LightEffect.PassShadowingSkinned].Apply();
        //    g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vb.VertexCount, 0, ib.IndexCount / 3);
        //}

        public void read(StreamReader s)
        {
        }
    }
}
