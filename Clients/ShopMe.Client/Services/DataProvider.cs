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

        public IAsyncEnumerable<ShopList> GetShopLists(CancellationToken cancellationToken)
        {
            return AsyncEnumerable.Create(cancel =>
            {
                static async IAsyncEnumerator<ShopList> Test()
                {
                    await Task.CompletedTask;

                    yield return new ShopList(1, "Lorem Ipsum");
                    yield return new ShopList(2, "Hsjhfc sjkdkjshfch sdfc");
                    yield return new ShopList(3, "Udfjhcsj hgefhsd sdfcjsdjf");
                }

                return Test();
            });
        }
    }
}