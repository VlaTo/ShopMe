namespace ShopMe.Application.Models
{
    public sealed class ShopList
    {
        public string Title
        {
            get;
        }

        public ShopList(string title)
        {
            Title = title;
        }
    }
}