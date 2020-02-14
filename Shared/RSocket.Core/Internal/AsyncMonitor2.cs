using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RSocket.Core.Internal
{
    internal sealed class AsyncMonitor2
    {
        private readonly ConcurrentQueue<TaskCompletionSource<bool>> awaiters;

        public AsyncMonitor2()
        {
            awaiters = new ConcurrentQueue<TaskCompletionSource<bool>>();
        }

        public void Pulse()
        {
            if (awaiters.TryDequeue(out var tcs))
            {
                tcs.TrySetResult(true);
            }
        }

        public Task WaitAsync()
        {
            if (awaiters.TryPeek(out var tcs))
            {
                return tcs.Task;
            }

            tcs = new TaskCompletionSource<bool>();

            awaiters.Enqueue(tcs);

            return tcs.Task;
        }
    }
}