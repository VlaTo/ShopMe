using RSocket.Core.Extensions;
using RSocket.Core.Internal;
using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

using IRSocketStream = System.IObserver<(System.Buffers.ReadOnlySequence<byte> metadata, System.Buffers.ReadOnlySequence<byte> data)>;

namespace RSocket.Core
{
    public interface IRSocketChannel
    {
        ValueTask Send((ReadOnlySequence<byte> metadata, ReadOnlySequence<byte> data) value);
    }

    /*public interface IRSocketStreamSerializer
    {
        ReadOnlySequence<byte> Serialize<TData>(TData item);
    }

    public interface IRSocketStreamDeserializer
    {
        TData Deserialize<TData>(ReadOnlySequence<byte> bytes);
    }*/

    /*
    public class RSocketStream<TData, TMetadata> : IRSocketStream
    {
        private readonly IObserver<(TData, TMetadata)> observer;
        private readonly Func<ReadOnlySequence<byte>, TData> fromData;
        private readonly Func<ReadOnlySequence<byte>, TMetadata> fromMetadata;
        private readonly TaskCompletionSource tcs;

        public int ResponseCredit
        {
            get; 
            set;
        }

        public RSocketStream(
            IObserver<(TData, TMetadata)> observer,
            int responseCredit,
            Func<ReadOnlySequence<byte>, TData> fromData,
            Func<ReadOnlySequence<byte>, TMetadata> fromMetadata = null)
        {
            this.observer = observer;
            this.fromData = fromData;
            this.fromMetadata = fromMetadata;

            ResponseCredit = responseCredit;
        }

        void IRSocketStream.OnCompleted()
        {
            observer.OnCompleted();
            tcs.SetCompleted();
        }

        void IRSocketStream.OnError(Exception error) => throw new NotImplementedException();

        void IRSocketStream.OnNext((ReadOnlySequence<byte> metadata, ReadOnlySequence<byte> data) value) =>
            observer.OnNext((fromData.Invoke(value.data), fromMetadata.Invoke(value.metadata)));

        public Task AsTask() => tcs.Task;
    }
    */

    public class RSocketStream : IRSocketStream
    {
        private readonly IRSocketStream observer;

        public IRequestCredit Credit
        {
            get; 
        }

        public RSocketStream(IRSocketStream observer, IRequestCredit credit)
        {
            this.observer = observer;

            Credit = credit;
        }

        public void OnCompleted() => observer.OnCompleted();

        public void OnError(Exception error) => throw new NotImplementedException();

        public void OnNext((ReadOnlySequence<byte> metadata, ReadOnlySequence<byte> data) value) =>
            observer.OnNext(value);
    }

    public abstract class RSocket
    {
        protected RSocketOptions Options
        {
            get;
        }

        internal IRSocketTransport Transport
        {
            get;
        }

        public RSocketStreamDispatcher Dispatcher
        {
            get;
        }

        protected RSocket(IRSocketTransport transport, RSocketOptions options = default)
        {
            Dispatcher = new RSocketStreamDispatcher();
            Transport = transport;
            Options = options ?? RSocketOptions.Default;
        }

        public virtual ValueTask ConnectAsync(CancellationToken cancellationToken) => ProcessAsync(cancellationToken);

        public ValueTask SetupAsync(
            TimeSpan keepAlive,
            TimeSpan lifetime,
            string metadataMimeType = null,
            string dataMimeType = null,
            ReadOnlySequence<byte> data = default,
            ReadOnlySequence<byte> metadata = default,
            CancellationToken cancellationToken = default)
        {
            return new RSocketProtocol.Setup(keepAlive, lifetime, metadataMimeType, dataMimeType, data, metadata)
                .Write(Transport.Output)
                .FlushAsync(true, cancellationToken);
        }

        protected abstract ValueTask ProcessAsync(CancellationToken cancellationToken = default);
    }
}