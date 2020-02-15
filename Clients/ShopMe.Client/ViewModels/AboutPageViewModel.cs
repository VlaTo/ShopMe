using Prism.Commands;
using Prism.Navigation;

namespace ShopMe.Client.ViewModels
{
    public sealed class AboutPageViewModel : ViewModelBase
    {
        public DelegateCommand BackCommand
        {
            get;
        }

        public AboutPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            BackCommand = new DelegateCommand(async () => await NavigationService.GoBackAsync());
        }
    }
}