using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace ShopMe.Client.ViewModels
{
    public class ListDescriptionViewModel : BindableBase
    {
        public readonly INavigationService navigation;
        private string title;
        private double progress;
        private bool completed;
        private DateTimeOffset created;

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

        public bool IsCompleted
        {
            get => completed;
            set => SetProperty(ref completed, value);
        }

        public DateTimeOffset Created
        {
            get => created;
            set => SetProperty(ref created, value);
        }

        public Command OpenDetails
        {
            get;
        }

        public Command Remove
        {
            get;
        }

        public Command Complete
        {
            get;
        }

        public ListDescriptionViewModel()
        {
            OpenDetails = new Command(DoOpenDetails);
            Remove = new Command(DoRemoveCommand);
            Complete = new Command(DoComplete);
        }

        public ListDescriptionViewModel(long id, INavigationService navigation)
            : this()
        {
            Id = id;
            this.navigation = navigation;
        }

        private void DoOpenDetails()
        {
            //await DisplayAlert("Item Tapped", "An item was tapped.", "OK");
            Debug.WriteLine($"[ShopListDescriptionViewModel.DoExecuteCommand] Id: {Id}, Title: {Title}");
        }

        private void DoRemoveCommand(object obj)
        {
            Debug.WriteLine($"[ShopListDescriptionViewModel.DoDeleteCommand] Title: {Title}");
        }

        private void DoComplete()
        {
            Debug.WriteLine($"[ShopListDescriptionViewModel.DoComplete] Title: {Title}");
        }
    }
}