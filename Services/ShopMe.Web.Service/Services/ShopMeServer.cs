using LibraProgramming.Serialization.Hessian;
using Microsoft.Extensions.Logging;
using RSocket.Core;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShopMe.Transport.Models;

namespace ShopMe.Web.Service.Services
{
    internal sealed class ShopMeServer : RSocketServer
    {
        public ShopMeServer(
            IRSocketTransport transport,
            RSocketOptions options,
            ILogger<ShopMeServer> logger)
            : base(transport, options, logger)
        {
            Stream(
                request => request,
                request => AsyncEnumerable.Create(ShopLists),
                result => (result, ReadOnlySequence<byte>.Empty)
            );
        }

        private static async IAsyncEnumerator<ReadOnlySequence<byte>> ShopLists(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var serializer = new DataContractHessianSerializer(typeof(AggregatedChanges));
            var lists = new[]
            {
                "Lorem ipsum dolor sit amet",
                "Consectetur adipiscing elit",
                "Sed do eiusmod tempor incididunt"
            };

            foreach (var list in lists)
            {
                var description = new ShopListInfo
                {
                    Title = list
                };

                using (var stream = new MemoryStream())
                {
                    serializer.WriteObject(stream, description);
                    yield return new ReadOnlySequence<byte>(stream.ToArray());
                }
            }
        }
    }
}