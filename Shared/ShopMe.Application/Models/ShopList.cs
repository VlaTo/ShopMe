using System;

namespace ShopMe.Application.Models
{
    public class ShopList : ShopListDescription
    {
        public ShopItem[] Items
        {
            get;
        }

        public ShopList(long id, string title, ShopItem[] items)
            : base(id, title)
        {
            Items = items ?? Array.Empty<ShopItem>();
        }
    }
}