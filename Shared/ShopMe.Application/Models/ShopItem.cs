namespace ShopMe.Application.Models
{
    public sealed class ShopItem
    {
        public long Id
        {
            get;
        }

        public string Title
        {
            get;
        }

        public ShopItem(long id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}