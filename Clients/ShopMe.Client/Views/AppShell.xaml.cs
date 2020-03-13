using ShopMe.Client.Controls.Interaction;
using ShopMe.Client.ViewModels.Contexts;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShopMe.Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("details", typeof(ShopListContentPage));
        }

        private async void OnInteractionRequested(object sender, InteractionRequestEventArgs e)
        {
            switch (e.Context)
            {
                case OpenShopListRequestContext open:
                {
                    await Shell.Current.GoToAsync($"details?id={open.Id}");
                    
                    e.Callback.Invoke();

                    break;
                }
            }
        }
    }
}
