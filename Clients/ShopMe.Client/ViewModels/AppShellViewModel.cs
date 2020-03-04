using System;
using Prism.Mvvm;
using ShopMe.Application;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.AppModel;
using Prism.Navigation;
using Xamarin.Essentials;
using Xamarin.Forms;
using ShopMe.Application.Observable;
using ShopMe.Application.Observable.Extensions;

namespace ShopMe.Client.ViewModels
{
    public sealed class AppShellViewModel : BindableBase, IInitializeAsync, INavigationAware, IAutoInitialize
    {
        private readonly IShopMeEngine engine;
        private readonly INavigationService navigation;
        private IDisposable disposable;

        public ObservableCollection<ShopListViewModel> Items
        {
            get;
        }

        public ObservableCollection<ShopListViewModel> LatestItems
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

            Items = new ObservableCollection<ShopListViewModel>();
            LatestItems = new ObservableCollection<ShopListViewModel>();
            BackCommand = new Command(async () => await navigation.GoBackAsync());
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            Items.Clear();

            var lists = await engine.GetActualListsAsync(CancellationToken.None);

            disposable = lists.Subscribe(
                async list =>
                {
                    var model = new ShopListViewModel(list.Id, navigation)
                    {
                        Title = list.Title
                    };

                    await MainThread.InvokeOnMainThreadAsync(() => Items.Add(model));

                },
                async list =>
                {
                    var model = Items.FirstOrDefault(item => item.Id == list.Id);

                    if (null != model)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() => Items.Remove(model));
                    }
                },
                async list =>
                {
                    var model = Items.FirstOrDefault(item => item.Id == list.Id);

                    if (null != model)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() => { model.Title = list.Title; });
                    }
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