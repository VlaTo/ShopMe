using ShopMe.Models;
using ShopMe.Models.Services;
using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShopMe.Client.Services
{
    internal sealed class RSocketShopListService : IShopListService
    {
        private readonly IShopListApi client;

        public RSocketShopListService(IShopListApi client)
        {
            this.client = client;
        }

        public IObservable<ShopListInfo> GetLists(CancellationToken cancellationToken = default)
        {
            return Observable.Create<ShopListInfo>(async observer =>
            {
                var response = client.GetAllListsAsync();

                await foreach (var list in response.WithCancellation(cancellationToken))
                {
                    observer.OnNext(list);
                }

                observer.OnCompleted();
            });
        }
    }
}