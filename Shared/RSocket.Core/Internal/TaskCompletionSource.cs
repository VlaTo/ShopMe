using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RSocket.Core.Internal
{
    internal sealed class TaskCompletionSource
    {
        private readonly TaskCompletionSource<bool> tcs;

        public Task Task => tcs.Task;

        public TaskCompletionSource()
            : this(TaskCreationOptions.None)
        {
        }

        public TaskCompletionSource(TaskCreationOptions creationOptions)
        {
            tcs = new TaskCompletionSource<bool>();

        }
        
        public bool TrySetException(Exception exception)
        {
            if (null == exception)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            return tcs.TrySetException(exception);
        }

        public bool TrySetException(IEnumerable<Exception> exceptions)
        {
            if (null == exceptions)
            {
                throw new ArgumentNullException(nameof(exceptions));
            }

            return tcs.TrySetException(exceptions);
        }

        public void SetException(Exception exception)
        {
            if (null == exception)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            if (false == TrySetException(exception))
            {
                throw new InvalidOperationException();
            }
        }
        
        public void SetException(IEnumerable<Exception> exceptions)
        {
            if (false == TrySetException(exceptions))
            {
                throw new InvalidOperationException();
            }
        }

        public bool TrySetCompleted() => tcs.TrySetResult(true);

        public void SetCompleted()
        {
            if (false == TrySetCompleted())
            {
                throw new InvalidOperationException();
            }
        }

        public bool TrySetCanceled() => TrySetCanceled(default);

        public bool TrySetCanceled(CancellationToken cancellationToken) => tcs.TrySetCanceled(cancellationToken);

        public void SetCanceled()
        {
            if (false == TrySetCanceled())
            {
                throw new InvalidOperationException();
            }
        }
    }
}