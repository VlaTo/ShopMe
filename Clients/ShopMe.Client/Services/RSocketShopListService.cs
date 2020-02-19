using LibraProgramming.Serialization.Hessian;
using RSocket.Core;
using ShopMe.Models;
using ShopMe.Models.Commands;
using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            var commandSerializer = new DataContractHessianSerializer(typeof(GetShopListsCommand));
            var command = commandSerializer.Serialize(new GetShopListsCommand
            {

            });

            return Observable.Create<ShopListDescription>(async observer =>
            {
                var responseSerializer = new DataContractHessianSerializer(typeof(ShopListDescription));
                var items = client.RequestStreamAsync(
                    result => (ShopListDescription) responseSerializer.Deserialize(result.data),
                    command,
                    cancellationToken: cancellationToken
                );

                await foreach (var item in items.WithCancellation(cancellationToken))
                {
                    observer.OnNext(item);
                }

                observer.OnCompleted();
            });
        }
    }
}