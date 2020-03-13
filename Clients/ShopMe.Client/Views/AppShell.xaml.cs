using ShopMe.Client.Controls.Behaviors;
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

        private async void OnInteractionRequested(object sender, RequestEventArgs<OpenShopListRequestContext> e)
        {
            await Shell.Current.GoToAsync($"details?id={e.Context.Id}");

            e.Callback.Invoke();
        }
    }
}
