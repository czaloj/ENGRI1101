using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Threading
{
    public abstract class Worker<T>
    {
        protected WorkerList<T> parentList;
        protected Thread thread;

        bool finished;
        public bool IsAlive
        {
            get
            {
                return
                    !finished && thread != null &&
                    thread.ThreadState.HasFlag(ThreadState.Running)
                    ;
            }
        }

        public Worker()
        {
            parentList = null;
            thread = null;
        }
        public void begin(
            WorkerList<T> l, T obj,
            string name,
            ThreadPriority p = ThreadPriority.Normal,
            bool back = true,
            ApartmentState ap = ApartmentState.MTA
            )
        {
            finished = false;
            parentList = l;
            parentList.OnDisposal += onListDispose;

            thread = new Thread((o) =>
            {
                f((T)o);
                parentList.OnDisposal -= onListDispose;
                finished = true;
            });
            thread.Name = name;
            thread.TrySetApartmentState(ap);
            thread.IsBackground = back;
            thread.Priority = p;
            thread.Start(obj);
        }

        protected abstract void f(T obj);

        public void onListDispose(WorkerList<T> l)
        {
            parentList.OnDisposal -= onListDispose;
            if (IsAlive) { thread.Abort(); }
        }
    }

    public class WorkerList<T> : IDisposableState<WorkerList<T>>
    {
        public ApartmentState Apartment;
        public ThreadPriority Priority;
        public bool IsBackground;

        protected LinkedList<Worker<T>> wAlive;
        public int WorkersAlive
        {
            get { return wAlive.Count; }
        }

        public event Action<WorkerList<T>> OnDisposal;

        public WorkerList()
        {
            IsDisposed = false;
            wAlive = new LinkedList<Worker<T>>();
        }
        ~WorkerList()
        {
            dispose();
        }
        public bool IsDisposed
        {
            get;
            protected set;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            dispose();
        }
        protected virtual void dispose()
        {
            IsDisposed = true;
            if (OnDisposal != null) { OnDisposal(this); }
            wAlive.Clear();
        }

        public void createNew<W>(string tName, T o) where W : Worker<T>, new()
        {
            createNew<W>(tName, new W(), o);
        }
        public void createNew<W>(string tName, W w, T o) where W : Worker<T>
        {
            w.begin(this, o, tName, Priority, IsBackground, Apartment);
            wAlive.AddLast(w);
        }

        private static bool workerKeep(Worker<T> w, int i) { return w.IsAlive; }
        public void update()
        {
            wAlive = new LinkedList<Worker<T>>(wAlive.Where(workerKeep));
        }
    }
}
