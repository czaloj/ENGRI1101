using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Microsoft.Xna.Framework
{
    public interface IDirectional
    {
        Vector3 Forward { get; }
        Vector3 Backward { get; }
        Vector3 Up { get; }
        Vector3 Down { get; }
        Vector3 Right { get; }
        Vector3 Left { get; }
    }
    public interface ITransform : IDirectional
    {
        Vector3 Translation { get; set; }
        Vector3 Scaling { get; set; }
        Quaternion Rotation { get; set; }
        Matrix Matrix { get; }

        void move(float x, float y, float z);
        void move(Vector3 t);

        void scale(float x, float y, float z);
        void scale(Vector3 s);

        void rotate(Quaternion q);
        void rotateX(float a);
        void rotateY(float a);
        void rotateZ(float a);
    }

    public class Transform : ITransform, ICloneable
    {
        #region Static
        private static readonly Transform identity = new Transform(Vector3.One, Quaternion.Identity, Vector3.Zero);
        public static Transform Identity { get { return new Transform(identity); } }

        public static bool operator ==(Transform t1, Transform t2)
        {
            return
                t1.scaling == t2.scaling &&
                t1.rotation == t2.rotation &&
                t1.translation == t2.translation
                ;
        }
        public static bool operator !=(Transform t1, Transform t2)
        {
            return
                t1.translation != t2.translation ||
                t1.rotation != t2.rotation ||
                t1.scaling != t2.scaling
                ;
        }
        public static Transform operator *(Transform t1, Transform t2)
        {
            return new Transform(
                t1.scaling * t2.scaling,
                t1.rotation * t2.rotation,
                Vector3.Transform(t1.translation * t2.scaling, t2.rotation) + t2.translation
                );
        }
        public static Transform inverse(Transform t)
        {
            Vector3 s = new Vector3(
                1f / t.scaling.X,
                1f / t.scaling.Y,
                1f / t.scaling.Z
                );
            Quaternion r = Quaternion.Inverse(t.rotation);
            return new Transform(
                s,
                r,
                -Vector3.Transform(t.translation * s, r)
                );
        }
        #endregion

        private Vector3 scaling;
        public Vector3 Scaling
        {
            get { return scaling; }
            set { scaling = value; needsUpdate = true; }
        }

        private Quaternion rotation;
        public Quaternion Rotation
        {
            get { return rotation; }
            set { rotation = value; needsUpdate = true; needsDirUpdate = true; }
        }
        private bool needsDirUpdate;
        private Vector3 lUnitZ, lUnitX, lUnitY;
        public Vector3 Forward
        {
            get
            {
                return -Backward;
            }
        }
        public Vector3 Backward
        {
            get
            {
                updateDir();
                return lUnitZ;
            }
        }
        public Vector3 Right
        {
            get
            {
                updateDir();
                return lUnitX;
            }
        }
        public Vector3 Left
        {
            get { return -Right; }
        }
        public Vector3 Up
        {
            get
            {
                updateDir();
                return lUnitY;
            }
        }
        public Vector3 Down
        {
            get { return -Up; }
        }

        private Vector3 translation;
        public Vector3 Translation
        {
            get { return translation; }
            set
            {
                translation = value;
                needsUpdate = true;
            }
        }

        private bool needsUpdate;
        private Matrix composition;
        public Matrix Matrix
        {
            get
            {
                updateComposite();
                return composition;
            }
        }

        public Transform(Matrix c)
        {
            c.Decompose(out scaling, out rotation, out translation);
            composition = c;
            needsUpdate = false;
            needsDirUpdate = true;
        }
        public Transform(Vector3 s, Quaternion r, Vector3 t)
        {
            scaling = s;
            rotation = r;
            translation = t;
            needsUpdate = true;
            needsDirUpdate = true;
        }
        public Transform(Transform tr)
            : this(tr.scaling, tr.rotation, tr.translation)
        {
        }

        private void updateDir()
        {
            if (needsDirUpdate)
            {
                lUnitX = Vector3.Transform(Vector3.UnitX, rotation);
                lUnitY = Vector3.Transform(Vector3.UnitY, rotation);
                lUnitZ = Vector3.Transform(Vector3.UnitZ, rotation);
                needsDirUpdate = false;
            }
        }
        private void updateComposite()
        {
            if (needsUpdate)
            {
                composition =
                    Matrix.CreateScale(scaling) *
                    Matrix.CreateFromQuaternion(rotation) *
                    Matrix.CreateTranslation(translation)
                    ;
                needsUpdate = false;
            }
        }

        #region Scaling
        public void scale(float x, float y, float z)
        {
            scaling.X *= x;
            scaling.Y *= y;
            scaling.Z *= z;
            needsUpdate = true;
        }
        public void scale(Vector3 s)
        {
            scaling *= s;
            needsUpdate = true;
        }
        public void scaleAbsolute(float x, float y, float z)
        {
            translation.X *= x;
            translation.Y *= y;
            translation.Z *= z;
            scaling.X *= x;
            scaling.Y *= y;
            scaling.Z *= z;
            needsUpdate = true;
        }
        public void scaleAbsolute(Vector3 s)
        {
            translation *= s;
            scaling *= s;
            needsUpdate = true;
        } 
        #endregion

        #region Rotation
        public void rotate(Quaternion q)
        { Rotation = q * rotation; }
        public void rotateX(float a)
        {
            a /= 2f;
            rotate(new Quaternion((float)Math.Sin(a), 0, 0, (float)Math.Cos(a)));
        }
        public void rotateY(float a)
        {
            a /= 2f;
            rotate(new Quaternion(0, (float)Math.Sin(a), 0, (float)Math.Cos(a)));
        }
        public void rotateZ(float a)
        {
            a /= 2f;
            rotate(new Quaternion(0, 0, (float)Math.Sin(a), (float)Math.Cos(a)));
        }
        public void rotateAbsolute(Quaternion q)
        {
            Vector3 t = translation;
            Vector3.Transform(ref t, ref q, out translation);
            Rotation *= q;
        }
        public void rotateXAbsolute(float a)
        {
            a /= 2f;
            rotateAbsolute(new Quaternion((float)Math.Sin(a), 0, 0, (float)Math.Cos(a)));
        }
        public void rotateYAbsolute(float a)
        {
            a /= 2f;
            rotateAbsolute(new Quaternion(0, (float)Math.Sin(a), 0, (float)Math.Cos(a)));
        }
        public void rotateZAbsolute(float a)
        {
            a /= 2f;
            rotateAbsolute(new Quaternion(0, 0, (float)Math.Sin(a), (float)Math.Cos(a)));
        } 
        #endregion

        #region Movement
        public void move(float x, float y, float z)
        {
            updateDir();
            translation +=
                x * Right +
                y * Up +
                z * Backward
                ;
            needsUpdate = true;
        }
        public void move(Vector3 t)
        {
            updateDir();
            translation +=
                t.X * Right +
                t.Y * Up +
                t.Z * Backward
                ;
            needsUpdate = true;
        }
        public void moveAbsolute(float x, float y, float z)
        {
            translation.X += x;
            translation.Y += y;
            translation.Z += z;
            needsUpdate = true;
        }
        public void moveAbsolute(Vector3 t)
        {
            translation += t;
            needsUpdate = true;
        } 
        #endregion

        public void addInertia(Transform o, float p)
        {
            rotate(Quaternion.Slerp(Quaternion.Identity, o.rotation, p));
            move(o.translation * p);
        }
        public void multiply(Transform o)
        {
            //Scale All First
            scaling *= o.scaling;
            translation *= o.scaling;

            //Rotate
            Vector3 t = translation;
            Vector3.Transform(ref t, ref o.rotation, out translation);
            rotation *= o.rotation;
            needsDirUpdate = true;

            //Translate
            translation += o.translation;
            needsUpdate = true;
        }
        public void invert()
        {
            //Scale Back To One
            scaling.X = 1f / scaling.X;
            scaling.Y = 1f / scaling.Y;
            scaling.Z = 1f / scaling.Z;

            //Inverse The Rotation
            Quaternion ir = rotation;
            Quaternion.Inverse(ref ir, out rotation);
            needsDirUpdate = true;

            //Scale/Rotate Translation
            Vector3 t = translation * scaling;
            Vector3.Transform(ref t, ref rotation, out translation);
            //Negate For Going Back To Origin
            translation = -translation;
            needsUpdate = true;
        }

        #region Overriding Functions
        /// <summary>
        /// Perform A Reference Check On The Objects
        /// </summary>
        /// <param name="obj">The Object To Test Against</param>
        /// <returns>True If Same Object</returns>
        public override bool Equals(object obj)
        {
            return object.ReferenceEquals(this, obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public object Clone()
        {
            return new Transform(this);
        }
        public Transform DeepClone()
        {
            return new Transform(this);
        }

        public override string ToString()
        {
            return string.Format("T[{0}] R[{1}] S[{2}]",
                Translation,
                rotation,
                scaling
                );
        }
        #endregion
    } 
}
