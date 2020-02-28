using System.Buffers;
using System.Collections.Generic;
using System.IO;
using LibraProgramming.Serialization.Hessian;
using RSocket.Core;
using ShopMe.Models;
using ShopMe.Models.Services;

namespace ShopMe.Console
{
    internal sealed class ShopListApiClient : ClientApiBase, IShopListApi
    {
        private readonly RSocketClient client;

        public ShopListApiClient(RSocketClient client)
        {
            this.client = client;
        }

        public IAsyncEnumerable<ShopListInfo> GetAllListsAsync()
        {
            var methodInfo = typeof(IShopListApi).GetMethod(nameof(IShopListApi.GetAllListsAsync));
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
            return enumerable;
        }
    }
}