using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LibraProgramming.Serialization.Hessian;
using RSocket.Core;
using ShopMe.Application.Models;
using ShopMe.Application.Services;
using ShopMe.Models;
using ShopMe.Models.Services;

namespace ShopMe.Client.Services
{
    internal sealed class RemoteShopListProvider : ClientApiBase, IShopListProvider
    {
        private readonly RSocketClient client;

        public RemoteShopListProvider(RSocketClient client)
        {
            this.client = client;
        }

        public IAsyncEnumerable<ShopList> GetChanges(CancellationToken cancellationToken)
        {
            var methodInfo = typeof(IShopListApi).GetMethod(nameof(IShopListApi.GetAllListsAsync));
            var hessianCall = new HessianCall(methodInfo);
            ReadOnlySequence<byte> bytes;
            
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
                    var list = (ShopListInfo) serializer.ReadObject(stream);
                    return new ShopList(list.Title);
                },
                bytes,
                cancellationToken: cancellationToken
            );

            return enumerable;
        }
    }
}