using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Prism;
using Prism.Ioc;
using RSocket.Core;
using RSocket.Core.Transports;
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

        protected override async void OnInitialized()
        {
            InitializeComponent();

            var client = Container.Resolve<RSocketClient>();

            await client.ConnectAsync(CancellationToken.None);

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
            containerRegistry.RegisterInstance(ConnectClient());
            containerRegistry.RegisterSingleton<IShopListService, RSocketShopListService>();

            //Registering Views+ViewModels
            containerRegistry.RegisterForNavigation<MainContentPage, MainContentPageViewModel>();
            containerRegistry.RegisterForNavigation<AboutPage, AboutPageViewModel>();
        }

        private static RSocketClient ConnectClient()
        {
            var factory = new NullLoggerFactory();
            var transport = new ClientWebSocketTransport("ws://localhost:5000/api", factory.CreateLogger<WebSocketTransport>());
            return new RSocketClient(transport);
        }
    }
}