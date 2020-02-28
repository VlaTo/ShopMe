using System.Diagnostics;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Navigation.Xaml;
using Xamarin.Forms;

namespace ShopMe.Client.ViewModels
{
    public class ShopListDescriptionViewModel : BindableBase
    {
        private string title;
        private string description;
        private double progress;
        private bool completed;

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        public double Progress
        {
            get => progress;
            set => SetProperty(ref progress, value);
        }

        public bool Completed
        {
            get => completed;
            set => SetProperty(ref completed, value);
        }

        public ICommand Execute
        {
            get;
        }

        public ShopListDescriptionViewModel(INavigationService navigationService)
        {
            Execute = new Command(DoExecuteCommand);
            //Execute = new DelegateCommand(DoExecuteCommand);
        }

        private void DoExecuteCommand()
        {
            //await DisplayAlert("Item Tapped", "An item was tapped.", "OK");
            Debug.WriteLine($"[ShopListDescriptionViewModel.DoExecuteCommand] Title: {Title}");
        }
    }
}