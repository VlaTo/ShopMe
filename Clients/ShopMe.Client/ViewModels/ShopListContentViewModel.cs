using Prism.Navigation;
using ShopMe.Application;
using ShopMe.Application.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ShopMe.Effects.Interaction;
using Xamarin.Forms;

namespace ShopMe.Client.ViewModels
{
    [QueryProperty(nameof(Id), "id")]
    public sealed class ShopListContentViewModel : ViewModelBase, IInitializeAsync, IDestructible
    {
        private readonly IShopMeEngine engine;
        private string id;
        private IDisposable disposable;

        public string Id
        {
            get => id;
            set
            {
                id = value;
                Debug.WriteLine($"Shop List Id: {id}");
            }
        }

        public InteractionRequest<InteractionRequestContext> UpdateShopListRequest
        {
            get;
        }

        public ShopListContentViewModel(IShopMeEngine engine)
        {
            this.engine = engine;
            UpdateShopListRequest = new InteractionRequest<InteractionRequestContext>();
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            var list = await engine.GetShopListAsync(GetShopListId(), CancellationToken.None);

            UpdateShopListRequest.Raise(
                InteractionRequestContext.Empty,
                () =>
                {
                    Title = list.Title;
                }
            );

            disposable = list.GetChanges(CancellationToken.None).Subscribe(ProcessShopListChanges);
        }

        private void ProcessShopListChanges(ShopListChanges changes)
        {
            UpdateShopListRequest.Raise(
                InteractionRequestContext.Empty,
                () =>
                {

                }
            );
        }

        private long GetShopListId()
        {
            if (long.TryParse(Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            throw new InvalidCastException();
        }

        public void Destroy()
        {
            disposable.Dispose();
        }
    }
}