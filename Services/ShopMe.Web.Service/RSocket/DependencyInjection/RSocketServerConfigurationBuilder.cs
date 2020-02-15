using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RSocket.Core;
using RSocket.Core.Transports;
using ShopMe.Web.Service.RSocket.Transport;

namespace ShopMe.Web.Service.RSocket.DependencyInjection
{
    internal sealed class RSocketServerConfigurationBuilder : IRSocketServerConfigurationBuilder
    {
        private readonly IServiceProvider provider;
        private IRSocketTransport transport;

        public RSocketServerConfigurationBuilder(IServiceProvider provider)
        {
            this.provider = provider;
        }

        IRSocketServerConfigurationBuilder IRSocketServerConfigurationBuilder.AddWebSocket()
        {
            var contextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            var logger = provider.GetRequiredService<ILogger<WebSocketTransport>>();
            var options = new WebSocketOptions(TimeSpan.FromMinutes(5.0d));

            transport = new ServerWebSocketTransport(contextAccessor, options, logger);

            return this;
        }

        public RSocketServerConfiguration Build()
        {
            return new RSocketServerConfiguration(transport);
        }
    }
}