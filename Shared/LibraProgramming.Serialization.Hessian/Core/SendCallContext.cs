using System;
using System.Threading.Tasks;

namespace LibraProgramming.Serialization.Hessian.Core
{
    internal abstract class SendCallContext : IDisposable
    {
        public static SendCallContext<TResponse> Create<TResponse>(TaskCompletionSource<TResponse> tcs)
        {
            return new SendCallContext<TResponse>(tcs);
        }

        public abstract void Dispose();
    }

    internal sealed class SendCallContext<TResponse> : SendCallContext
    {
        private bool disposed;

        public TaskCompletionSource<TResponse> Completion
        {
            get;
        }

        internal SendCallContext(TaskCompletionSource<TResponse> tcs)
        {
            Completion = tcs;
        }

        public override void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool dispose)
        {
            if (disposed)
            {
                return;
            }

            try
            {
                if (dispose)
                {

                }
            }
            finally
            {
                disposed = true;
            }
        }
    }
}