using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using RSocket.Core;
using System;
using System.Threading.Tasks;

namespace ShopMe.Web.Service.RSocket.DependencyInjection
{
    public static class EndpointRouteBuilderExtensions
    {
        public static RSocketEndpointConventionBuilder MapRSocket(
            this IEndpointRouteBuilder routeBuilder,
            string pattern)
        {
            return MapRSocketInternal(routeBuilder, RoutePatternFactory.Parse(pattern), Actions.None);
        }

        public static RSocketEndpointConventionBuilder MapRSocket(
            this IEndpointRouteBuilder routeBuilder,
            string pattern,
            Action<HttpConnectionDispatcherOptions> configureOptions)
        {
            return MapRSocketInternal(routeBuilder, RoutePatternFactory.Parse(pattern), configureOptions);
        }

        public static RSocketEndpointConventionBuilder MapRSocket(
            this IEndpointRouteBuilder routeBuilder,
            RoutePattern pattern)
        {
            return MapRSocketInternal(routeBuilder, pattern, Actions.None);
        }

        public static RSocketEndpointConventionBuilder MapRSocket(
            this IEndpointRouteBuilder routeBuilder,
            RoutePattern pattern,
            Action<HttpConnectionDispatcherOptions> configureOptions)
        {
            return MapRSocketInternal(routeBuilder, pattern, configureOptions);
        }

        private static RSocketEndpointConventionBuilder MapRSocketInternal(
            IEndpointRouteBuilder routeBuilder,
            RoutePattern pattern,
            Action<HttpConnectionDispatcherOptions> configureOptions)
        {
            var dispatcherOptions = new HttpConnectionDispatcherOptions();
            var conventionBuilder = new RSocketEndpointConventionBuilder();

            configureOptions.Invoke(dispatcherOptions);

            routeBuilder.Map(pattern, ProcessRSocketAsync);

            return conventionBuilder;
        }

        private static async Task ProcessRSocketAsync(HttpContext context)
        {
            var manager = context.RequestServices.GetRequiredService<RSocketServerManager>();
            var server = context.RequestServices.GetRequiredService<RSocketServer>();

            using (manager.RegisterServer(server))
            {
                await server.ConnectAsync(context.RequestAborted);
            }
        }

        private static class Actions
        {
            public static readonly Action<HttpConnectionDispatcherOptions> None;

            static Actions()
            {
                None = options => { };
            }
        }
    }
}