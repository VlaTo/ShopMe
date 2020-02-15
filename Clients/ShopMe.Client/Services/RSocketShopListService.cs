using System;
using System.Buffers;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RSocket.Core;
using ShopMe.Models;

namespace ShopMe.Client.Services
{
    internal sealed class RSocketShopListService : IShopListService
    {
        private readonly RSocketClient client;

        public RSocketShopListService(RSocketClient client)
        {
            this.client = client;
        }

        public IObservable<ShopListDescription> GetLists(CancellationToken cancellationToken = default)
        {
            const string command = "shoplists";
            var bytes = Encoding.UTF8.GetBytes(command);

            return Observable.Create<ShopListDescription>(async observer =>
            {
                var items = client.RequestStreamAsync(
                    result => Encoding.UTF8.GetString(result.data.ToArray()),
                    new ReadOnlySequence<byte>(bytes),
                    cancellationToken: cancellationToken
                );

                await foreach (var item in items.WithCancellation(cancellationToken))
                {
                    var model = new ShopListDescription(item);
                    observer.OnNext(model);
                }

                observer.OnCompleted();
            });
        }
    }
}