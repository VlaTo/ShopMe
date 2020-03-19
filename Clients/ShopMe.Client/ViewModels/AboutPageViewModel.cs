using Xamarin.Forms;

namespace ShopMe.Client.ViewModels
{
    public sealed class AboutPageViewModel : ViewModelBase
    {
        public Command BackCommand
        {
            get;
        }

        public AboutPageViewModel()
        {
            BackCommand = new Command(() => { });
        }
    }
}