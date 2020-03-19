using ShopMe.Effects.Interaction;

namespace ShopMe.Client.ViewModels.Contexts
{
    public sealed class OpenShopListRequestContext : InteractionRequestContext
    {
        public long Id
        {
            get;
        }

        public OpenShopListRequestContext(long id)
        {
            Id = id;
        }
    }
}