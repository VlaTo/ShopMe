using System.Threading.Tasks;

namespace RSocket.Core.Internal
{
    public sealed class ClientRequestCredit : IRequestCredit
    {
        public int Count
        {
            get;
            private set;
        }

        public ClientRequestCredit(int initialRequest)
        {
            Count = initialRequest;
        }

        public void Add(int value)
        {
            Count += value;
        }

        public ValueTask<bool> DecrementAsync()
        {
            return 0 < Count ? new ValueTask<bool>(0 <= --Count) : new ValueTask<bool>(false);
        }
    }
}