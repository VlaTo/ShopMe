using System;
using Prism.Mvvm;
using ShopMe.Application;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.AppModel;
using Prism.Navigation;
using Xamarin.Essentials;
using Xamarin.Forms;
using ShopMe.Application.Observable;

namespace ShopMe.Client.ViewModels
{
    public sealed class AppShellViewModel : BindableBase, IInitializeAsync, INavigationAware, IAutoInitialize
    {
        private readonly IShopMeEngine engine;
        private readonly INavigationService navigation;
        private IDisposable disposable;

        public ObservableCollection<ShopListDescriptionViewModel> Items
        {
            get;
        }

        public ObservableCollection<ShopListDescriptionViewModel> LatestItems
        {
            get;
        }

        public ICommand BackCommand
        {
            get;
        }

        public AppShellViewModel(IShopMeEngine engine, INavigationService navigation)
        {
            this.engine = engine;
            this.navigation = navigation;

            Items = new ObservableCollection<ShopListDescriptionViewModel>();
            LatestItems = new ObservableCollection<ShopListDescriptionViewModel>();
            BackCommand = new Command(async () => await navigation.GoBackAsync());
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            Debugger.Break();

            Items.Clear();

            var lists = await engine.GetActualLists();

            disposable = lists.Subscribe(async item =>
                {
                    var model = new ShopListDescriptionViewModel(navigation)
                    {
                        Title = item.Title
                    };

                    await MainThread.InvokeOnMainThreadAsync(() => Items.Add(model));

                },
                async item =>
                {
                    var model = new ShopListDescriptionViewModel(navigation)
                    {
                        Title = item.Title
                    };

                    await MainThread.InvokeOnMainThreadAsync(() => Items.Remove(model));
                }
            );
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            Debugger.Break();
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            Debugger.Break();
        }
    }
}