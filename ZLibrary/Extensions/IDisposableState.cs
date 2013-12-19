using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public interface IDisposableState<T> : IDisposable
    {
        bool IsDisposed { get; }
        event Action<T> OnDisposal;
    }
    public interface IDisposableState : IDisposableState<object> { }

    public static class IDisposableStateExt
    {
        /// <summary>
        /// Releases An Object Using The Internal Dispose() Method
        /// </summary>
        /// <remarks>
        /// It Does Not Fire An Event Or Set The Object Disposal State
        /// To Being Disposed. This Must Not Be Used Within The Dispose Function,
        /// But Calling this.TryDispose(false) In The Finalizer Is Okay
        /// </remarks>
        /// <typeparam name="T">The State Released On Disposal</typeparam>
        /// <param name="obj">The Object To Be Disposed</param>
        /// <param name="suppress">Whether The Object Should Have Its Finalizer Suppressed</param>
        /// <returns>True If Dispose Succeeded</returns>
        public static bool TryDispose<T>(this IDisposableState<T> obj, bool suppress = true)
        {
            if (obj.IsDisposed) { return false; }
            else
            {
                if (suppress) { GC.SuppressFinalize(obj); }
                obj.Dispose();
                return true;
            }
        }
    }
}
