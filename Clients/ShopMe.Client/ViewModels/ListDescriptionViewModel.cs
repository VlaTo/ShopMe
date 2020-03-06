using Prism.Mvvm;
using Prism.Navigation;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Forms;

namespace ShopMe.Client.ViewModels
{
    public class ListDescriptionViewModel : BindableBase
    {
        public readonly INavigationService navigation;
        private string title;
        private double progress;
        private bool completed;

        public long Id
        {
            get;
        }


        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
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

        public ListDescriptionViewModel()
        {
            Execute = new Command(DoExecuteCommand);
        }

        public ListDescriptionViewModel(long id, INavigationService navigation)
            : this()
        {
            Id = id;
            this.navigation = navigation;
        }

        private void DoExecuteCommand()
        {
            //await DisplayAlert("Item Tapped", "An item was tapped.", "OK");
            Debug.WriteLine($"[ShopListDescriptionViewModel.DoExecuteCommand] Title: {Title}");
        }
    }
}