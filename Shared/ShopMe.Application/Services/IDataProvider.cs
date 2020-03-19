using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShopMe.Application.Models;

namespace ShopMe.Application.Services
{
    public interface IDataProvider
    {
        IAsyncEnumerable<ShopListDescription> GetShopLists(CancellationToken cancellationToken);

        Task<ShopListDescription> GetShopListAsync(long id, CancellationToken cancellationToken);
    }
}