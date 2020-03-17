using System.Linq;
using Xamarin.Forms;

namespace ShopMe.Client.Controls.Effects
{
    public static class TouchEffect
    {
        public static readonly BindableProperty ColorProperty;

        public static void SetColor(BindableObject view, Color value) => view.SetValue(ColorProperty, value);

        public static Color GetColor(BindableObject view) => (Color) view.GetValue(ColorProperty);

        static TouchEffect()
        {
            ColorProperty = BindableProperty.CreateAttached(
                "Color",
                typeof(Color),
                typeof(TouchEffect),
                Color.Default,
                propertyChanged: DoColorPropertyChanged
            );
        }

        private static void DoColorPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (false == (bindable is View view))
            {
                return;
            }

            var touchEffect = view.Effects.FirstOrDefault(effect => effect is TouchRoutingEffect);

            if (Color.Default != GetColor(view))
            {
                view.InputTransparent = false;

                if (null != touchEffect)
                {
                    return;
                }

                view.Effects.Add(new TouchRoutingEffect());

                //if(EffectsConfig.)
            }
            else
            {
                if (null == touchEffect || null == view.BindingContext)
                {
                    return;
                }

                view.Effects.Remove(touchEffect);
            }
        }
    }
}