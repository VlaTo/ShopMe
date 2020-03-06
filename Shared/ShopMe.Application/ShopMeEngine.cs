using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShopMe.Application.Models;
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

        public IObservable<ShopListDescriptionChanges> GetActualLists(CancellationToken cancellationToken)
        {
            return System.Reactive.Linq.Observable.Create<ShopListDescriptionChanges>((observer, cancel) =>
            {
                async Task GetChanges()
                {
                    var added = new List<ShopListDescription>();

                    await foreach (var list in dataProvider.GetShopLists(cancellationToken))
                    {
                        added.Add(list);
                    }

                    observer.OnNext(new ShopListDescriptionChanges(added.ToArray()));

                    observer.OnCompleted();
                }

                return GetChanges();
            });
        }
    }
}