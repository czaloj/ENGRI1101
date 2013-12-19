using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ZLibrary.Threading
{
    /// <summary>
    /// Thread That Allows For Pausing / Resuming
    /// </summary>
    public abstract class WaitingThread
    {
        //The Thread
        private Thread thread;

        //Used For Pausing / Resuming
        private bool isWaiting = false;
        private CountdownEvent countDown = new CountdownEvent(1);
        private object countLock = new object();

        /// <summary>
        /// Starts The Thread Only If It Can Be Started
        /// </summary>
        public virtual void start()
        {
            if (thread == null ||
                thread.ThreadState == ThreadState.Stopped ||
                thread.ThreadState == ThreadState.Aborted ||
                thread.ThreadState == ThreadState.Unstarted
                )
            {
                thread = new Thread(new ThreadStart(run));
                thread.Start();
            }
        }
        /// <summary>
        /// Aborts The Thread
        /// </summary>
        public virtual void stop()
        {
            thread.Abort();
        }
        /// <summary>
        /// Tells If A Thread Is Dead Or Non-existent
        /// </summary>
        /// <returns>True If Dead</returns>
        public bool isFrayed()
        {
            return
                thread == null ||
                thread.ThreadState == ThreadState.Aborted ||
                thread.ThreadState == ThreadState.Suspended ||
                thread.ThreadState == ThreadState.Unstarted ||
                thread.ThreadState == ThreadState.Stopped;
        }
        /// <summary>
        /// Tells If A Thread Is Paused
        /// </summary>
        /// <returns>True If Paused</returns>
        public bool isPaused()
        {
            lock (countLock)
            {
                return isWaiting;
            }
        }
        /// <summary>
        /// Pauses The Thread If It Isn't Already
        /// </summary>
        public virtual void pause()
        {
            lock (countLock)
            {
                if (!isWaiting)
                {
                    isWaiting = true;
                    countDown.Reset(1);
                }
            }
            countDown.Wait();
        }
        /// <summary>
        /// Resumes The Thread From A Paused State
        /// </summary>
        public virtual void resume()
        {
            lock (countLock)
            {
                if (isWaiting)
                {
                    isWaiting = false;
                    countDown.Signal();
                }
            }
        }
        /// <summary>
        /// This Is The Method Invoked By The Thread
        /// </summary>
        public abstract void run();
    }
}
