using System.Diagnostics.Contracts;

namespace ShopMe.Application.Models
{
    public class ShopListDescription
    {
        public long Id
        {
            get;
        }

        public string Title
        {
            get;
        }

        public ShopListDescription(long id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}