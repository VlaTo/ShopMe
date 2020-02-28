using System;
using System.Threading;
using ShopMe.Models;

namespace ShopMe.Client.Services
{
    public interface IShopListService
    {
        IObservable<ShopListInfo> GetLists(CancellationToken cancellationToken = default);
    }
}