using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibraProgramming.Serialization.Hessian;
using MediatR;
using Microsoft.Extensions.Logging;
using RSocket.Core;
using ShopMe.Models.Services;

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
                request =>
                {
                    //var bytes = ReadOnlySequence<byte>.Empty;
                    /*var methodInfo = typeof(IShopListApi).GetMethod(nameof(IShopListApi.GetAllListsAsync));
                    var hessianCall = new HessianCall(methodInfo);
                    using (var stream = new MemoryStream(request.Data.ToArray()))
                    {
                        hessianCall.ReadCall(stream);
                        bytes = new ReadOnlySequence<byte>(stream.ToArray());
                    }*/
                    return request.Data;
                },
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