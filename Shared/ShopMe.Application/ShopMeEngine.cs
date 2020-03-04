using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShopMe.Application.Models;
using ShopMe.Application.Observable.Collections;
using ShopMe.Application.Services;

namespace ShopMe.Application
{
    public sealed class ShopMeEngine : IShopMeEngine
    {
        private readonly IDataProvider dataProvider;
        private readonly IChangesProvider changesProvider;

        public ShopMeEngine(IDataProvider dataProvider, IChangesProvider changesProvider)
        {
            this.dataProvider = dataProvider;
            this.changesProvider = changesProvider;
        }

        public async Task<IObservableCollection<ShopList>> GetActualListsAsync(CancellationToken cancellationToken)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var result = new Observable.Collections.ObservableCollection<ShopList>();

            await foreach (var list in dataProvider.GetShopLists(cancellationTokenSource.Token))
            {
                result.Add(list);
            }

            return result;
        }
    }
}