using System.IO.Pipelines;
using System.Threading.Tasks;

namespace RSocket.Core.Internal
{
    internal abstract class RSocketProtocolHandler : IRSocketProtocol
    {
        private int streamId;

        public RSocketStreamDispatcher Dispatcher
        {
            get;
        }

        public PipeWriter Pipe
        {
            get;
        }

        protected RSocketProtocolHandler(RSocketStreamDispatcher dispatcher, int streamId, PipeWriter pipe)
        {
            this.streamId = streamId;

            Dispatcher = dispatcher;
            Pipe = pipe;
        }

        public abstract ValueTask SetupAsync(in RSocketProtocol.Setup message);

        public abstract ValueTask RequestResponseAsync(in RSocketProtocol.RequestResponse message);

        public abstract ValueTask RequestStreamAsync(in RSocketProtocol.RequestStream message);

        public abstract ValueTask RequestChannelAsync(in RSocketProtocol.RequestChannel message);

        public abstract ValueTask RequestNAsync(in RSocketProtocol.RequestN message);

        public abstract ValueTask PayloadAsync(in RSocketProtocol.Payload message);

        public abstract ValueTask RequestFireAndForgetAsync(in RSocketProtocol.RequestFireAndForget message);
    }
}