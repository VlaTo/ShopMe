using System.Diagnostics.Contracts;

namespace ShopMe.Application.Models
{
    public sealed class ShopList
    {
        public long Id
        {
            get;
        }

        public string Title
        {
            get;
        }

        public ShopList(long id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}