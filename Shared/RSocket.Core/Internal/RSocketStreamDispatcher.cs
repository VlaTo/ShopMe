using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

using IRSocketStream = System.IObserver<(System.Buffers.ReadOnlySequence<byte> metadata, System.Buffers.ReadOnlySequence<byte> data)>;

namespace RSocket.Core.Internal
{
    public sealed class RSocketStreamDispatcher
    {
        private readonly ConcurrentDictionary<int, RSocketStream> dispatcher;
        private int id;

        public RSocketStreamDispatcher()
        {
            dispatcher = new ConcurrentDictionary<int, RSocketStream>(EqualityComparer<int>.Default);
        }

        public bool HasStream(int streamId) => dispatcher.ContainsKey(streamId);

        public bool TryGetStream(int streamId, out RSocketStream stream) =>
            dispatcher.TryGetValue(streamId, out stream);

        public bool RemoveStream(int streamId)
        {
            if (dispatcher.TryRemove(streamId, out var observer))
            {
                observer.OnCompleted();
                return true;
            }

            return false;
        }

        public int DispatchStream(IRSocketStream stream, IRequestCredit requestCredit) =>
            DispatchStream(NewStreamId(), stream, requestCredit);

        public int DispatchStream(int streamId, IRSocketStream observer, IRequestCredit requestCredit)
        {
            dispatcher[streamId] = new RSocketStream(observer, requestCredit);
            return streamId;
        }

        private int NewStreamId() => Interlocked.Add(ref id, 2);
    }
}