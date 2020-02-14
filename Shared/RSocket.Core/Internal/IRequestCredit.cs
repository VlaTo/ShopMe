using System.Threading.Tasks;

namespace RSocket.Core.Internal
{
    public interface IRequestCredit
    {
        int Count
        {
            get;
        }

        void Add(int value);

        ValueTask<bool> DecrementAsync();
    }
}