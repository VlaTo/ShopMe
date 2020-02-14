using System.Threading.Tasks;

namespace RSocket.Core.Internal
{
    internal sealed class AsyncMonitor3
    {
        private readonly object mutex;
        private TaskCompletionSource<bool> awaiter;

        public AsyncMonitor3()
        {
            mutex = new object();
            awaiter = null;
        }

        public void Pulse()
        {
            var w = awaiter;

            if (w == null)
            {
                return;
            }

            lock (mutex)
            {
                w = awaiter;

                if (w == null)
                {
                    return;
                }

                awaiter = null;
                w.TrySetResult(true);
            }
        }

        public Task WaitAsync()
        {
            var w = awaiter;
            if (w != null)
            {
                return w.Task;
            }

            lock (mutex)
            {
                w = awaiter;
                if (w != null)
                {
                    return w.Task;
                }

                w = awaiter = new TaskCompletionSource<bool>();
                return w.Task;
            }
        }
    }
}