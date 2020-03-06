using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShopMe.Application.Models;
using ShopMe.Application.Services;

namespace ShopMe.Client.Services
{
    internal sealed class DataProvider : IDataProvider
    {
        public DataProvider()
        {
        }

        public IAsyncEnumerable<ShopListDescription> GetShopLists(CancellationToken cancellationToken)
        {
            return AsyncEnumerable.Create(cancel =>
            {
                static async IAsyncEnumerator<ShopListDescription> Test()
                {
                    await Task.CompletedTask;

                    yield return new ShopListDescription(1, "Lorem Ipsum");
                    yield return new ShopListDescription(2, "Hsjhfc sjkdkjshfch sdfc");
                    yield return new ShopListDescription(3, "Udfjhcsj hgefhsd sdfcjsdjf");
                }

                return Test();
            });
        }
    }
}