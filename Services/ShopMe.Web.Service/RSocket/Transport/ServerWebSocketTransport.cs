using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RSocket.Core.Transports;
using ShopMe.Web.Service.Extensions;

namespace ShopMe.Web.Service.RSocket.Transport
{
    internal class ServerWebSocketTransport : WebSocketTransport
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly WebSocketOptions options;
        private ValueTask handler;
        private bool aborted;

        public ServerWebSocketTransport(
            IHttpContextAccessor httpContextAccessor,
            WebSocketOptions options,
            ILogger<WebSocketTransport> logger,
            PipeOptions outputPipeOptions = default,
            PipeOptions inputPipeOptions = default)
            : base(logger, outputPipeOptions, inputPipeOptions)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.options = options;
            aborted = false;
        }

        protected override async ValueTask ProcessRequestAsync(CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (false == httpContext.WebSockets.IsWebSocketRequest)
            {
                return;
            }

            Logger.ReportIncomingConnection(httpContext.Connection);

            var subProtocol = options.SubProtocolSelector?.Invoke(httpContext.WebSockets.WebSocketRequestedProtocols);

            using (var socket = await httpContext.WebSockets.AcceptWebSocketAsync(subProtocol))
            {
                Logger.ReportSocketAccepted(httpContext.Connection.Id, socket);

                try
                {
                    await Transport.ProcessSocketAsync(socket);
                }
                catch (ConnectionAbortedException exception)
                {
                    Logger.ReportSocketError(socket, exception);
                }
                catch (Exception exception)
                {
                    Logger.ReportSocketError(socket, exception);
                }
                finally
                {
                    Logger.ReportSocketClose(socket);
                }
            }
        }
    }
}