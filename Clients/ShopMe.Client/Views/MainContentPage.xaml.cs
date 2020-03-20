using System;
using System.Diagnostics;
using ShopMe.Client.ViewModels.Contexts;
using ShopMe.Effects.Behaviors;
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

            var value = Preferences.Get("test-key", "no-test-key");
        }

        private async void OnOpenCreateNewRequested(object sender, RequestEventArgs<OpenCreateNewRequestContext> e)
        {
            var result = await Shell.Current.DisplayAlert(
                "Create new",
                "Aliquam imperdiet ac odio sed efficitur. Vestibulum tristique lorem at urna faucibus.",
                "OK",
                "Cancel"
            );

            if (result)
            {
                e.Context.SetResult("Aliquam imperdiet");
            }
            else
            {
                e.Context.Cancel();
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            Debug.WriteLine("[MainContentPage.TapGestureRecognizer_Tapped] Tap");
        }
    }
}
