using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ZLibrary.Operators;
using Microsoft.Xna.Framework;
using XNA3D.Connected;

namespace XNA3D.ZDR
{
    public class Animation
    {
        public string Name;
        public long ID;
        protected int frameLength;
        public int TotalFrames
        {
            get
            {
                return frameLength;
            }
        }

        protected AnimationTimeline[] animations;
        public int BoneCount
        {
            get
            {
                return animations.Length;
            }
        }
        public AnimationTimeline this[int i]
        {
            get
            {
                return animations[i];
            }
        }

        public Animation(string file)
        {
            using (FileStream fs = File.Open(file, FileMode.Open))
            {
                using (StreamReader s = new StreamReader(fs))
                {
                    read(s);
                }
            }
        }

        public AnimationTimeline getTimeline(int i)
        {
            foreach (AnimationTimeline t in animations)
            {
                if (t.BoneIndex == i) { return t; }
            }
            return null;
        }

        public void read(StreamReader s)
        {
            // TODO: TokenStream
            //Token.HeaderValueBinding[] hAnimation = new Token.HeaderValueBinding[]
            //{
            //    new Token.HeaderValueBinding("Animation"),
            //    new Token.HeaderValueBinding("Frames"),
            //    new Token.HeaderValueBinding("Limbs")
            //};
            //Token[] tA;
            //if (!Token.readUntilAllHeaders(s, out tA, hAnimation))
            //{ throw new ArgumentException("Animation Header Could Not Be Read Or Found"); }

            ////Name
            //Name = tA[0].OriginalSplit[1];

            ////Frame Length
            //if (!tA[1].getArg<int>(2, ref frameLength))
            //{ throw new ArgumentException("Expected Integer Frame Length For Third Argument In Frames"); }

            ////Limb Count
            //int l = 0, f = 0;
            //if (!tA[2].getArg<int>(0, ref l))
            //{ throw new ArgumentException("Expected Integer Limb Count For Limbs"); }
            //animations = new AnimationTimeline[l];

            //Token.HeaderValueBinding[] hAnimationTimeline = new Token.HeaderValueBinding[]
            //{
            //    new Token.HeaderValueBinding("Limb"),
            //    new Token.HeaderValueBinding("Frames")
            //};
            //for (int i = 0; i < animations.Length; i++)
            //{
            //    if (!Token.readUntilAllHeaders(s, out tA, hAnimationTimeline))
            //    { throw new ArgumentException(string.Format("Animation Header {0} Could Not Be Read Or Found", i + 1)); }
            //    if (!tA[0].getArg<int>(0, ref l))
            //    { throw new ArgumentException("Expected Integer Limb Index For Limb"); }
            //    if (!tA[1].getArg<int>(0, ref f))
            //    { throw new ArgumentException("Expected Integer Timeline Frame Count For Frames"); }
            //    animations[i] = new AnimationTimeline(l, f);
            //    animations[i].read(s);
            //    animations[i].build(frameLength);
            //}
        }

        public class Player
        {
            Animation animation;
            AnimationTimeline.Player[] tlPlayers;

            bool loop;

            float duration;
            float fps;
            float time;

            float fTrue;
            int fCur;
            int fNext;
            float mRatio;

            float setRatio;

            public Player(Animation a, float fps = 30f, bool loop = true, float setRatio = 1f)
            {
                animation = a;
                tlPlayers = new AnimationTimeline.Player[animation.BoneCount];
                for (int i = 0; i < animation.BoneCount; i++)
                {
                    tlPlayers[i] = new AnimationTimeline.Player(animation[i]);
                }

                this.fps = fps;
                this.loop = loop;
                this.setRatio = setRatio;
                
                duration = animation.frameLength / this.fps;

                time = 0f;
                fTrue = 0f;
                fCur = 0;
                fNext = 0;
            }

            public void update(float dt)
            {
                //Update Elapsed Time
                time += dt;
                
                //Get True Frame And Check Looping
                fTrue = time * fps;
                if (fTrue >= animation.frameLength)
                {
                    if (loop)
                    {
                        time %= duration;
                        fTrue = time * fps;
                    }
                    else
                    {
                        return;
                    }
                }

                //Get Frame Indeces
                fCur = ZMath.fastFloor(fTrue);
                fNext = fCur + 1;

                //Update Timelines
                if (fNext < animation.frameLength)
                {
                    mRatio = fTrue - fCur;
                    foreach (AnimationTimeline.Player tlP in tlPlayers)
                    {
                        tlP.update(fCur, fNext, mRatio);
                    }
                }
                else
                {
                    foreach (AnimationTimeline.Player tlP in tlPlayers)
                    {
                        tlP.update(fCur);
                    }
                }
            }
            public void setToBoneList(Matrix[] mBones)
            {
                if (setRatio >= 1f)
                {
                    foreach (AnimationTimeline.Player p in tlPlayers)
                    {
                        mBones[p.Index] = p.mCur;
                    }
                }
                else if (setRatio <= 0f)
                {
                    return;
                }
                else
                {
                    foreach (AnimationTimeline.Player p in tlPlayers)
                    {
                        mBones[p.Index] = Matrix.Lerp(mBones[p.Index], p.mCur, setRatio);
                    }
                }
            }
            public void simpleSetToBoneList(Matrix[] mBones)
            {
                foreach (AnimationTimeline.Player p in tlPlayers)
                {
                    mBones[p.Index] = p.mCur;
                }
            }

            public bool usesAnimation(Animation a)
            {
                return animation == a;
            }
        }
    }
}
