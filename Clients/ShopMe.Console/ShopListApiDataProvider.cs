using RSocket.Core;
using ShopMe.Application.Models;
using ShopMe.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ShopMe.Console
{
    internal sealed class ShopListApiDataProvider : ClientApiBase, IDataProvider
    {
        private readonly RSocketClient client;

        public ShopListApiDataProvider(RSocketClient client)
        {
            this.client = client;
        }

        public IAsyncEnumerable<ShopListDescription> GetShopLists(CancellationToken cancellationToken)
        {
            return AsyncEnumerable.Empty<ShopListDescription>();

            /*var methodInfo = typeof(IShopListApi).GetMethod(nameof(IShopListApi.GetAllListsAsync));
            var hessianCall = new HessianCall(methodInfo);
            var bytes = ReadOnlySequence<byte>.Empty;
            using (var stream = new MemoryStream())
            {
                hessianCall.WriteCall(stream);
                bytes = new ReadOnlySequence<byte>(stream.ToArray());
            }
            var serializer = new DataContractHessianSerializer(typeof(ShopListInfo));
            var enumerable = client.RequestStreamAsync(
                result =>
                {
                    using var stream = new MemoryStream(result.data.ToArray());
                    return (ShopListInfo) serializer.ReadObject(stream);
                },
                bytes
            );
            return enumerable;*/
        }
    }
}