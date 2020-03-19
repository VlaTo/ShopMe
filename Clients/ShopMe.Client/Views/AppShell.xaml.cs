using ShopMe.Client.ViewModels.Contexts;
using ShopMe.Effects.Behaviors;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShopMe.Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell
    {
        private const string detailsPageName = "details";

        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(detailsPageName, typeof(ShopListContentPage));
        }

        private async void OnInteractionRequested(object sender, RequestEventArgs<OpenShopListRequestContext> e)
        {
            await Shell.Current.GoToAsync($"{detailsPageName}?id={e.Context.Id}");

            e.Callback.Invoke();
        }
    }
}
