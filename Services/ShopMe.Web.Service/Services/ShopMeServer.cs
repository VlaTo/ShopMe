using LibraProgramming.Serialization.Hessian;
using MediatR;
using Microsoft.Extensions.Logging;
using RSocket.Core;
using ShopMe.Models;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShopMe.Web.Service.Services
{
    internal sealed class ShopMeServer : RSocketServer
    {
        private readonly IMediator mediator;

        public ShopMeServer(
            IMediator mediator,
            IRSocketTransport transport,
            RSocketOptions options,
            ILogger<ShopMeServer> logger)
            : base(transport, options, logger)
        {
            this.mediator = mediator;

            Stream(
                request => request,
                request => AsyncEnumerable.Create(ShopLists),
                result => (result, ReadOnlySequence<byte>.Empty)
            );
        }

        private static async IAsyncEnumerator<ReadOnlySequence<byte>> ShopLists(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var serializer = new DataContractHessianSerializer(typeof(ShopListDescription));
            var lists = new[]
            {
                "Lorem ipsum dolor sit amet",
                "Consectetur adipiscing elit",
                "Sed do eiusmod tempor incididunt"
            };

            foreach (var list in lists)
            {
                var description = new ShopListDescription
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