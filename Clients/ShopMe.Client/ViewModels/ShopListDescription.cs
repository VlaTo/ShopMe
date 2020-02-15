using Prism.Mvvm;

namespace ShopMe.Client.ViewModels
{
    public class ShopListDescription : BindableBase
    {
        private string title;

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public ShopListDescription()
        {
        }
    }
}