using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using RSocket.Core;

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
                request => request.Data,
                request =>
                {
                    var command = Encoding.UTF8.GetString(request.ToArray());
                    return HandleCommand(command);
                },
                result =>
                {
                    var bytes = Encoding.UTF8.GetBytes(result);
                    return (new ReadOnlySequence<byte>(bytes), ReadOnlySequence<byte>.Empty);
                }
            );
        }

        private IAsyncEnumerable<string> HandleCommand(string command)
        {
            switch (command)
            {
                case "shoplists":
                {
                    return AsyncEnumerable.Create<string>(ShopLists);
                }

                default:
                {
                    return AsyncEnumerable.Empty<string>();
                }
            }
        }

        private async IAsyncEnumerator<string> ShopLists(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            yield return "Lorem ipsum dolor sit amet";
            yield return "Consectetur adipiscing elit";
            yield return "Sed do eiusmod tempor incididunt";
        }
    }
}