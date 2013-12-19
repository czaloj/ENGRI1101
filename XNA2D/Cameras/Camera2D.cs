using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA2D.Cameras
{
    public class Camera2D
    {
        protected Matrix mView;
        protected Matrix mProjection;

        public Matrix View
        {
            get { return mView; }
        }
        public Matrix Projection
        {
            get { return mProjection; }
        }

        protected Vector3 location;
        public Vector3 Location
        {
            get { return location; }
        }

        protected Vector2 viewSize;
        protected Vector2 projSize;
        public Vector2 ViewSize
        {
            get { return viewSize; }
        }

        protected float rotation;
        public float RotationAngle
        {
            get { return rotation; }
        }
        protected Vector3 upOrientation;
        protected Vector2 posX;
        protected Vector2 posY;

        public Camera2D()
        {
        }
        public Camera2D(Vector2 location, Vector2 viewSize, float rotation = 0f)
        {
            this.location = new Vector3(location, 0);
            setViewSize(viewSize);
            setRotation(rotation);
        }
        public void refreshView()
        {
            mView = Matrix.CreateLookAt(location, location + Vector3.Forward, upOrientation);
        }
        public void refreshProjection()
        {
            mProjection = Matrix.CreateOrthographic(projSize.X, projSize.Y, 0, 1);
        }

        public void setRotation(float r)
        {
            rotation = MathHelper.WrapAngle(r);
            posY.X = (float)Math.Cos(rotation + MathHelper.PiOver2);
            posY.Y = (float)Math.Sin(rotation + MathHelper.PiOver2);
            posX.X = (float)Math.Cos(rotation);
            posX.Y = (float)Math.Sin(rotation);

            upOrientation.X = posY.X;
            upOrientation.Y = posY.Y;

            refreshView();
        }
        public void setLocation(Vector2 v)
        {
            location.X = v.X;
            location.Y = v.Y;
            refreshView();
        }
        public void setViewSize(Vector2 v)
        {
            viewSize = v;
            projSize = viewSize;
            refreshProjection();
        }

        public void move(Vector2 v)
        {
            Vector2 loc = v.X * posX + v.Y * posY;
            location.X += loc.X;
            location.Y += loc.Y;
            refreshView();
        }
        public void rotate(float f)
        {
            setRotation(rotation + f);
        }
        public void zoom(Vector2 v)
        {
            setViewSize(viewSize / v);
        }

        public void getViewLocation(out Vector2 v)
        {
            v = new Vector2(location.X, location.Y);
        }
    }
}
