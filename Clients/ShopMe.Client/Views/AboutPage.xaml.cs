using System.Diagnostics;
using System.Threading.Tasks;
using Prism.Navigation;
using Xamarin.Forms.Xaml;

namespace ShopMe.Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : IInitializeAsync, IDestructible, INavigationAware
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        public Task InitializeAsync(INavigationParameters parameters)
        {
            Debug.WriteLine($"AboutPage.InitializeAsync");

            return Task.CompletedTask;
        }

        public void Destroy()
        {
            Debug.WriteLine($"AboutPage.Destroy");
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            Debug.WriteLine($"AboutPage.OnNavigatedFrom");
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            Debug.WriteLine($"AboutPage.OnNavigatedTo");
        }
    }
}