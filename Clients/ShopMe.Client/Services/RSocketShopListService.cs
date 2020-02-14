using RSocket.Core;

namespace ShopMe.Client.Services
{
    internal sealed class RSocketShopListService : IShopListService
    {
        private readonly RSocketClient client;

        public RSocketShopListService(RSocketClient client)
        {
            this.client = client;
        }
    }
}