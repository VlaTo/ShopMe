using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ShopMe.Application.Models;
using ShopMe.Application.Services;

namespace ShopMe.Client.Services
{
    internal sealed class ShopListProvider : IShopListProvider
    {
        public static readonly IShopListProvider Empty;

        static ShopListProvider()
        {
            Empty = new ShopListProvider();
        }

        private ShopListProvider()
        {
        }

        public IAsyncEnumerable<ShopList> GetChanges(CancellationToken cancellationToken)
        {
            return AsyncEnumerable.Empty<ShopList>();
        }
    }
}