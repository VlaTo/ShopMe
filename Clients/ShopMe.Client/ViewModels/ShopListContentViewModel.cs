using System.Diagnostics;
using Prism.Navigation;
using Xamarin.Forms;

namespace ShopMe.Client.ViewModels
{
    [QueryProperty(nameof(Id), "id")]
    public sealed class ShopListContentViewModel : ViewModelBase
    {
        private string id;

        public string Id
        {
            get => id;
            set
            {
                id = value;
                Debug.WriteLine($"Shop List Id: {id}");
            }
        }

        public ShopListContentViewModel(INavigationService navigationService)
            : base(navigationService)
        {
        }
    }
}