using Xamarin.Forms;

namespace ShopMe.Effects
{
    public class BorderView : ContentView
    {
        public static readonly BindableProperty CornerRadiusProperty;

        public static readonly BindableProperty BorderColorProperty;

        public static readonly BindableProperty BorderWidthProperty;

        public double CornerRadius
        {
            get => (double) GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public double BorderWidth
        {
            get => (double) GetValue(BorderWidthProperty);
            set => SetValue(BorderWidthProperty, value);
        }

        public Color BorderColor
        {
            get => (Color) GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        static BorderView()
        {
            CornerRadiusProperty = BindableProperty.Create(
                nameof(CornerRadius),
                typeof(double),
                typeof(BorderView),
                default(double)
            );
            BorderWidthProperty = BindableProperty.Create(
                nameof(BorderWidth),
                typeof(double),
                typeof(BorderView),
                default(double)
            );
            BorderColorProperty = BindableProperty.Create(
                nameof(BorderColor),
                typeof(Color),
                typeof(BorderView),
                Color.Default
            );
        }
    }
}