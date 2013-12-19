using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA2D.Framework
{
    public delegate void OnCamera2DMovement();
    public delegate void OnCamera2DScaling();

    public static class Camera2D
    {
        private static Vector2 center;
        public static Vector2 Center
        {
            get
            {
                return center;
            }
        }

        private static Vector2 size;
        private static float scale;

        private static Vector2 scaleSize;
        private static Vector2 topLeft;

        public static event OnCamera2DMovement OnMovement;
        public static event OnCamera2DScaling OnScaling;

        public static Vector2 TopLeft
        {
            get
            {
                return topLeft;
            }
        }
        public static float ScalingMultiplier
        {
            get
            {
                return scale;
            }
        }

        static Camera2D()
        {
            center = Vector2.Zero;
            size = new Vector2(800, 600);
            scale = 1f;

            OnScaling += onScale;
            OnScaling();
            OnMovement += onMove;
            OnMovement();
        }

        public static void setLocation(Vector2 location)
        {
            center = location;
            OnMovement();
        }
        public static void move(Vector2 amount)
        {
            center += amount;
            OnMovement();
        }
        public static void resize(Vector2 newSize)
        {
            size = newSize;
            OnScaling();
        }
        public static void rescale(float newScale)
        {
            scale = newScale;
            OnScaling();
        }

        private static void onMove()
        {
            topLeft = center - scaleSize / 2f;
        }
        private static void onScale()
        {
            scaleSize = size / scale;
            onMove();
        }

        public static Vector2 calculateScreenLocation(Vector2 location)
        {
            return ((location - topLeft) * scale);
        }
        public static float calculateScreenScale(float _scale)
        {
            return scale * _scale;
        }
        public static Vector2 calculateScreenScale(Vector2 _scale)
        {
            return scale * _scale;
        }

        public static Vector2 unproject(Vector2 screenLocation)
        {
            return (screenLocation / scale) + topLeft;
        }
    }
}
