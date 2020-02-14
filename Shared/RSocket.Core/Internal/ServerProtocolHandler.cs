using RSocket.Core.Extensions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace RSocket.Core.Internal
{
    internal sealed class ServerProtocolHandler : RSocketProtocolHandler
    {
        private readonly IRSocketScheduler scheduler;

        /*public Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata),
            IAsyncEnumerable<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata)>> Streamer
        {
            get; 
            set;
        }

        public Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata),
            ValueTask<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata)>> Responder
        {
            get; 
            set;
        }

        public Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata), ValueTask> Executor
        {
            get; 
            set;
        }

        public Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata),
            IObservable<(ReadOnlySequence<byte> data, ReadOnlySequence<byte> metadata)>,
            IAsyncEnumerable<(ReadOnlySequence<byte> data, ReadOnlySequence<byte> metadata)>> Channeler
        {
            get;
            set;
        }*/

        public ServerProtocolHandler(
            RSocketStreamDispatcher dispatcher,
            IRSocketScheduler scheduler,
            PipeWriter pipe)
            : base(dispatcher, 0, pipe)
        {
            this.scheduler = scheduler;

            /*Executor = _ => throw new NotImplementedException();
            Responder = _ => throw new NotImplementedException();
            Streamer = _ => throw new NotImplementedException();
            Channeler = (request, incoming) => throw new NotImplementedException();*/
        }

        public override ValueTask SetupAsync(in RSocketProtocol.Setup message)
        {
            //TODO: implement setup here
            return new ValueTask();
        }

        public override ValueTask RequestResponseAsync(in RSocketProtocol.RequestResponse message)
        {
            var data = message.Data;
            var metadata = message.Metadata;

            scheduler.Schedule(message.StreamId, async (streamId, cancellationToken) =>
            {
                var response = await scheduler.Responder.Invoke((data, metadata));
                await new RSocketProtocol.Payload(streamId, response.Data, response.Metadata, next: true,
                        complete: true)
                    .Write(Pipe)
                    .FlushAsync(true, cancellationToken);
            });

            return new ValueTask();
        }

        public override ValueTask RequestStreamAsync(in RSocketProtocol.RequestStream message)
        {
            var data = message.Data;
            var metadata = message.Metadata;
            var requestCredit = new ServerRequestCredit(message.InitialRequest);

            scheduler.Schedule(message.StreamId, async (streamId, cancellationToken) =>
            {
                Dispatcher.DispatchStream(streamId, null, requestCredit);

                var source = scheduler.Streamer.Invoke((data, metadata));

                await foreach (var value in source.WithCancellation(cancellationToken))
                {
                    if (false == await requestCredit.DecrementAsync())
                    {
                        break;
                    }

                    await new RSocketProtocol.Payload(streamId, value.Data, value.Metadata, next: true)
                        .Write(Pipe)
                        .FlushAsync(true, cancellationToken);
                }

                await new RSocketProtocol.Payload(streamId, complete: true)
                    .Write(Pipe)
                    .FlushAsync(true, cancellationToken);
            });

            return new ValueTask();
        }

        public override ValueTask RequestChannelAsync(in RSocketProtocol.RequestChannel message)
        {
            var data = message.Data;
            var metadata = message.Metadata;
            var initialRequest = message.InitialRequest;

            scheduler.Schedule(message.StreamId, async (streamId, cancellationToken) =>
            {
                var requestCredit = new ServerRequestCredit(initialRequest);
                var incoming = Observable.Create<(ReadOnlySequence<byte> metadata, ReadOnlySequence<byte> data)>(
                    observer =>
                    {
                        Dispatcher.DispatchStream(streamId, observer, requestCredit);
                        //NOTE: return empty disposable meaning nothing to dispose
                        return Disposable.Empty;
                    });

                var channel = scheduler.Channeler.Invoke((data, metadata), incoming);

                await foreach (var value in channel.WithCancellation(cancellationToken))
                {
                    if (false == await requestCredit.DecrementAsync())
                    {
                        break;
                    }

                    await new RSocketProtocol.Payload(streamId, value.data, value.metadata, next: true)
                        .Write(Pipe)
                        .FlushAsync(true, cancellationToken);
                }

                await new RSocketProtocol.Payload(streamId, complete: true)
                    .Write(Pipe)
                    .FlushAsync(true, cancellationToken);
            });

            return new ValueTask();
        }

        public override ValueTask RequestFireAndForgetAsync(in RSocketProtocol.RequestFireAndForget message)
        {
            var data = message.Data;
            var metadata = message.Metadata;

            scheduler.Schedule(
                message.StreamId,
                async (streamId, cancellationToken) => await scheduler.Executor.Invoke((data, metadata))
            );

            return new ValueTask();
        }

        public override ValueTask RequestNAsync(in RSocketProtocol.RequestN message)
        {
            if (false == Dispatcher.TryGetStream(message.StreamId, out var stream))
            {
                throw new KeyNotFoundException();
            }

            stream.Credit.Add(message.RequestNumber);

            return new ValueTask();
        }

        public override ValueTask PayloadAsync(in RSocketProtocol.Payload message)
        {
            var streamId = message.StreamId;

            if (false == Dispatcher.TryGetStream(streamId, out var stream))
            {
                throw new KeyNotFoundException();
            }

            if (message.IsNext)
            {
                stream.OnNext((message.Metadata, message.Data));
            }

            if (message.IsComplete)
            {
                stream.OnCompleted();
                Dispatcher.RemoveStream(streamId);
            }

            return new ValueTask();
        }
    }
}