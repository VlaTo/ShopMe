using Xamarin.Forms;

namespace ShopMe.Effects
{
    public sealed class TouchRoutingEffect : RoutingEffect
    {
        public TouchRoutingEffect()
            : base("ShopMe.Effects." + nameof(TouchEffect))
        {
            var temp = Resolve("ShopMe.Effects." + nameof(TouchEffect));
        }
    }
}