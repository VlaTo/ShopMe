using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace RSocket.Core
{
    /// <summary>
    /// A Pipeline Transport for a connectable RSocket. Once connected, the Input and Output Pipelines
    /// can be used to communicate abstractly with the RSocket bytestream.
    /// </summary>
    public interface IRSocketTransport : IDuplexPipe
    {
        ValueTask StartAsync(CancellationToken cancellationToken = default);

        Task StopAsync();
    }
}