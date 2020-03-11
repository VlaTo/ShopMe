using System.Diagnostics;
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

        private void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
        {
            Debug.WriteLine($"[MainContentPage.TapGestureRecognizer_Tapped]");
        }
    }
}
