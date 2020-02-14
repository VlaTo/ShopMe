using RSocket.Core.Extensions;
using RSocket.Core.Internal;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace RSocket.Core
{
    public class RSocketClient : RSocket
    {
        private ValueTask handler;
        private readonly RSocketProtocolHandler protocolHandler;

        public RSocketClient(IRSocketTransport transport, RSocketOptions options = default)
            : base(transport, options)
        {
            protocolHandler = new ClientProtocolHandler(Dispatcher, Transport.Output, options);
        }

        public async ValueTask ConnectAsync(
            CancellationToken cancellationToken,
            ReadOnlySequence<byte> data = default,
            ReadOnlySequence<byte> metadata = default)
        {
            await Transport.StartAsync(cancellationToken);

            handler = base.ConnectAsync(cancellationToken);

            await SetupAsync(
                Options.KeepAlive,
                Options.Lifetime,
                Options.MetadataMimeType,
                Options.DataMimeType,
                data,
                metadata,
                cancellationToken
            );
        }

        public ValueTask<T> RequestResponseAsync<T>(
            Func<(ReadOnlySequence<byte> data, ReadOnlySequence<byte> metadata), T> resultMapper,
            ReadOnlySequence<byte> data,
            ReadOnlySequence<byte> metadata = default,
            CancellationToken cancellationToken = default) =>
            new Receiver<T>(stream =>
                    {
                        var requestCredit = new ClientRequestCredit(1);
                        var streamId = Dispatcher.DispatchStream(stream, requestCredit);
                        return new RSocketProtocol.RequestResponse(streamId, data, metadata)
                            .Write(Transport.Output)
                            .FlushAsync(true, cancellationToken);
                    },
                    resultMapper.Invoke)
                .ExecuteAsync(cancellationToken);

        public IAsyncEnumerable<T> RequestStreamAsync<T>(
            Func<(ReadOnlySequence<byte> data, ReadOnlySequence<byte> metadata), T> resultMapper,
            ReadOnlySequence<byte> data,
            ReadOnlySequence<byte> metadata = default,
            int initialRequest = RSocketOptions.DefaultInitialRequest,
            CancellationToken cancellationToken = default) =>
            new Receiver<T>(stream =>
                {
                    var credit = Options.GetInitialRequest(initialRequest);
                    var requestCredit = new ClientRequestCredit(credit);
                    var streamId = Dispatcher.DispatchStream(stream, requestCredit);

                    return new RSocketProtocol.RequestStream(streamId, data, metadata, credit)
                        .Write(Transport.Output)
                        .FlushAsync(true, cancellationToken);
                },
                resultMapper.Invoke
            );

        public IAsyncEnumerable<T> RequestChannelAsync<TSource, T>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, ReadOnlySequence<byte>> sourceMapper,
            Func<(ReadOnlySequence<byte> data, ReadOnlySequence<byte> metadata), T> resultMapper,
            ReadOnlySequence<byte> data = default,
            ReadOnlySequence<byte> metadata = default,
            int initialRequest = RSocketOptions.DefaultInitialRequest,
            CancellationToken cancellationToken = default) =>
            new Receiver<TSource, T>(async stream =>
                {
                    var credit = Options.GetInitialRequest(initialRequest);
                    var requestCredit = new ClientRequestCredit(credit);
                    var streamId = Dispatcher.DispatchStream(stream, requestCredit);

                    await new RSocketProtocol.RequestChannel(streamId, data, metadata, credit)
                        .Write(Transport.Output)
                        .FlushAsync(true, cancellationToken);

                    var channel = new ChannelHandler(Dispatcher, streamId, protocolHandler.Pipe, cancellationToken);

                    return channel;
                },
                source,
                _ => (default, sourceMapper.Invoke(_)),
                resultMapper.Invoke
            );

        public ValueTask RequestFireAndForget(
            ReadOnlySequence<byte> data = default,
            ReadOnlySequence<byte> metadata = default,
            CancellationToken cancellationToken = default) =>
            new Receiver<bool>(async stream =>
                    {
                        var requestCredit = new ClientRequestCredit(1);
                        var streamId = Dispatcher.DispatchStream(stream, requestCredit);

                        await new RSocketProtocol.RequestFireAndForget(streamId, data, metadata)
                            .Write(Transport.Output)
                            .FlushAsync(true, default);
                    },
                    null)
                .RunAsync(cancellationToken);

        protected override ValueTask ProcessAsync(CancellationToken cancellationToken = default) =>
            RSocketProtocol.HandleAsync(protocolHandler, Transport.Input, cancellationToken);

        private class ChannelHandler : IRSocketChannel
        {
            private readonly RSocketStreamDispatcher dispatcher;
            private readonly int streamId;
            private readonly PipeWriter pipe;
            private readonly CancellationToken cancellationToken;

            public ChannelHandler(RSocketStreamDispatcher dispatcher, int streamId, PipeWriter pipe, CancellationToken cancellationToken)
            {
                this.dispatcher = dispatcher; 
                this.streamId = streamId;
                this.pipe = pipe;
                this.cancellationToken = cancellationToken;
            }

            public ValueTask Send((ReadOnlySequence<byte> metadata, ReadOnlySequence<byte> data) value)
            {
                if (false == dispatcher.HasStream(streamId))
                {
                    throw new InvalidOperationException("Channel is closed");
                }

                return new RSocketProtocol.Payload(streamId, value.data, value.metadata, next: true)
                    .Write(pipe)
                    .FlushAsync(true, cancellationToken);
            }
        }
    }
}