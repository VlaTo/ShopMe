using ShopMe.Effects.Behaviors;
using ShopMe.Effects.Interaction;
using Xamarin.Forms.Xaml;

namespace ShopMe.Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShopListContentPage
    {
        public ShopListContentPage()
        {
            InitializeComponent();
        }

        private void OnUpdateShopListRequested(object sender, RequestEventArgs<InteractionRequestContext> e)
        {
            Dispatcher.BeginInvokeOnMainThread(e.Callback);
        }
    }
}