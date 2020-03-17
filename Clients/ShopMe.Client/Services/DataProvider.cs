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

            yield return new ShopListDescription(1, "Lorem ipsum dolor sit amet");
            yield return new ShopListDescription(2, "Consectetur adipiscing elit");
            yield return new ShopListDescription(3, "Aenean ullamcorper congue arcu");
            yield return new ShopListDescription(4, "Duis nec turpis elit");
            yield return new ShopListDescription(5, "Mauris ullamcorper pretium");

            /*
             * Aliquam imperdiet ac odio sed efficitur. Vestibulum tristique lorem at urna faucibus, id elementum eros volutpat.
             * Ut scelerisque ipsum id malesuada suscipit. Nunc at dui purus. Proin lobortis libero sit amet elit faucibus
             * vulputate.
             * Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Vivamus ut suscipit neque,
             * in consequat leo. Aenean et malesuada quam, vitae mollis lectus. Maecenas sed condimentum tortor.
             * Aenean non tortor non purus scelerisque porta sed sit amet massa. Nam at scelerisque nisi.
             * Proin dolor lectus, cursus eget tortor at, blandit tempor eros. Nam scelerisque leo a mi pretium euismod.
             * Morbi nisi dolor, aliquam et eros id, pharetra sollicitudin velit. Quisque lobortis justo tellus, eu mattis
             * elit scelerisque eget.
             * Lorem ipsum dolor sit amet, consectetur adipiscing elit. Interdum et malesuada fames ac ante ipsum primis
             * in faucibus.
             * Quisque et viverra diam. Aliquam non purus ornare nisl pellentesque porta non id justo.
             * Suspendisse ut tellus commodo, dictum erat at, rhoncus justo. Quisque eget ante faucibus, pulvinar quam sed,
             * eleifend velit.
             */
        }
    }
}