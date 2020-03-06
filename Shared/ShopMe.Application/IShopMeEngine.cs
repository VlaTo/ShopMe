using ShopMe.Application.Models;
using System;
using System.Threading;

namespace ShopMe.Application
{
    public interface IShopMeEngine
    {
        IObservable<ShopListDescriptionChanges> GetActualLists(CancellationToken cancellationToken);
    }
}
