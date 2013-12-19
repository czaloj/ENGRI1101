using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using XNA3D.Connected;

namespace XNA3D.ZDR
{
    public class AnimationTimeline
    {
        protected int limbIndex;
        public int BoneIndex
        {
            get
            {
                return limbIndex;
            }
        }

        protected Frame[] frames;
        public int KeyFrameCount
        {
            get
            {
                return frames.Length;
            }
        }

        protected int totalFrames;
        protected Matrix[] mLine;

        public AnimationTimeline(int limb, int keyframeCount)
        {
            limbIndex = limb;
            frames = new Frame[keyframeCount];
        }
        public void setLimb(int index)
        {
            limbIndex = index;
        }

        public void read(StreamReader s)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = new Frame(i);
                frames[i].read(s);
            }
            for (int i = 1; i < frames.Length; i++)
            {
                frames[i - 1].setNextFrame(frames[i]);
                frames[i].setPreviousFrame(frames[i - 1]);
            }
        }

        public void build(int totalFrames)
        {
            this.totalFrames = totalFrames;
            mLine = new Matrix[this.totalFrames];
            Frame cur, next;
            cur = frames[0];
            next = cur.NextFrame;
            int f = 0;
            float lr = 0f;

            while (f < cur.KeyFrameIndex)
            {
                mLine[f] = cur.OffM3.World;
                f++;
            }
            while (next != null)
            {
                lr = (float)(f - cur.KeyFrameIndex) / (float)(next.KeyFrameIndex - cur.KeyFrameIndex);
                mLine[f] = Frame.lerp(cur, next, lr).World;
                f++;

                //Check For KeyFrame Advance
                if (f > next.KeyFrameIndex)
                {
                    cur = cur.NextFrame;
                    next = next.NextFrame;
                }
            }
            while (f < this.totalFrames)
            {
                mLine[f] = cur.OffM3.World;
                f++;
            }
        }

        public class Frame
        {
            public Vector3 Scale;
            public Vector3 Rotation_XYZ;
            public Vector3 Translation;
            public Quaternion Quaternion;

            public M3Info OffM3
            {
                get
                {
                    return new M3Info(
                        Matrix.CreateScale(Scale),
                        Matrix.CreateFromQuaternion(Quaternion),
                        Matrix.CreateTranslation(Translation)
                        );
                }
            }
            public Matrix mOffset;

            public int KeyFrameIndex;
            public int TimelineIndex;
            public Frame NextFrame;
            public Frame PreviousFrame;

            public Frame(int tI)
            {
                TimelineIndex = tI;
            }

            public void setNextFrame(Frame f)
            {
                NextFrame = f;
            }
            public void setPreviousFrame(Frame f)
            {
                PreviousFrame = f;
            }

            public void read(StreamReader s)
            {
                Token.HeaderValueBinding[] hFrame = new Token.HeaderValueBinding[]
            {
                new Token.HeaderValueBinding("Frame"),
                new Token.HeaderValueBinding("Scale"),
                new Token.HeaderValueBinding("Rot"),
                new Token.HeaderValueBinding("Trans")
            };
                // TODO: TokenStream
                //Token[] tA;

                //if (!Token.readUntilAllHeaders(s, out tA, hFrame))
                //{ throw new ArgumentException(string.Format("Could Not Read Whole New Frame")); }

                //if (!tA[0].getArg<int>(0, ref KeyFrameIndex))
                //{ throw new ArgumentException("Expected Integer Key Frame Index For Frame"); }

                //Vector3[] v = new Vector3[3];
                //for (int vi = 0; vi < 3; vi++)
                //{
                //    if (!tA[vi + 1].getArgVector3(0, ref v[vi]))
                //    { throw new ArgumentException(string.Format("Expected Vector3 For Row {0} in Frame", vi, KeyFrameIndex)); }
                //}
                //Scale = v[0];
                //Rotation_XYZ = v[1];
                //Translation = v[2];

                ////Create Matrices
                //Quaternion = Quaternion.CreateFromRotationMatrix(
                //    Matrix.CreateRotationX(Rotation_XYZ.X) *
                //    Matrix.CreateRotationY(Rotation_XYZ.Y) *
                //    Matrix.CreateRotationZ(Rotation_XYZ.Z)
                //    );
                //mOffset =
                //    Matrix.CreateScale(Scale) *
                //    Matrix.CreateRotationX(Rotation_XYZ.X) *
                //    Matrix.CreateRotationY(Rotation_XYZ.Y) *
                //    Matrix.CreateRotationZ(Rotation_XYZ.Z) *
                //    Matrix.CreateTranslation(Translation)
                //    ;
            }

            public static M3Info lerp(Frame f1, Frame f2, float r)
            {
                return new M3Info(
                    Matrix.CreateScale(Vector3.Lerp(f1.Scale, f2.Scale, r)),
                    Matrix.CreateFromQuaternion(Quaternion.Slerp(f1.Quaternion, f2.Quaternion, r)),
                    Matrix.CreateTranslation(Vector3.Lerp(f1.Translation, f2.Translation, r))
                    );
            }
        }
        public class Player
        {
            public AnimationTimeline timeline;
            public int Index
            {
                get
                {
                    return timeline.limbIndex;
                }
            }
            public Matrix mCur;

            public Player(AnimationTimeline atl)
            {
                timeline = atl;
            }

            public void update(int frame)
            {
                mCur = timeline.mLine[frame];
            }
            public void update(int f1, int f2, float r)
            {
                mCur = Matrix.Lerp(timeline.mLine[f1], timeline.mLine[f2], r);
            }
        }
    }
}
