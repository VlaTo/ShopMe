using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

using IRSocketStream = System.IObserver<(System.Buffers.ReadOnlySequence<byte> metadata, System.Buffers.ReadOnlySequence<byte> data)>;

namespace RSocket.Core
{
    internal class Receiver<T> : IAsyncEnumerable<T>
    {
        private readonly Func<IRSocketStream, ValueTask> subscriber;
        private readonly Func<(ReadOnlySequence<byte> data, ReadOnlySequence<byte> metadata), T> mapper;

        public Receiver(
            Func<IRSocketStream, ValueTask> subscriber,
            Func<(ReadOnlySequence<byte> data, ReadOnlySequence<byte> metadata), T> mapper)
        {
            this.subscriber = subscriber;
            this.mapper = mapper;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return CreateObservable()
                .Select(value => mapper.Invoke((value.data, value.metadata)))
                .ToAsyncEnumerable()
                .GetAsyncEnumerator(cancellationToken);
        }

        public async ValueTask<T> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var observable = CreateObservable();
            var value = await observable.ToTask(cancellationToken);

            return mapper.Invoke((value.data, value.metadata));
        }
        
        public async ValueTask RunAsync(CancellationToken cancellation = default)
        {
            IRSocketStream stream = null;
            var observable = Observable.Create<(ReadOnlySequence<byte> metadata, ReadOnlySequence<byte> data)>(async observer =>
            {
                stream = observer;
                await subscriber.Invoke(observer).ConfigureAwait(false);
                //NOTE: return empty disposable meaning nothing to dispose
                return Disposable.Empty;
            });

            var result = observable.ToTask(cancellation);

            stream.OnNext((ReadOnlySequence<byte>.Empty, ReadOnlySequence<byte>.Empty));
            stream.OnCompleted();

            await result;
        }

        private IObservable<(ReadOnlySequence<byte> metadata, ReadOnlySequence<byte> data)> CreateObservable()
        {
            return Observable.Create<(ReadOnlySequence<byte> metadata, ReadOnlySequence<byte> data)>(async observer =>
            {
                await subscriber.Invoke(observer).ConfigureAwait(false);
                //NOTE: return empty disposable meaning nothing to dispose
                return Disposable.Empty;
            });
        }
    }

    internal sealed class Receiver<TSource, T> : Receiver<T>
    {
        public Receiver(
            Func<IRSocketStream, ValueTask<IRSocketChannel>> subscriber,
            IAsyncEnumerable<TSource> source,
            Func<TSource, (ReadOnlySequence<byte> data, ReadOnlySequence<byte> metadata)> sourceMapper,
            Func<(ReadOnlySequence<byte> data, ReadOnlySequence<byte> metadata), T> resultMapper)
            : base(stream => Subscribe(subscriber.Invoke(stream), source, sourceMapper), resultMapper)
        {
        }

        private static async ValueTask Subscribe(
            ValueTask<IRSocketChannel> original,
            IAsyncEnumerable<TSource> source,
            Func<TSource, (ReadOnlySequence<byte> metadata, ReadOnlySequence<byte> data)> sourceMapper)
        {
            var channel = await original;

            await foreach (var item in source)
            {
                await channel.Send(sourceMapper.Invoke(item));
            }
        }
    }
}