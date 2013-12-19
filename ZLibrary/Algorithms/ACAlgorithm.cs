using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using ZLibrary.ADT;

namespace ZLibrary.Algorithms
{
    public abstract class ACAlgorithm<I, O> : IAlgorithm<I, O>
        where I : struct
        where O : struct
    {
        protected StreamWriter log;
        
        protected bool logWrite;
        /// <summary>
        /// Get Or Set Log-Writing Functionality
        /// Will Only Be Set True If A Log Is Available And Writeable
        /// </summary>
        public bool WriteToLog
        {
            get
            {
                return logWrite;
            }
            set
            {
                logWrite |= (value && log != null && log.BaseStream.CanWrite);
            }
        }

        public ACAlgorithm() { }
        /// <summary>
        /// This Will Close The Log, If Any
        /// </summary>
        public void Dispose()
        {
            if (log != null)
            {
                log.Dispose();
                log = null;
            }
        }

        /// <summary>
        /// Processes The Input As Defined By 
        /// The Algorithm And Returns The Output
        /// </summary>
        /// <param name="input">The Input</param>
        /// <returns>The Output From The Specified Input</returns>
        public abstract O process(I input);

        public override string ToString()
        {
            return "Algorithm | " + typeof(I).Name + " -> " + typeof(O).Name;
        }

        /// <summary>
        /// Sets The Default Output Log
        /// </summary>
        /// <param name="s">The Log</param>
        /// <param name="closeOnCompletion">True If The Log Should Close When The Algorithm Completes</param>
        /// <param name="writetoLog">Whether Log Writing Is Initially Set</param>
        /// <param name="closeExisting">Whether The Previous DefaultLog Should Be Closed Before The Set, If Any</param>
        public void setLog(Stream s, bool closeOnCompletion = false, bool writetoLog = true, bool closeExisting = true)
        {
            if (closeExisting && log != null && log.BaseStream.CanWrite)
            {
                log.Close();
            }
            if (s != null)
            {
                //Using A More Limited Character Set Is Okay
                log = new StreamWriter(s, Encoding.UTF8);
                WriteToLog = writetoLog;
            }
        }

        #region IAlgorithm<I,O> Members


        public void setLog(Stream s, bool closeOnCompletion = false, bool writetoLog = true)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public abstract class ACThreadedAlgorithm<I, O> : ACAlgorithm<I, O>, IThreadedAlgorithm<I, O>
        where I : struct
        where O : struct
    {
        public event AlgEvent<O> OnCompletion;

        public bool IsFinished
        {
            get { return threadProcess != null && threadProcess.ThreadState != ThreadState.Running; }
        }

        protected I input;

        protected O? result;
        public O? Result
        {
            get { return result; }
        }
        public bool HasResult
        {
            get { return result != null && result.HasValue; }
        }

        protected Thread threadProcess;

        public ACThreadedAlgorithm()
            : base()
        {
            result = null;
        }

        protected abstract void processThread();

        public void begin(I input)
        {
            //Set Input Referenced By Process Thread
            this.input = input;

            terminate();

            //Try To Create The Thread
            try
            {
                threadProcess = new Thread(processThread);
                threadProcess.Name = ToString();
                threadProcess.IsBackground = true;

                //Try To Set A Good Apartment State
                if (!threadProcess.TrySetApartmentState(ApartmentState.MTA))
                {
                    threadProcess.TrySetApartmentState(ApartmentState.STA);
                }
                if (WriteToLog)
                {
                    log.WriteLine("Thread Successfully Created With Apartment State: " + Enum.GetName(typeof(ThreadState), threadProcess.GetApartmentState()));
                }
            }
            catch (Exception e)
            {
                //Was Unable To Create The Thread
                if (WriteToLog)
                {
                    log.WriteLine("Unable To Create The Thread:\r\n" + e.Message);
                }
            }

            threadProcess.Start();
        }

        protected void endThread(bool hasResult)
        {
            if (hasResult && HasResult && OnCompletion != null)
            {
                //Send Signal Out To Listeners
                OnCompletion(this, result.Value);
            }
            if (WriteToLog)
            {
                log.WriteLine("Algorithm Terminated Successfully");
            }
        }
        public void terminate()
        {
            if (threadProcess != null && threadProcess.ThreadState.HasFlag(ThreadState.Running))
            {
                try
                {
                    //Terminate The Thread
                    threadProcess.Abort();
                }
                catch (Exception e)
                {
                    //Problem With Threading
                    if (WriteToLog)
                    {
                        log.WriteLine("Algorithm Could Not Be Terminated:\r\n" + e.Message);
                    }
                }
            }
        }
    }

    public abstract class ACStateAlgorithm<I, O, S> : ACThreadedAlgorithm<I, O>, IStateAlgorithm<I, O, S>
        where I : struct
        where O : struct
        where S : struct
    {
        private System.Collections.Concurrent.ConcurrentQueue<S> StateQueue;
        public int NewStatesCount
        {
            get { return StateQueue.Count; }
        }

        protected S curState;
        public S CurrentState
        {
            get { return curState; }
        }

        protected bool useStep;
        public bool UseStepping
        {
            get { return useStep; }
            set { useStep = value; }
        }
        protected bool useStepPause;
        public bool PauseByStep
        {
            get { return useStepPause; }
            set { useStepPause = value; }
        }
        protected int milliPauseTime;
        public int MilliPauseRate
        {
            get { return milliPauseTime; }
            set { milliPauseTime = value; }
        }

        protected bool paused;
        public bool IsPaused
        {
            get { return paused; }
        }

        public ACStateAlgorithm()
            : base()
        {
            StateQueue = new System.Collections.Concurrent.ConcurrentQueue<S>();
        }

        public void step()
        {
            paused = false;
        }
        protected void pause(int milliRefreshRate = 50)
        {
            paused = true;
            while (paused)
            {
                Thread.Sleep(milliRefreshRate);
            }
        }
        protected void handleStepping(int milliRefreshRate = 50)
        {
            if (useStep)
            {
                if (useStepPause)
                {
                    pause(milliRefreshRate);
                }
                else
                {
                    Thread.Sleep(milliPauseTime);
                }
            }
        }

        public S getNextState()
        {
            S o;
            while (!StateQueue.TryDequeue(out o)) { }
            return o;
        }
        protected void addState(S state)
        {
            StateQueue.Enqueue(state);
        }
        public void clearStates()
        {
            StateQueue = new System.Collections.Concurrent.ConcurrentQueue<S>();
        }
    }
}
