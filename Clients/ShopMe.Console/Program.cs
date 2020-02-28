using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraProgramming.Serialization.Hessian;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RSocket.Core;
using RSocket.Core.Transports;
using ShopMe.Models;
using ShopMe.Models.Commands;
using ShopMe.Models.Services;

namespace ShopMe.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var loggerFactory = new NullLoggerFactory();
            var transport = new ClientWebSocketTransport("ws://localhost:5000/api", loggerFactory.CreateLogger<WebSocketTransport>());
            var client = new RSocketClient(transport);
            var service = new ShopListApiClient(client);
            //var invoker = new DefaultCallInvoker(null);
            
            RetrieveListAsync(service, client).Wait();

            System.Console.WriteLine("Press key to close");
            System.Console.ReadLine();
        }

        private static async Task RetrieveListAsync(IShopListApi api, RSocketClient client)
        {
            await client.ConnectAsync(default);

            //System.Console.WriteLine("Press key");
            //System.Console.ReadLine();

            await foreach (var description in api.GetAllListsAsync())
            {
                System.Console.WriteLine($"List title: {description.Title}");
            }

            System.Console.WriteLine("Done");

            /*const string command = "shoplists";
            var bytes = Encoding.UTF8.GetBytes(command);
            var method = new Method<GetShopListsCommand, List<ShopListDescription>>(MethodType.ServerStreaming, "GetShopLists");
            var call = invoker.CreateAsyncCall(method, "localhost", new CallOptions(), new GetShopListsCommand());
            var result = await call.ResponseAsync;

            var observable = Observable.Create<ShopListDescription>(async observer =>
            {
                var items = client.RequestStreamAsync(
                    result => Encoding.UTF8.GetString(result.data.ToArray()),
                    new ReadOnlySequence<byte>(bytes)
                );

                System.Console.WriteLine("Requesting list");

                await foreach (var item in items)
                {
                    var model = new ShopListDescription(item);
                    observer.OnNext(model);
                }

                observer.OnCompleted();
            });

            observable.Subscribe(description =>
                {
                    System.Console.WriteLine($"List title: {description.Title}");
                },
                () =>
                {
                    System.Console.WriteLine("Done");
                });*/
        }
    }
}
