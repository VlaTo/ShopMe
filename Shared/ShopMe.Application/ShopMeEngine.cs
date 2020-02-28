using System.Threading;
using System.Threading.Tasks;
using ShopMe.Application.Models;
using ShopMe.Application.Observable.Collections;
using ShopMe.Application.Services;

namespace ShopMe.Application
{
    public sealed class ShopMeEngine : IShopMeEngine
    {
        private readonly IShopListProvider local;
        private readonly IShopListProvider remote;

        public ShopMeEngine(IShopListProvider local, IShopListProvider remote)
        {
            this.local = local;
            this.remote = remote;
        }

        public async Task<IObservableCollection<ShopList>> GetActualLists()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var collection = new ObservableCollection<ShopList>();

            await foreach (var change in local.GetChanges(cancellationTokenSource.Token))
            {
                collection.Add(change);
            }

            return collection;
        }
    }
}