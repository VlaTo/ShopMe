using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShopMe.Web.Service.RSocket.DependencyInjection;
using ShopMe.Web.Service.Services;

namespace ShopMe.Web.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "ShopMe Server";

            var host = WebHost
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, configuration) =>
                {
                    var environment = context.HostingEnvironment;

                    configuration
                        .SetBasePath(environment.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true);

                    configuration.AddEnvironmentVariables();

                    if (null != args)
                    {
                        configuration.AddCommandLine(args);
                    }
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddMediatR(new[]
                        {
                            Assembly.GetEntryAssembly()
                        });

                    services
                        .AddLogging(logging =>
                        {
                            logging.AddConsole().AddDebug();
                        })
                        .AddRouting()
                        .AddConnections()
                        .AddWebSockets(options =>
                        {
                            options.KeepAliveInterval = TimeSpan.FromMinutes(2.0d);
                        })
                        .AddRSocketServer<ShopMeServer>(options =>
                        {
                            options.AddWebSocket();
                        });
                })
                .Configure((context, app) =>
                {
                    app
                        .UseRouting()
                        .UseWebSockets();

                    if (context.HostingEnvironment.IsDevelopment())
                    {

                    }

                    app
                        .UseEndpoints(routes =>
                        {
                            routes.MapRSocket("/api");
                        });
                })
                .Build();

            host.Run();
        }
    }
}
