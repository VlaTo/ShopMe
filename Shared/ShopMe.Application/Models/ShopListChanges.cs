using System;

namespace ShopMe.Application.Models
{
    public sealed class ShopListChanges
    {
        public long Id
        {
            get;
        }

        public ShopItem[] Added
        {
            get;
        }

        public ShopItem[] Updated
        {
            get;
        }

        public long[] Deleted
        {
            get;
        }

        public ShopListChanges(long id, ShopItem[] added, ShopItem[] updated, long[] deleted)
        {
            Id = id;
            Added = added ?? Array.Empty<ShopItem>();
            Updated = updated ?? Array.Empty<ShopItem>();
            Deleted = deleted ?? Array.Empty<long>();
        }
    }
}