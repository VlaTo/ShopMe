using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RSocket.Core;
using System;

namespace ShopMe.Web.Service.RSocket.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRSocketServer<TServer>(this IServiceCollection services, Action<IRSocketServerConfigurationBuilder> configurator)
            where TServer : RSocketServer
        {
            services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddTransient(provider => GetConfiguration(provider, configurator));
            services.TryAddTransient(provider =>
            {
                var configuration = provider.GetRequiredService<RSocketServerConfiguration>();
                return configuration.Transport;
            });
            services.TryAddTransient(provider =>
            {
                var configuration = provider.GetRequiredService<RSocketServerConfiguration>();
                return configuration.Options;
            });
            services.TryAddSingleton<RSocketServerManager>();
            services.TryAddTransient<RSocketServer, TServer>();

            //services.TryAddSingleton<RSocketServer, TServer>();

            /*services.TryAddTransient(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var configuration = GetConfiguration(provider, configurator);

                return new RSocketServer(configuration.Transport, configuration.Options, loggerFactory.CreateLogger<TServer>());
            });*/

            return services;
        }

        private static RSocketServerConfiguration GetConfiguration(IServiceProvider provider, Action<IRSocketServerConfigurationBuilder> configuration)
        {
            var builder = new RSocketServerConfigurationBuilder(provider);

            configuration.Invoke(builder);

            return builder.Build();
        }
    }
}
