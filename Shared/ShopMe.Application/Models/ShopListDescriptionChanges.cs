using System;

namespace ShopMe.Application.Models
{
    public sealed class ShopListDescriptionChanges
    {
        public ShopListDescription[] Added
        {
            get;
        }

        public ShopListDescription[] Updated
        {
            get;
        }

        public long[] Deleted
        {
            get;
        }

        public ShopListDescriptionChanges(ShopListDescription[] added, ShopListDescription[] updated = null, long[] deleted = null)
        {
            Added = added;
            Updated = updated ?? Array.Empty<ShopListDescription>();
            Deleted = deleted ?? Array.Empty<long>();
        }
    }
}