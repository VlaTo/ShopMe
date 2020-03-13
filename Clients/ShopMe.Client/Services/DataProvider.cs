using Microsoft.EntityFrameworkCore;
using ShopMe.Application.Models;
using ShopMe.Application.Services;
using ShopMe.Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ShopMe.Client.Services
{
    internal sealed class DataProvider : IDataProvider
    {
        private readonly ShopMeDbContext context;

        public DataProvider(ShopMeDbContext context)
        {
            this.context = context;
        }

        public async IAsyncEnumerable<ShopListDescription> GetShopLists([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var shopLists = Array.Empty<Data.Models.ShopList>();

            using (var transaction = await context.Database.BeginTransactionAsync(cancellationToken))
            {
                shopLists = await context.ShopLists.AsNoTracking()
                    .Where(list => list.IsActive)
                    .OrderBy(list => list.Created)
                    .ToArrayAsync(cancellationToken);
            }

            foreach (var shopList in shopLists)
            {
                yield return new ShopListDescription(shopList.Id, shopList.Title);
            }

            yield return new ShopListDescription(1, "Lorem Ipsum");
            yield return new ShopListDescription(2, "Hsjhfc sjkdkjshfch sdfc");
            yield return new ShopListDescription(3, "Udfjhcsj hgefhsd sdfcjsdjf");
        }
    }
}