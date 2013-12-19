using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA3D.Objects
{
    public interface IObjectLinker : IOffsetted, IEnumerable<IOffsetted>
    {
        int ChildrenCount { get; }
        IEnumerable<IOffsetted> Children { get; }

        void addChild(IOffsetted o);
        bool removeChild(IOffsetted o);
    }

    public class ObjectLinker : IObjectLinker
    {
#if OLD
        protected Matrix mOff, mWorld;
        public Matrix Offset
        {
            get { return mOff; }
            set { mOff = value; refreshFromParent(); }
        }
        public Matrix World
        {
            get { return mWorld; }
            set
            {
                mWorld = value;
                foreach (IOffsetted child in children)
                { child.World = child.Offset * mWorld; }
            }
        }
        protected Vector3 scaling;
        public Vector3 Scaling
        {
            get { return scaling; }
        }
        public Vector3 Forward
        {
            get { return World.Forward; }
        }
        public Vector3 Up
        {
            get { return World.Up; }
        }
        public Vector3 Right
        {
            get { return World.Right; }
        } 
#endif
        protected Transform tOffset;
        public Transform Offset
        {
            get { return tOffset; }
            set { tOffset = value; refreshFromParent(); }
        }

        protected Transform transform;
        public Transform Transform
        {
            get { return transform; }
            set
            {
                transform = value;
                foreach (IOffsetted child in children)
                { child.Transform = child.Offset * Transform; }
            }
        }

        public LinkedList<IOffsetted> children;
        public int ChildrenCount
        {
            get { return children.Count; }
        }
        #region Enumerate Children
        public IEnumerator<IOffsetted> GetEnumerator()
        {
            foreach (IOffsetted o in children) { yield return o; }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (IOffsetted o in children) { yield return o; }
        }
        public IEnumerable<IOffsetted> Children
        {
            get { foreach (IOffsetted o in children) { yield return o; } }
        }
        #endregion

        protected ITransformable parent;
        public ITransformable Parent
        {
            get { return parent; }
            set { parent = value; refreshFromParent(); }
        }
        public bool HasParent
        {
            get { return parent != null; }
        }

        public ObjectLinker()
        {
            parent = null;
            tOffset = Transform.Identity;
            transform = Transform.Identity;

            children = new LinkedList<IOffsetted>();
        }
        public ObjectLinker(params IOffsetted[] c)
        {
            parent = null;
            children = new LinkedList<IOffsetted>(c);
        }

        public void addChild(IOffsetted o)
        {
            if (o == null) { throw new NullReferenceException("Null Child Attempted To Be Added"); }
            children.AddLast(o);
        }
        public bool removeChild(IOffsetted o)
        {
            if (o == null) { throw new NullReferenceException("Null Child Attempted To Be Removed"); }
            return children.Remove(o);
        }

        public void refreshFromParent()
        {
            if (HasParent)
            {
                transform = tOffset * parent.Transform;
            }
        }

#if OLD
        #region Local Transformation
        public void move(float x, float y, float z)
        { World *= Matrix.CreateTranslation(x, y, z); }
        public void move(Vector3 t) { move(t.X, t.Y, t.Z); }

        public void scale(float x, float y, float z)
        {
            World = Matrix.CreateScale(x, y, z) * World;
            scaling.X *= x;
            scaling.Y *= y;
            scaling.Z *= z;
        }
        public void scale(Vector3 s) { scale(s.X, s.Y, s.Z); }
        public void scale(float f) { scale(f, f, f); }

        public void rotate(Matrix m)
        { World = m * World; }
        public void pitch(float a) { rotate(Matrix.CreateRotationX(a)); }
        public void yaw(float a) { rotate(Matrix.CreateRotationY(a)); }
        public void roll(float a) { rotate(Matrix.CreateRotationZ(a)); }
        #endregion 
#endif
    }
}
