using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA3D.Physics
{
    public interface ISimpleBody
    {
        Vector3 Phys_Position { get; }
        Vector3 Phys_Velocity { get; }
        Vector3 Phys_Acceleration { get; }
        float Mass { get; }

        LinkedList<Force> Forces { get; }

        void addForce(Force f);
        void removeForce(Force f);
    }
    public interface IBody : ISimpleBody
    {
        Vector3 Momentum { get; }
        Vector3 KineticEnergy { get; }
    }

    public class SimpleBody : ISimpleBody
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

        public SimpleBody()
        {
            forces = new LinkedList<Force>();
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
    }
}
