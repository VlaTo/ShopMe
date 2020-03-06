using Prism.AppModel;
using Prism.Navigation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShopMe.Client
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell
    {
        public AppShell()
        {
            InitializeComponent();

            //BindingContextChanged += OnBindingChanged;
        }

        /*private void OnBindingChanged(object sender, EventArgs e)
        {
            var context = BindingContext;

            if (context is IInitializeAsync request)
            {
                 request.InitializeAsync(null);
            }
        }*/

        private void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            var context = BindingContext;

            if (context is IAutoInitialize)
            {
                if (context is IInitialize requestor)
                {
                    requestor.Initialize(null);
                }
            }
        }
    }
}
