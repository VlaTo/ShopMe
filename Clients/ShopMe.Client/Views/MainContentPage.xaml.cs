﻿using System;
using Prism.Navigation;
using ShopMe.Client.Services;
using ShopMe.Client.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShopMe.Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainContentPage
    {
        public MainContentPage()
        {
            InitializeComponent();

            var value = Preferences.Get("test-key", "test");
        }
        
        private async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            await DisplayAlert("Item Tapped", "An item was tapped.", "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        private void OnPageAppearing(object sender, EventArgs e)
        {
            if (BindingContext is IAppearing appearing)
            {
                appearing.Appearing();
            }
        }
    }
}