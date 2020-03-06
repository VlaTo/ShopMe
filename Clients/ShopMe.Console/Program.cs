using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RSocket.Core;
using RSocket.Core.Transports;
using ShopMe.Application;
using ShopMe.Application.Observable.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace ShopMe.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var loggerFactory = new NullLoggerFactory();
            var transport = new ClientWebSocketTransport("ws://localhost:5000/api", loggerFactory.CreateLogger<WebSocketTransport>());
            var client = new RSocketClient(transport);
            var dataProvider = new ShopListApiDataProvider(client);
            //var invoker = new DefaultCallInvoker(null);
            var engine = new ShopMeEngine(dataProvider, null);
            
            RetrieveListAsync(client, engine).Wait();

            System.Console.WriteLine("Press key to close");
            System.Console.ReadLine();
        }

        private static async Task RetrieveListAsync(RSocketClient client, IShopMeEngine engine)
        {
            await client.ConnectAsync(default);

            //System.Console.WriteLine("Press key");
            //System.Console.ReadLine();

            var result = engine.GetActualLists(CancellationToken.None);

            /*result.Subscribe(
                added => { System.Console.WriteLine($"List title: {added.Title}"); },
                removed => { System.Console.WriteLine($"List title: {removed.Id}"); },
                changed => { System.Console.WriteLine($"List title: {changed.Title}"); }
            );*/

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
