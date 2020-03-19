using Prism.AppModel;
using Prism.Mvvm;
using Prism.Navigation;
using ShopMe.Application;
using ShopMe.Application.Models;
using ShopMe.Client.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ShopMe.Client.Controls;
using ShopMe.Client.ViewModels.Contexts;
using ShopMe.Effects.Interaction;
using Xamarin.Forms;

namespace ShopMe.Client.ViewModels
{
    public sealed class AppShellViewModel : BindableBase, IInitialize, IDestructible, IAutoInitialize
    {
        private readonly IShopMeEngine engine;
        private readonly INavigationService navigation;
        private readonly IInteractionDispatcher dispatcher;
        private IDisposable disposable;
        private string title;

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public ObservableCollection<ListDescriptionViewModel> Items
        {
            get;
        }

        public ObservableCollection<ListDescriptionViewModel> LatestItems
        {
            get;
        }

        public Command GoBack
        {
            get;
        }

        public Command RefreshList
        {
            get;
        }

        public Command<ListDescriptionViewModel> OpenDetails
        {
            get;
        }

        public InteractionRequest<OpenShopListRequestContext> OpenDetailsRequired
        {
            get;
        }

        public AppShellViewModel(
            IShopMeEngine engine,
            INavigationService navigation,
            IInteractionDispatcher dispatcher)
        {
            this.engine = engine;
            this.navigation = navigation;
            this.dispatcher = dispatcher;

            disposable = Disposable.Empty;

            Items = new ObservableCollection<ListDescriptionViewModel>();
            LatestItems = new ObservableCollection<ListDescriptionViewModel>();
            OpenDetails = new Command<ListDescriptionViewModel>(OnOpenDetails);
            GoBack = new Command(OnBackButton);
            RefreshList = new Command(OnRefreshList);
            OpenDetailsRequired = new InteractionRequest<OpenShopListRequestContext>();
        }

        public void Initialize(INavigationParameters parameters)
        {
            Items.Clear();

            disposable = engine
                .GetActualLists(CancellationToken.None)
                .Subscribe(ProcessChanges);
        }

        public void Destroy()
        {
            disposable.Dispose();
        }

        private void ProcessChanges(ShopListDescriptionChanges changes)
        {
            dispatcher.Dispatch(() =>
            {
                if (0 < changes.Added.Length)
                {
                    for (var index = 0; index < changes.Added.Length; index++)
                    {
                        var description = changes.Added[index];

                        Items.Add(new ListDescriptionViewModel(description.Id, navigation)
                        {
                            Title = description.Title
                        });
                    }
                }
            });

            /*
                    var model = new ShopListViewModel(list.Id, navigation)
                    {
                        Title = list.Title
                    };

                    await MainThread.InvokeOnMainThreadAsync(() => Items.Add(model));
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
             */
        }

        private async void OnBackButton()
        {
            await navigation.GoBackAsync();
        }

        private async void OnRefreshList()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(600.0d));
        }

        private void OnOpenDetails(ListDescriptionViewModel item)
        {
            if (null == item)
            {
                return;
            }

            OpenDetailsRequired.Raise(new OpenShopListRequestContext(item.Id), () =>
            {
                Debug.WriteLine("[AppShellViewModel.OnSelectedItemTapped] Request callback");
            });

            Debug.WriteLine("[AppShellViewModel.OnSelectedItemTapped]");
        }
    }
}