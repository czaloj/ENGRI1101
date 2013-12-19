using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Threading
{
    public static class Sync
    {
        public static bool CompareAndSwap<T>(ref T location, T comparand, T newValue) where T : class
        {
            return comparand == Interlocked.CompareExchange<T>(ref location, newValue, comparand);
        }
    }
}
