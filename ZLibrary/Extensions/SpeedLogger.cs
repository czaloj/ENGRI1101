using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Diagnostics
{
    public class SpeedLogger : IEnumerable<SpeedLogger.Checkpoint>
    {
        private Dictionary<string, Checkpoint> checkPoints;
        public int Count
        {
            get { return checkPoints.Count; }
        }
        public Checkpoint this[string name]
        {
            get { return checkPoints[name]; }
        }

        private LinkedQueue<DumpData> dumpQueue;

        private double frameTime;

        private long index;
        public long Index
        {
            get { return index++; }
        }

        public SpeedLogger()
        {
            index = 0;
            checkPoints = new Dictionary<string, Checkpoint>();
            dumpQueue = new LinkedQueue<DumpData>();
            frameTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
        }

        public event Action<DumpData> OnDump;

        public event Action<SpeedLogger, Checkpoint> OnNewCheckpoint;
        public event Action<SpeedLogger, Checkpoint> OnCheckpointRemoval;

        public bool addCheck(Checkpoint p)
        {
            if (checkPoints.ContainsKey(p.Name)) { return false; }
            checkPoints.Add(p.Name, p);
            if (OnNewCheckpoint != null) { OnNewCheckpoint(this, p); }
            return true;
        }
        public bool addCheck(string name, out Checkpoint p)
        {
            p = new Checkpoint(name, this);
            return addCheck(p);
        }
        public bool removeCheck(string name)
        {
            if(checkPoints.ContainsKey(name))
            {
                Checkpoint c = checkPoints[name];
                index--;
                if (OnCheckpointRemoval != null) { OnCheckpointRemoval(this, c); }
                return checkPoints.Remove(name);
            }
            return false;
        }

        public void setNewFrame() { dumpQueue.enqueue(new DumpData(DateTime.Now.TimeOfDay.TotalMilliseconds)); }

        public void dump()
        {
            if (OnDump == null) { dumpQueue.clear(); return; }
            DumpData d;
            while (dumpQueue.Count > 0)
            {
                d = dumpQueue.dequeue();
                if (d.IsNewFrame)
                {
                    OnDump(new DumpData(d.ElapsedMilliseconds - frameTime));
                    frameTime = d.ElapsedMilliseconds;
                }
                else { OnDump(d); }
            }
        }

        public struct DumpData
        {
            public const long NewFrameIndex = -1;

            public long Index;
            public string CheckpointName;
            public double ElapsedMilliseconds;

            public bool IsNewFrame { get { return Index == NewFrameIndex; } }

            public DumpData(Checkpoint c)
            {
                Index = c.Index;
                CheckpointName = c.Name;
                ElapsedMilliseconds = c.LastTime;
            }
            public DumpData(double frameTime)
            {
                Index = NewFrameIndex;
                CheckpointName = null;
                ElapsedMilliseconds = frameTime;
            }
        }

        public class Checkpoint : IDisposable
        {
            public string Name { get; private set; }
            private Stopwatch watch;
            private SpeedLogger log;
            public int Priority { get; private set; }

            private long index;
            public long Index
            {
                get{return index;}
            }

            private double lastTime;
            public double LastTime
            {
                get { return lastTime; }
            }

            public Checkpoint(string name, SpeedLogger logger)
            {
                if(logger == null){throw new ArgumentException("Must Be Bound To A Logger");}
                watch = new Stopwatch();
                log = logger;
                Name = name;
                index = log.Index;
                log.OnCheckpointRemoval += onLogRemoval;
            }
            ~Checkpoint()
            {
                log.OnCheckpointRemoval -= onLogRemoval;
            }
            public void Dispose()
            {
                GC.SuppressFinalize(this);
                log.OnCheckpointRemoval -= onLogRemoval;
            }

            public void start()
            {
                watch.Start();
            }
            public void pause()
            {
                watch.Stop();
            }
            public void end()
            {
                if (watch.IsRunning)
                {
                    watch.Stop();
                    lastTime = watch.Elapsed.TotalMilliseconds;
                    log.dumpQueue.enqueue(new DumpData(this));
                    watch.Reset();
                }
            }

            public void onLogRemoval(SpeedLogger log, Checkpoint cp)
            {
                if (index > cp.index) { index--; }
            }

            public override string ToString()
            {
                return string.Format("{0}: {1} ms", Name, lastTime);
            }
        }

        public IEnumerator<SpeedLogger.Checkpoint> GetEnumerator()
        {
            foreach (var c in checkPoints) { yield return c.Value; }
        }
        Collections.IEnumerator Collections.IEnumerable.GetEnumerator()
        {
            foreach (var c in checkPoints) { yield return c.Value; }
        }
    }
}
