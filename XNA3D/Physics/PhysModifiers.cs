using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNA3D.Physics
{
    public class Force : IDisposable
    {
        public const short Unknown = 0;
        public const short Gravity = 1;

        public delegate void Change(Vector3 change);

        public short Type;
        public Vector3 Direction;
        public float Magnitude;
        public Vector3 Net;

        public event Change OnChange;
        public event Change OnDestroy;

        public Force(Vector3 net, short type = Unknown)
        {
            Type = type;
            Net = net;
            Magnitude = Net.Length();
            Direction = Net / Magnitude;
            OnChange = null;
            OnDestroy = null;
        }
        public Force(Vector3 dir, float mag, short type = Unknown)
        {
            Type = type;
            Direction = dir;
            Magnitude = mag;
            Net = Direction * Magnitude;
            OnChange = null;
            OnDestroy = null;
        }

        public void Apply(ref Vector3 acceleration, float mass)
        {
            acceleration += Net / mass;
        }
        public void Dispose()
        {
            if (OnDestroy != null)
            {
                OnDestroy(-Net);
            }
        }
        
        public void changeMagnitude(float mag)
        {
            Magnitude = mag;
            Net = Direction * Magnitude;
        }
        public void changeMagnitudeSignal(float mag)
        {
            Vector3 n = Net;
            changeMagnitude(mag);
            if (OnChange != null) { OnChange(Net - n); }
        }

        public void changeDirection(Vector3 dir)
        {
            Direction = dir;
            Net = Direction * Magnitude;
        }
        public void changeDirectionSignal(Vector3 dir)
        {
            Vector3 n = Net;
            changeDirection(dir);
            if (OnChange != null) { OnChange(Net - n); }
        }

        public void changeNet(Vector3 net)
        {
            Net = net;
            Magnitude = Net.Length();
            Direction = Net / Magnitude;
        }
        public void changeNetSignal(Vector3 net)
        {
            Vector3 n = Net;
            changeNet(net);
            if (OnChange != null) { OnChange(Net - n); }
        }
    }

    public class WorldPhysics
    {
        public static WorldPhysics CommonWorld;
        static WorldPhysics()
        {
            CommonWorld = new WorldPhysics(Vector3.Down * 9.8f);
        }

        protected LinkedList<Binding> bodies;

        protected Vector3 gravAcceleration;

        public WorldPhysics(Vector3 g)
        {
            bodies = new LinkedList<Binding>();
            gravAcceleration = g;
        }

        public void addBody(ISimpleBody body)
        {
            Binding b = new Binding()
            {
                body = body,
                gravity = new Force(gravAcceleration * body.Mass, Force.Gravity)
            };
            body.addForce(b.gravity);
            bodies.AddLast(b);
        }
        public void removeBody(ISimpleBody body)
        {
            LinkedListNode<Binding> n = bodies.First;
            while (n != null)
            {
                if (n.Value.body == body) { break; }
                n = n.Next;
            }
            if (n != null)
            {
                n.Value.body.removeForce(n.Value.gravity);
                bodies.Remove(n);
            }
        }

        protected struct Binding
        {
            public ISimpleBody body;
            public Force gravity;
        }
    }
}
