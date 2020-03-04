using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Prism;
using Prism.Events;
using Prism.Ioc;
using RSocket.Core;
using RSocket.Core.Transports;
using ShopMe.Application;
using ShopMe.Application.Services;
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

            MainPage = Container.Resolve<AppShell>(); // new AppShell();

            /*var result = await NavigationService.NavigateAsync("NavigationPage/MainPage");

            if (false == result.Success)
            {
                ;
            }*/
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //Services
            containerRegistry.RegisterInstance(CreateClient());
            //containerRegistry.RegisterSingleton<IShopListService, RSocketShopListService>();
            containerRegistry.RegisterSingleton<IDataProvider, DataProvider>();
            containerRegistry.RegisterSingleton<IChangesProvider, RemoteChangesProvider>();
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<IShopMeEngine, ShopMeEngine>();

            //Registering Views+ViewModels
            containerRegistry.RegisterForNavigation<AppShell, AppShellViewModel>();
            //containerRegistry.RegisterForNavigation<MainContentPage, MainContentPageViewModel>();
            containerRegistry.RegisterForNavigation<AboutPage, AboutPageViewModel>();
        }

        private static RSocketClient CreateClient()
        {
            var factory = new NullLoggerFactory();
            var transport = new ClientWebSocketTransport("ws://localhost:5000/api", factory.CreateLogger<WebSocketTransport>());
            return new RSocketClient(transport);
        }
    }
}