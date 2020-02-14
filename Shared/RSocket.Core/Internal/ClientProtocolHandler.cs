using RSocket.Core.Extensions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace RSocket.Core.Internal
{
    internal sealed class ClientProtocolHandler : RSocketProtocolHandler
    {
        private readonly RSocketOptions options;

        public ClientProtocolHandler(RSocketStreamDispatcher dispatcher, PipeWriter pipe, RSocketOptions options)
            : base(dispatcher, -1, pipe)
        {
            this.options = options;
        }

        public override ValueTask SetupAsync(in RSocketProtocol.Setup message)
        {
            throw new NotImplementedException();
        }

        public override ValueTask RequestResponseAsync(in RSocketProtocol.RequestResponse message)
        {
            throw new NotImplementedException();
        }

        public override ValueTask RequestStreamAsync(in RSocketProtocol.RequestStream message)
        {
            throw new NotImplementedException();
        }

        public override ValueTask RequestChannelAsync(in RSocketProtocol.RequestChannel message)
        {
            throw new NotImplementedException();
        }

        public override ValueTask RequestNAsync(in RSocketProtocol.RequestN message)
        {
            throw new NotImplementedException();
        }

        public override ValueTask RequestFireAndForgetAsync(in RSocketProtocol.RequestFireAndForget message)
        {
            throw new NotImplementedException();
        }

        public override ValueTask PayloadAsync(in RSocketProtocol.Payload message)
        {
            var streamId = message.StreamId;

            if (false == Dispatcher.TryGetStream(streamId, out var observer))
            {
                throw new KeyNotFoundException();
            }

            var value = (message.Metadata, message.Data);

            return RunObserver(streamId, observer, message.IsNext, message.IsComplete, value);
        }

        private async ValueTask RunObserver(int streamId, RSocketStream stream, bool next, bool completed, (ReadOnlySequence<byte> Metadata, ReadOnlySequence<byte> Data) value)
        {
            if (next)
            {
                if (await stream.Credit.DecrementAsync())
                {
                    stream.OnNext(value);
                }
            }

            if (completed)
            {
                stream.OnCompleted();
                Dispatcher.RemoveStream(streamId);
            }
            else if (0 == stream.Credit.Count)
            {
                var initialRequest = options.GetInitialRequest(options.NextRequestCredit);

                stream.Credit.Add(initialRequest);

                await new RSocketProtocol.RequestN(streamId, initialRequest)
                    .Write(Pipe)
                    .FlushAsync(true, default);
            }
        }
    }
}