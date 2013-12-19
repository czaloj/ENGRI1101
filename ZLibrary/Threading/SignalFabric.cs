using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ZLibrary.Threading
{
    /// <summary>
    /// Represents A Lot Of Interwoven Threads
    /// That Can Be Paused And Resumed
    /// </summary>
    /// <typeparam name="T">A Subclass Of Waiting Thread</typeparam>
    public abstract class SignalFabric<T> where T : WaitingThread, new()
    {
        //Stores All The Threads
        private List<T> threads = new List<T>();

        //Used For Determining Dormancy
        private object activeLock = new object();
        private int activeThreads = 0;

        /// <summary>
        /// Creates And Starts All The Threads In The Fabric
        /// </summary>
        /// <param name="n">Number Of Threads To Create</param>
        public virtual void buildThreads(int n)
        {
            threads = new List<T>(n);
            for (int i = 0; i < n; i++)
            {
                T thread = new T();
                addThread(thread);
            }
        }
        /// <summary>
        /// Adds And Starts A New Thread
        /// </summary>
        /// <param name="thread">The Thread</param>
        public virtual void addThread(T thread)
        {
            lock (activeLock)
            {
                threads.Add(thread);
                thread.start();
                activeThreads++;
            }
        }
        /// <summary>
        /// Removes The Thread From The Fabric
        /// </summary>
        /// <param name="thread">The Thread To Be Removed</param>
        public virtual void removeThread(T thread)
        {
            lock (activeLock)
            {
                thread.stop();
                threads.Remove(thread);
                activeThreads--;
            }
        }
        /// <summary>
        /// Return How Many Threads Are Actively Running
        /// </summary>
        /// <returns>The Number Of Active Threads</returns>
        public int numberOfActiveThreads()
        {
            lock (activeLock)
            {
                return activeThreads;
            }
        }
        /// <summary>
        /// Pauses A Single Thread
        /// </summary>
        /// <param name="thread">The Thread (Should Be In The List)</param>
        public virtual void pause(T thread)
        {
            if (!thread.isPaused())
            {
                lock (activeLock)
                {
                    activeThreads--;
                }
                thread.pause();
            }
        }
        /// <summary>
        /// Pauses All Active Threads
        /// </summary>
        public virtual void pauseAll()
        {
            foreach (T thread in threads)
            {
                if (!thread.isPaused())
                {
                    if (!thread.isFrayed())
                    {
                        activeThreads--;
                        thread.pause();
                    }
                    else
                    {
                        threads.Remove(thread);
                    }
                }
            }
        }
        /// <summary>
        /// Resumes A Single Thread
        /// </summary>
        /// <param name="thread">The Thread (Should Be In The List)</param>
        public virtual void resume(T thread)
        {
            lock (activeLock)
            {
                if (thread.isPaused())
                {
                    thread.resume();
                    activeThreads++;
                }
            }
        }
        /// <summary>
        /// Resumes All Paused Threads
        /// </summary>
        public virtual void resumeAll()
        {
            foreach (T thread in threads)
            {
                if (thread.isPaused())
                {
                    if (!thread.isFrayed())
                    {
                        activeThreads++;
                        thread.resume();
                    }
                    else
                    {
                        threads.Remove(thread);
                    }
                }
            }
        }
        /// <summary>
        /// Stops All Active Threads
        /// </summary>
        public virtual void stopAll()
        {
            lock (activeLock)
            {
                foreach (T thread in threads)
                {
                    thread.stop();
                }
                threads.Clear();
            }
        }
        /// <summary>
        /// Removes All The Threads That Are Terminated In The List
        /// </summary>
        public void removeFrayedThreads()
        {
            lock (activeLock)
            {
                foreach (T thread in threads)
                {
                    if (thread.isFrayed())
                    {
                        threads.Remove(thread);
                    }
                }
            }
        }
    }
}
