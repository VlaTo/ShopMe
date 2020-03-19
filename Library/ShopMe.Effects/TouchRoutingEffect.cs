using Xamarin.Forms;

namespace ShopMe.Effects
{
    public sealed class TouchRoutingEffect : RoutingEffect
    {
        public TouchRoutingEffect()
            : base("ShopMe.Client.Controls.Effects." + nameof(TouchEffect))
        {
            var temp = Resolve("ShopMe.Client.Controls.Effects." + nameof(TouchEffect));
        }
    }
}