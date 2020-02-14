using Microsoft.Extensions.Logging;
using RSocket.Core.Internal;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RSocket.Core
{
    public class RSocketServer : RSocket, IRSocketScheduler
    {
        public Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata),
            IAsyncEnumerable<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata)>> Streamer
        {
            get;
            private set;
        }

        public Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata),
            ValueTask<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata)>> Responder
        {
            get;
            private set;
        }

        public Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata), ValueTask> Executor
        {
            get;
            private set;
        }

        public Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata),
            IObservable<(ReadOnlySequence<byte> data, ReadOnlySequence<byte> metadata)>,
            IAsyncEnumerable<(ReadOnlySequence<byte> data, ReadOnlySequence<byte> metadata)>> Channeler
        {
            get;
            private set;
        }

        protected ILogger Logger
        {
            get;
        }

        public RSocketServer(IRSocketTransport transport, RSocketOptions options, ILogger logger)
            : base(transport, options)
        {
            Logger = logger;
            Executor = _ => throw new NotImplementedException();
            Responder = _ => throw new NotImplementedException();
            Streamer = _ => throw new NotImplementedException();
            Channeler = (request, incoming) => throw new NotImplementedException();
        }

        public override async ValueTask ConnectAsync(CancellationToken cancellationToken)
        {
            await Transport.StartAsync(cancellationToken);
            await base.ConnectAsync(cancellationToken);
        }

        protected void Respond<TRequest, TResult>(
            Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata), TRequest> requestTransform,
            Func<TRequest, IAsyncEnumerable<TResult>> producer,
            Func<TResult, (ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata)> resultTransform) =>
            Responder = request => producer
                .Invoke(requestTransform.Invoke(request))
                .Select(resultTransform)
                .FirstAsync();

        protected void Stream<TRequest, TResult>(
            Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata), TRequest> requestTransform,
            Func<TRequest, IAsyncEnumerable<TResult>> producer,
            Func<TResult, (ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata)> resultTransform) =>
            Streamer = request => producer
                .Invoke(requestTransform.Invoke(request))
                .Select(resultTransform);

        protected void Channel<TRequest, TIncoming, TOutgoing>(
            Func<TRequest, IObservable<TIncoming>, IAsyncEnumerable<TOutgoing>> pipeline,
            Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata), TRequest> requestTransform,
            Func<(ReadOnlySequence<byte> Metadata, ReadOnlySequence<byte> Data), TIncoming> incomingTransform,
            Func<TOutgoing, (ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata)> outgoingTransform) =>
            Channeler = (request, incoming) =>
            {
                var source = pipeline.Invoke(
                    requestTransform.Invoke(request),
                    incoming.Select(incomingTransform.Invoke)
                );
                return source.Select(outgoingTransform.Invoke);
            };

        protected void Execute<TRequest>(
            Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata), TRequest> requestTransform,
            Func<TRequest, ValueTask> action) =>
            Executor = request => action.Invoke(requestTransform.Invoke(request));

        protected override ValueTask ProcessAsync(CancellationToken cancellationToken = default)
        {
            var protocolHandler = new ServerProtocolHandler(Dispatcher, this, Transport.Output);
            return RSocketProtocol.HandleAsync(protocolHandler, Transport.Input, cancellationToken);
        }

        async ValueTask IRSocketScheduler.Schedule(int streamId, Func<int, CancellationToken, Task> operation, CancellationToken cancellationToken)
        {
            var task = operation.Invoke(streamId, cancellationToken);

            if (false == task.IsCompleted)
            {
                await task.ConfigureAwait(false);
            }
            else
            {
                await task;
            }
        }
    }
}