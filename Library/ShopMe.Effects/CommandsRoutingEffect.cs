using Xamarin.Forms;

namespace ShopMe.Effects
{
    public class CommandsRoutingEffect : RoutingEffect
    {
        public CommandsRoutingEffect()
            : base("ShopMe.Client.Controls.Effects." + nameof(Commands))
        {
        }
    }
}