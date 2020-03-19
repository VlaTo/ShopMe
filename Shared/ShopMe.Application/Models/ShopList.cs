using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShopMe.Application.Models
{
    public class ShopList : ShopListDescription
    {
        public ShopList(long id, string title)
            : base(id, title)
        {
        }

        public IObservable<ShopListChanges> GetChanges(CancellationToken cancellationToken)
        {
            return System.Reactive.Linq.Observable.Create<ShopListChanges>((observer, cancel) =>
            {
                Task GetChanges()
                {
                    var changes = new ShopListChanges(
                        Id,
                        new[]
                        {
                            new ShopItem(1, "1"),
                            new ShopItem(2, "2"),
                            new ShopItem(3, "3"),
                            new ShopItem(4, "4"),
                            new ShopItem(5, "5"),
                        }
                    );

                    observer.OnNext(changes);
                    observer.OnCompleted();

                    return Task.CompletedTask;
                }

                return GetChanges();
            });
        }
    }
}