using System;
using Prism.Mvvm;
using Prism.Navigation;
using ShopMe.Client.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Events;
using Xamarin.Essentials;

namespace ShopMe.Client.ViewModels
{
    public class MainContentPageViewModel : BindableBase, IAppearing
    {
        private readonly IShopListService service;
        private readonly INavigationService navigationService;
        private readonly IEventAggregator eventAggregator;
        private readonly CancellationTokenSource cts;

        public DelegateCommand BackCommand
        {
            get;
        }

        public ObservableCollection<ShopListDescriptionViewModel> Items
        {
            get; 
        }

        public MainContentPageViewModel(IShopListService service, INavigationService navigationService, IEventAggregator eventAggregator)
        {
            this.service = service;
            this.navigationService = navigationService;
            this.eventAggregator = eventAggregator;
            cts = new CancellationTokenSource();

            Items = new ObservableCollection<ShopListDescriptionViewModel>();
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
                var model = new ShopListDescriptionViewModel(navigationService)
                {
                    Title = list.Title
                };

                await MainThread.InvokeOnMainThreadAsync(() => Items.Add(model));

            });
        }
    }
}
