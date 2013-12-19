using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Diagnostics
{
    public class SpeedTest
    {
        private static readonly Action NoAction = () => { };

        Stopwatch watch;
        Action action;

        int loopC, testC;
        public int LoopCount
        {
            get { return loopC; }
            set { loopC = value; }
        }
        public int TestCount
        {
            get { return testC; }
            set { testC = value; }
        }

        double time;
        public double AverageTimeMilliSeconds
        {
            get { return time; }
        }

        private void init()
        {
            watch = new Stopwatch();
            time = double.PositiveInfinity;
            loopC = 10;
            testC = 10;
        }
        public SpeedTest()
        {
            init();
            action = NoAction;
        }
        public SpeedTest(Action f)
        {
            init();
            action = f;
        }

        public void setFunction(Action f)
        {
            action = f;
        }
        public void runTests()
        {
            int ti, li;
            time = 0.0;
            for (ti = 0; ti < testC; )
            {
                watch.Reset();
                watch.Start();
                for (li = 0; li < loopC; li++)
                {
                    action();
                }
                watch.Stop();
                time *= (double)(ti) / (double)(ti + 1);
                ti++;
                time += (watch.Elapsed.TotalMilliseconds / loopC) / ti;
            }
        }
    }
}
