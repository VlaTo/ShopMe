using Prism;
using Prism.Ioc;
using RSocket.Core;
using ShopMe.Client.Services;
using ShopMe.Client.ViewModels;
using ShopMe.Client.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace ShopMe.Client
{
    public partial class App
    {
        /* 
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor. 
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */
        public App()
            : this(null)
        {
        }

        public App(IPlatformInitializer initializer)
            : base(initializer)
        {
        }

        protected override void OnInitialized()
        {
            InitializeComponent();

            MainPage = new AppShell();

            /*var result = await NavigationService.NavigateAsync("NavigationPage/MainPage");

            if (false == result.Success)
            {
                ;
            }*/
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //Services
            containerRegistry.RegisterSingleton<RSocketClient>();
            containerRegistry.RegisterSingleton<IShopListService, RSocketShopListService>();

            //Registering Views+ViewModels
            //containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>();
        }
    }
}
