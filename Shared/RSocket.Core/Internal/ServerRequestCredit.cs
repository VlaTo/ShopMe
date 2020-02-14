using System.Threading.Tasks;

namespace RSocket.Core.Internal
{
    public sealed class ServerRequestCredit : IRequestCredit
    {
        private readonly object gate;
        private readonly AsyncMonitor monitor;

        public int Count
        {
            get; 
            private set;
        }

        public ServerRequestCredit(int creditCount)
        {
            monitor = new AsyncMonitor();
            gate = new object();
            Count = creditCount;
        }

        public void Add(int value)
        {
            lock (gate)
            {
                Count += value;
                monitor.Pulse();
            }
        }

        public async ValueTask<bool> DecrementAsync()
        {
            while (true)
            {
                var completion = Task.CompletedTask;

                lock (gate)
                {
                    if (0 < Count)
                    {
                        Count--;
                        break;
                    }

                    completion = monitor.WaitAsync();
                }

                await completion;
            }

            return true;
        }
    }
}