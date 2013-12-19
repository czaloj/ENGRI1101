using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZLibrary.Algorithms
{
    public delegate void AlgEvent<T>(object sender, T args);

    public interface IAlgorithm<I, O> : IDisposable
        where I : struct
        where O : struct
    {
        bool WriteToLog { get; set; }

        O process(I input);

        void setLog(Stream s, bool closeOnCompletion = false, bool writetoLog = true);
    }

    public interface IThreadedAlgorithm<I, O> : IAlgorithm<I, O>
        where I : struct
        where O : struct
    {
        event AlgEvent<O> OnCompletion;

        bool IsFinished { get; }

        bool HasResult { get; }
        O? Result { get; }

        void begin(I input);
        void terminate();
    }

    public interface ISteppable
    {
        bool IsPaused { get; }

        void step();
    }

    public interface IStateAlgorithm<I, O, S> : IThreadedAlgorithm<I, O>, ISteppable
        where I : struct
        where O : struct
        where S : struct
    {
        bool UseStepping { get; set; }
        bool PauseByStep { get; set; }
        int MilliPauseRate { get; set; }

        int NewStatesCount { get; }
        S getNextState();
    }
}
