using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RSocket.Core.Internal
{
    internal interface IRSocketScheduler
    {
        Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata),
            IAsyncEnumerable<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata)>> Streamer
        {
            get; 
        }

        Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata),
            ValueTask<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata)>> Responder
        {
            get; 
        }

        Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata), ValueTask> Executor
        {
            get; 
        }

        Func<(ReadOnlySequence<byte> Data, ReadOnlySequence<byte> Metadata),
            IObservable<(ReadOnlySequence<byte> data, ReadOnlySequence<byte> metadata)>,
            IAsyncEnumerable<(ReadOnlySequence<byte> data, ReadOnlySequence<byte> metadata)>> Channeler
        {
            get;
        }

        ValueTask Schedule(int streamId, Func<int, CancellationToken, Task> operation, CancellationToken cancellationToken = default);
    }
}