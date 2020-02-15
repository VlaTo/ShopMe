using System;
using Prism.Mvvm;
using Prism.Navigation;
using ShopMe.Client.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Prism.Commands;
using Xamarin.Essentials;

namespace ShopMe.Client.ViewModels
{
    public class MainContentPageViewModel : BindableBase, IAppearing
    {
        private readonly IShopListService service;
        private readonly INavigationService navigationService;
        private readonly CancellationTokenSource cts;

        public DelegateCommand BackCommand
        {
            get;
        }

        public ObservableCollection<ShopListDescription> Items
        {
            get; 
        }

        public MainContentPageViewModel(IShopListService service, INavigationService navigationService)
        {
            this.service = service;
            this.navigationService = navigationService;
            cts = new CancellationTokenSource();

            Items = new ObservableCollection<ShopListDescription>();
            BackCommand = new DelegateCommand(async () => await this.navigationService.GoBackAsync());
        }

        /*public override void OnNavigatedTo(INavigationParameters parameters)
        {
            Items.Clear();

            service.GetListsAsync(cts.Token).Do(async list =>
            {
                var model = new ShopListDescription
                {
                    Title = list.Title
                };

                await MainThread.InvokeOnMainThreadAsync(() => Items.Add(model));

            });

            base.OnNavigatedTo(parameters);
        }*/

        public void Appearing()
        {
            Items.Clear();

            service.GetLists(cts.Token).Subscribe(async list =>
            {
                var model = new ShopListDescription
                {
                    Title = list.Title
                };

                await MainThread.InvokeOnMainThreadAsync(() => Items.Add(model));

            });
        }
    }
}
