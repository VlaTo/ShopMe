using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace RSocket.Core
{
    public sealed class RSocketReactiveClient : RSocketClient
    {
        public RSocketReactiveClient(IRSocketTransport transport, RSocketOptions options = default) : base(transport, options)
        {
        }
        
        /*public IObservable<(TData Data, TMetadata Metadata)> RequestStream<TData, TMetadata, TRequestData,
            TRequestMetadata>(TRequestData data, TRequestMetadata metadata = default)
        {
            return Observable.Create<(TData, TMetadata)>(observer =>
            {
                var stream = new RSocketStream<TData, TMetadata>(observer, null, null);

                RequestStreamAsync(stream, data, metadata);

                return stream.Task;
            });
        }


        public Task RequestStreamAsync<T, M>(
            IRSocketStream stream,
            T data,
            M metadata = default,
            int initial = RSocketOptions.DefaultInitialRequest) =>
            RequestStream(
                stream,
                RequestDataSerializer.Serialize(data),
                RequestMetadataSerializer.Serialize(metadata),
                initial
            );*/

    }
}