using Microsoft.EntityFrameworkCore;
using ShopMe.Application.Models;
using ShopMe.Application.Services;
using ShopMe.Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

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

            yield return GetShopListDescription(1);
            yield return GetShopListDescription(2);
            yield return GetShopListDescription(3);
            yield return GetShopListDescription(4);
            yield return GetShopListDescription(5);
            yield return GetShopListDescription(6);
            yield return GetShopListDescription(7);

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

        public Task<ShopListDescription> GetShopListAsync(long id, CancellationToken cancellationToken)
        {
            return Task.FromResult(GetShopListDescription(id));
        }

        private static ShopListDescription GetShopListDescription(long id)
        {
            switch (id)
            {
                case 1:
                {
                    return new ShopListDescription(id, "Lorem ipsum dolor sit amet");
                }

                case 2:
                {
                    return new ShopListDescription(id, "Consectetur adipiscing elit");
                }

                case 3:
                {
                    return new ShopListDescription(id, "Aenean ullamcorper congue arcu");
                }

                case 4:
                {
                    return new ShopListDescription(id, "Duis nec turpis elit");
                }

                case 5:
                {
                    return new ShopListDescription(id, "Mauris ullamcorper pretium");
                }

                case 6:
                {
                    return new ShopListDescription(id, "Malesuada fames ac ante ipsum");
                }

                case 7:
                {
                    return new ShopListDescription(id, "Pharetra sollicitudin");
                }

                default:
                {
                    return new ShopListDescription(id, "Quisque lobortis");
                }
            }
        }
    }
}