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

        public ShopListChanges(long id, ShopItem[] added, ShopItem[] updated = null, long[] deleted = null)
        {
            if (null == added)
            {
                throw new ArgumentNullException(nameof(added));
            }

            Id = id;
            Added = added;
            Updated = updated ?? Array.Empty<ShopItem>();
            Deleted = deleted ?? Array.Empty<long>();
        }
    }
}