using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using LibraProgramming.Serialization.Hessian;
using RSocket.Core;
using ShopMe.Application.Models;
using ShopMe.Application.Services;
using ShopMe.Transport.Models;

namespace ShopMe.Client.Services
{
    internal sealed class RemoteChangesProvider : ClientApiBase, IChangesProvider
    {
        private readonly RSocketClient client;

        public RemoteChangesProvider(RSocketClient client)
        {
            this.client = client;
        }

        /*public IAsyncEnumerable<ShopList> GetShopLists(CancellationToken cancellationToken)
        {
            --var methodInfo = typeof(IShopListApi).GetMethod(nameof(IShopListApi.GetAllListsAsync));
            var hessianCall = new HessianCall(methodInfo);
            ReadOnlySequence<byte> bytes;
            
            using (var stream = new MemoryStream())
            {
                hessianCall.WriteCall(stream);
                bytes = new ReadOnlySequence<byte>(stream.ToArray());
            }--

            var token = Guid.NewGuid().ToString("N");
            var bytes = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(token));
            
            var serializer = new DataContractHessianSerializer(typeof(AggregatedChanges));
            var enumerable = client.RequestStreamAsync(
                result =>
                {
                    using var stream = new MemoryStream(result.data.ToArray());
                    var list = (AggregatedChanges) serializer.ReadObject(stream);
                    return new ShopList(list.Title);
                },
                bytes,
                cancellationToken: cancellationToken
            );

            return enumerable;
        }*/
    }
}