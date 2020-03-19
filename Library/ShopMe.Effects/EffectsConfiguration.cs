using Xamarin.Forms;

namespace ShopMe.Effects
{
    public static class EffectsConfiguration
    {
        public static readonly BindableProperty ChildrenInputTransparentProperty;

        public static void SetChildrenInputTransparent(BindableObject view, bool value) =>
            view.SetValue(ChildrenInputTransparentProperty, value);

        public static bool GetChildrenInputTransparent(BindableObject view) =>
            (bool) view.GetValue(ChildrenInputTransparentProperty);

        public static bool AutoChildrenInputTransparent
        {
            get;
        }

        static EffectsConfiguration()
        {
            AutoChildrenInputTransparent = true;
            ChildrenInputTransparentProperty = BindableProperty.CreateAttached(
                "ChildrenInputTransparent",
                typeof(bool),
                typeof(EffectsConfiguration),
                false,
                propertyChanged: (source, oldValue, newValue) =>
                {
                    ConfigureChildrenInputTransparent(source);
                }
            );
        }

        private static void ConfigureChildrenInputTransparent(BindableObject source)
        {
            if (!(source is Layout layout))
            {
                return;
            }

            if (GetChildrenInputTransparent(source))
            {
                foreach (var layoutChild in layout.Children)
                {
                    AddInputTransparentToElement(layoutChild);
                }

                layout.ChildAdded += OnLayoutChildAdded;
            }
            else
            {
                layout.ChildAdded -= OnLayoutChildAdded;
            }
        }

        private static void OnLayoutChildAdded(object sender, ElementEventArgs e) =>
            AddInputTransparentToElement(e.Element);

        private static void AddInputTransparentToElement(BindableObject obj)
        {
            if (!(obj is View view) || Color.Default != TouchEffect.GetColor(view))
            {
                return;
            }

            if (null == Commands.GetTapCommand(view) /*&& Commands.GetLongTap(view) == null*/)
            {
                view.InputTransparent = true;
            }
        }
    }
}