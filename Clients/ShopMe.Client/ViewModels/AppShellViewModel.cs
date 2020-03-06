﻿using Prism.AppModel;
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
using System.Windows.Input;
using Xamarin.Forms;

namespace ShopMe.Client.ViewModels
{
    public sealed class AppShellViewModel : BindableBase, IInitialize, IDestructible, IAutoInitialize
    {
        private readonly IShopMeEngine engine;
        private readonly INavigationService navigation;
        private readonly IInteractionDispatcher dispatcher;
        private IDisposable disposable;

        public ObservableCollection<ListDescriptionViewModel> Items
        {
            get;
        }

        public ObservableCollection<ListDescriptionViewModel> LatestItems
        {
            get;
        }

        public ICommand BackCommand
        {
            get;
        }

        public ICommand TappedCommand
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
            TappedCommand = new Command<ListDescriptionViewModel>(OnSelectedItemTapped);
            BackCommand = new Command(OnBackButton);
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

        private void OnSelectedItemTapped(ListDescriptionViewModel obj)
        {
            Debug.Write("OnSelectedItemTapped");
        }
    }
}