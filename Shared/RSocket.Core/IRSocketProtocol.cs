using System.Buffers;
using System.Threading.Tasks;

namespace RSocket.Core
{
    internal interface IRSocketProtocol
    {
        ValueTask SetupAsync(in RSocketProtocol.Setup message);

        ValueTask RequestResponseAsync(in RSocketProtocol.RequestResponse message);

        ValueTask RequestStreamAsync(in RSocketProtocol.RequestStream message);

        ValueTask RequestChannelAsync(in RSocketProtocol.RequestChannel message);

        ValueTask RequestNAsync(in RSocketProtocol.RequestN message);

        ValueTask PayloadAsync(in RSocketProtocol.Payload message);

        ValueTask RequestFireAndForgetAsync(in RSocketProtocol.RequestFireAndForget message);
    }
}