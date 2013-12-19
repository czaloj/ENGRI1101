using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNA2D.Cameras;
using BlisterUI;
using BlisterUI.Input;

namespace ORLabs.Graphics
{
    public class CameraController
    {
        public const int ActionCount = 8;
        public const int MoveUp = 0;
        public const int MoveDown = 1;
        public const int MoveLeft = 2;
        public const int MoveRight = 3;
        public const int RotateLeft = 4;
        public const int RotateRight = 5;
        public const int ZoomIn = 6;
        public const int ZoomOut = 7;

        public const float MovementSpeed = 20f;
        public const float RotationSpeed = 1.5f;
        public const float ZoomSpeed = 0.4f;

        protected bool[] pressedActions;
        public bool[] Actions { get { return pressedActions; } }
        protected bool isHooked;

        public int MoveYDirection
        {
            get
            {
                return
                    (pressedActions[MoveUp] ? 1 : 0) +
                    (pressedActions[MoveDown] ? -1 : 0)
                    ;
            }
        }
        public int MoveXDirection
        {
            get
            {
                return
                    (pressedActions[MoveRight] ? 1 : 0) +
                    (pressedActions[MoveLeft] ? -1 : 0)
                    ;
            }
        }
        public int RotationDirection
        {
            get
            {
                return
                    (pressedActions[RotateLeft] ? 1 : 0) +
                    (pressedActions[RotateRight] ? -1 : 0)
                    ;
            }
        }
        public int ZoomDirection
        {
            get
            {
                return
                    (pressedActions[ZoomIn] ? 1 : 0) +
                    (pressedActions[ZoomOut] ? -1 : 0)
                    ;
            }
        }

        public CameraController()
        {
            pressedActions = new bool[ActionCount];
            isHooked = false;
            hook();
        }

        public void hook()
        {
            if (!isHooked)
            {
                KeyboardEventDispatcher.OnKeyPressed += onKeyPress;
                KeyboardEventDispatcher.OnKeyReleased += onKeyRelease;
                isHooked = true;
            }
        }
        public void unhook()
        {
            if (isHooked)
            {
                KeyboardEventDispatcher.OnKeyPressed -= onKeyPress;
                KeyboardEventDispatcher.OnKeyReleased -= onKeyRelease;
                isHooked = false;
            }
        }

        public void updateCamera(Camera2D camera, float dt)
        {
            Vector2 move = new Vector2(MoveXDirection, MoveYDirection);
            if (move.X != 0 || move.Y != 0)
            {
                move *= camera.ViewSize * (dt) / 2f;
                camera.move(move);
            }
            float r = RotationDirection;
            if (r != 0)
            {
                r *= RotationSpeed * dt;
                camera.rotate(r);
            }
            switch (ZoomDirection)
            {
                case 1: camera.zoom(Vector2.One * (1 + ZoomSpeed * dt)); break;
                case -1: camera.zoom(Vector2.One / (1 + ZoomSpeed * dt)); break;
                default: break;
            }
        }

        protected void onKeyPress(object sender, KeyEventArgs args)
        {
            switch (args.KeyCode)
            {
                case Keys.W: pressedActions[MoveUp] = true; break;
                case Keys.S: pressedActions[MoveDown] = true; break;
                case Keys.A: pressedActions[MoveLeft] = true; break;
                case Keys.D: pressedActions[MoveRight] = true; break;
                case Keys.Q: pressedActions[RotateLeft] = true; break;
                case Keys.E: pressedActions[RotateRight] = true; break;
                case Keys.Z: pressedActions[ZoomIn] = true; break;
                case Keys.X: pressedActions[ZoomOut] = true; break;
            }
        }
        protected void onKeyRelease(object sender, KeyEventArgs args)
        {
            switch (args.KeyCode)
            {
                case Keys.W: pressedActions[MoveUp] = false; break;
                case Keys.S: pressedActions[MoveDown] = false; break;
                case Keys.A: pressedActions[MoveLeft] = false; break;
                case Keys.D: pressedActions[MoveRight] = false; break;
                case Keys.Q: pressedActions[RotateLeft] = false; break;
                case Keys.E: pressedActions[RotateRight] = false; break;
                case Keys.Z: pressedActions[ZoomIn] = false; break;
                case Keys.X: pressedActions[ZoomOut] = false; break;
            }
        }

    }
}
