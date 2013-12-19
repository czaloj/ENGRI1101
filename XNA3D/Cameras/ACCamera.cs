using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNA3D.Cameras
{
    public delegate void OnViewChange(Matrix view);
    public delegate void OnProjectionChange(Matrix projection);

    public interface ICamera
    {
        Matrix View { get; }
        Matrix Projection { get; }
        Vector3 Location { get; }
    }

    public class ACCamera : ICamera
    {
        protected Matrix projection;
        public Matrix Projection
        {
            get
            {
                return projection;
            }
        }
        protected float fieldOfView;
        public float FieldOfView
        {
            get
            {
                return fieldOfView;
            }
            set
            {
                setFieldOfView(value);
            }
        }
        protected float nearClip;
        public float NearClip
        {
            get
            {
                return nearClip;
            }
            set
            {
                setClipDistances(value, farClip);
            }
        }
        protected float farClip;
        public float FarClip
        {
            get
            {
                return farClip;
            }
            set
            {
                setClipDistances(nearClip, value);
            }
        }
        protected float aspectRatio;

        protected Matrix view;
        public Matrix View
        {
            get
            {
                return view;
            }
        }

        protected Vector3 location;
        public Vector3 Location
        {
            get
            {
                return location;
            }
        }

        protected Matrix rotation;
        protected Vector3 forward;
        public Vector3 Forward
        {
            get { return forward; }
        }
        protected Vector3 up;
        public Vector3 Up
        {
            get { return up; }
        }
        protected Vector3 right;
        public Vector3 Right
        {
            get { return right; }
        }

        public event OnViewChange OnViewChangeEvent;
        public event OnProjectionChange OnProjectionChangeEvent;

        protected BoundingFrustum frustrum;

        public ACCamera()
        {
            //Default Values
            frustrum = new BoundingFrustum(Matrix.Identity);

            fieldOfView = MathHelper.PiOver4;
            nearClip = 0.1f;
            farClip = 10000f;
            aspectRatio = 4f / 3f;
            refreshProjection();
        }

        //For Projection
        public void setFieldOfView(float angle)
        {
            fieldOfView = angle;
            refreshProjection();
        }
        public void setClipDistances(float near, float far)
        {
            nearClip = near;
            farClip = far;
            refreshProjection();
        }
        public void onScreenResize(Vector2 size)
        {
            aspectRatio = size.X / size.Y;
            refreshProjection();
        }
        public void setAspectRatio(float ratio)
        {
            aspectRatio = ratio;
            refreshProjection();
        }
        public void refreshProjection()
        {
            projection = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView,
                aspectRatio,
                nearClip,
                farClip
                );
            frustrum.Matrix = view * projection;
            if (OnProjectionChangeEvent != null) { OnProjectionChangeEvent(projection); }
        }

        public void setView(Matrix m)
        {
            view = m;
        }
        public void setProjection(Matrix m)
        {
            projection = m;
        }


        //For View
        public void setLocation(Vector3 location)
        {
            this.location = location;
            refreshView();
        }
        public void moveByAxis(Vector3 movement)
        {
            location += movement;
            refreshView();
        }
        public void setRotation(Matrix rotation)
        {
            this.rotation = rotation;
            refreshRotation();
            refreshView();
        }
        protected void rotate(Matrix r)
        {
            rotation = r * rotation;
            refreshRotation();
            refreshView();
        }
        protected void refreshRotation()
        {
            forward = Vector3.Transform(Vector3.Forward, rotation);
            up = Vector3.Transform(Vector3.Up, rotation);
            right = Vector3.Cross(forward, up);
        }
        public void refreshView()
        {
            view = Matrix.CreateLookAt(location, location + forward, up);
            frustrum.Matrix = view * projection;
            if (OnViewChangeEvent != null) { OnViewChangeEvent(view); }
        }
    }
}
