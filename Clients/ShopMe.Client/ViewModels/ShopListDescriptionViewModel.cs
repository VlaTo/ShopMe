using System.Diagnostics;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace ShopMe.Client.ViewModels
{
    public class ShopListDescriptionViewModel : BindableBase
    {
        private string title;

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public ICommand Execute
        {
            get;
        }

        public ShopListDescriptionViewModel()
        {
            Execute = new DelegateCommand(DoExecuteCommand);
        }

        private void DoExecuteCommand()
        {
            Debug.WriteLine($"[ShopListDescriptionViewModel.DoExecuteCommand] Title: {Title}");
        }
    }
}