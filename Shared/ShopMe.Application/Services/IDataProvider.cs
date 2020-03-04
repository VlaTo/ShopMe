using System.Collections.Generic;
using System.Threading;
using ShopMe.Application.Models;

namespace ShopMe.Application.Services
{
    public interface IDataProvider
    {
        IAsyncEnumerable<ShopList> GetShopLists(CancellationToken cancellationToken);
    }
}