using ShopMe.Application.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShopMe.Application
{
    public interface IShopMeEngine
    {
        IObservable<ShopListDescriptionChanges> GetActualLists(CancellationToken cancellationToken);

        Task<ShopList> GetShopListAsync(long id, CancellationToken cancellationToken);
    }
}
