using System.Collections.Generic;

namespace ShopMe.Models.Services
{
    public interface IShopListApi
    {
        IAsyncEnumerable<ShopListInfo> GetAllListsAsync();
    }
}