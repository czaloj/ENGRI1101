using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using XNA3D.Objects.Data;
using XNA3D.Objects.Graphics;
using XNA3D.Cameras;
using XNA3D.Physics;

namespace XNA3D.Objects
{
    public class PhysicalObject : GameObject, IBody
    {
        protected PhysData pData;
        public Vector3 Phys_Position
        {
            get { return pData.Position; }
        }
        public Vector3 Phys_Velocity
        {
            get { return pData.Velocity; }
        }
        public Vector3 Phys_Acceleration
        {
            get { return pData.Acceleration; }
        }

        protected float mass;
        public float Mass
        {
            get { return mass; }
        }

        protected LinkedList<Force> forces;
        public LinkedList<Force> Forces
        {
            get { return forces; }
        }
        protected Force moveX, moveY, moveZ;

        public Vector3 Momentum
        {
            get { return mass * pData.Velocity; }
        }
        public Vector3 KineticEnergy
        {
            get { return mass * pData.Velocity * pData.Velocity / 2f; }
        }

        public PhysicalObject(float m = 1f)
            : base()
        {
            forces = new LinkedList<Force>();

            mass = m;
            pData = new PhysData(Vector3.Zero, Vector3.Zero, Vector3.Zero);

            moveX = new Force(Vector3.Right, 0f);
            moveY = new Force(Vector3.Up, 0f);
            moveZ = new Force(Vector3.Forward, 0f);
            addForce(moveX);
            addForce(moveY);
            addForce(moveZ);
        }

        public void addForce(Force f)
        {
            forces.AddLast(f);
            f.Apply(ref pData.Acceleration, mass);
            f.OnChange += onForceChange;
            f.OnDestroy += onForceRemove;
        }
        public void removeForce(Force f)
        {
            forces.Remove(f);
            f.Apply(ref pData.Acceleration, -mass);
            f.OnChange -= onForceChange;
            f.OnDestroy -= onForceRemove;
        }

        public void onForceChange(Vector3 change)
        {
            pData.Acceleration += change / mass;
        }
        public void onForceRemove(Vector3 change)
        {
            pData.Acceleration += change / mass;
        }

        //public virtual void moveForward(float elapsedTime)
        //{
        //    moveZ.changeMagnitudeSignal(elapsedTime);
        //}
        //public virtual void moveUp(float elapsedTime)
        //{
        //    moveY.changeMagnitudeSignal(elapsedTime);
        //}
        //public virtual void moveRight(float elapsedTime)
        //{
        //    moveX.changeMagnitudeSignal(elapsedTime);
        //}

        public override void addPitch(float angle)
        {
            base.addPitch(angle);
            moveZ.changeDirectionSignal(Forward);
            moveY.changeDirectionSignal(Up);
        }
        public override void addRoll(float angle)
        {
            base.addRoll(angle);
            moveY.changeDirectionSignal(Up);
            moveX.changeDirectionSignal(Right);
        }
        public override void addYaw(float angle)
        {
            base.addYaw(angle);
            moveX.changeDirectionSignal(Right);
            moveZ.changeDirectionSignal(Forward);
        }

        public void setVelocity(Vector3 v)
        {
            pData.Velocity = v;
        }

        public override void setPosition(Vector3 position)
        {
            base.setPosition(position);
            pData.Position = Position;
        }

        protected virtual void move(float dt)
        {
            PhysData.integrate(pData, dt, out pData);
            base.setPosition(pData.Position);
        }
        public virtual void update(float dt)
        {
            move(dt);
        }

    }
}
