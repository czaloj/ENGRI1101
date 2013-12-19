using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ZLibrary.Operators;

using XNA3D.Objects.Data;
using XNA3D.Objects.Graphics;
using XNA3D.Cameras;
using XNA3D.ZDR;

namespace XNA3D.Objects
{
    public interface ITransformable
    {
        Transform Transform { get; set; }
    }
    public interface IOffsetted : ITransformable
    {
        Transform Offset { get; set; }

        ITransformable Parent { get; set; }
        bool HasParent { get; }
        void refreshFromParent();
    }

    public interface IPositionHolder { Vector3 Position { get; set; } }
    public interface IWorldTransformHolder { Matrix World { get; set; } }

    public class GameObject : ITransformable, IVisible<GameObject>
    {
        public event Action<GameObject, bool> OnVisibilityChange;
        protected bool isVisible;
        public virtual bool IsVisible
        {
            get
            {
                return isVisible;
            }
            set
            {
                //Only Change If There Is A Difference
                if (value != isVisible)
                {
                    isVisible = value;
                    if (OnVisibilityChange != null) { OnVisibilityChange(this, isVisible); }
                }
            }
        }

        protected Transform transform;
        public Transform Transform
        {
            get { return transform; }
            set { transform = value; }
        }

        public Matrix World
        {
            get { return transform.Matrix; }
            set { transform = new Transform(value); }
        }

        public Vector3 Position
        {
            get { return transform.Translation; }
            set { transform.Translation = value; }
        }
        public Quaternion Orientation
        {
            get { return transform.Rotation; }
        }
        public Vector3 Scale
        {
            get { return transform.Scaling; }
        }

        public Vector3 Forward
        {
            get { return transform.Forward; }
        }
        public Vector3 Right
        {
            get { return transform.Right; }
        }
        public Vector3 Up
        {
            get { return transform.Up; }
        }

        #region Initializers
        private void init()
        {
            transform = Transform.Identity;
        }
        public GameObject() : base() { init(); }
        #endregion

        public virtual void setPosition(Vector3 pos)
        {
            transform.Translation = pos;
        }

        public virtual void setOrientation(Vector3 pyr)
        {
            transform.Rotation = Quaternion.CreateFromYawPitchRoll(pyr.Y, pyr.X, pyr.Z);
        }
        public virtual void addPitch(float angle)
        {
            transform.rotateX(angle);
        }
        public virtual void addYaw(float angle)
        {
            transform.rotateY(angle);
        }
        public virtual void addRoll(float angle)
        {
            transform.rotateZ(angle);
        }
    }
}
