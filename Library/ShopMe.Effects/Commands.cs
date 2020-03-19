using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace ShopMe.Effects
{
    public static class Commands
    {
        public static readonly BindableProperty TapCommandProperty;

        public static readonly BindableProperty TapCommandParameterProperty;

        public static void SetTapCommand(BindableObject view, ICommand value) =>
            view.SetValue(TapCommandProperty, value);

        public static ICommand GetTapCommand(BindableObject view) =>
            (ICommand) view.GetValue(TapCommandProperty);

        public static void SetTapCommandParameter(BindableObject view, object value) =>
            view.SetValue(TapCommandParameterProperty, value);

        public static object GetTapCommandParameter(BindableObject view) =>
            view.GetValue(TapCommandParameterProperty);

        static Commands()
        {
            TapCommandProperty = BindableProperty.CreateAttached(
                "TapCommand",
                typeof(ICommand),
                typeof(Commands),
                default(ICommand),
                propertyChanged: OnAttachedPropertyChanged
            );
            TapCommandParameterProperty = BindableProperty.CreateAttached(
                "TapCommandParameter",
                typeof(object),
                typeof(Commands),
                default,
                propertyChanged: OnAttachedPropertyChanged
            );
        }

        private static void OnAttachedPropertyChanged(BindableObject source, object oldValue, object newValue)
        {
            if (!(source is View view))
            {
                return;
            }

            var effect = view.Effects.FirstOrDefault(e => e is CommandsRoutingEffect);

            if (null != GetTapCommand(source) /*|| GetLongTap(bindable) != null*/)
            {
                view.InputTransparent = false;

                if (null != effect)
                {
                    return;
                }

                view.Effects.Add(new CommandsRoutingEffect());

                if (EffectsConfiguration.AutoChildrenInputTransparent && source is Layout)
                {
                    if (false == EffectsConfiguration.GetChildrenInputTransparent(view))
                    {
                        EffectsConfiguration.SetChildrenInputTransparent(view, true);
                    }
                }
            }
            else
            {
                if (null == effect || null == view.BindingContext)
                {
                    return;
                }

                view.Effects.Remove(effect);

                if (EffectsConfiguration.AutoChildrenInputTransparent && source is Layout)
                {
                    if (EffectsConfiguration.GetChildrenInputTransparent(view))
                    {
                        EffectsConfiguration.SetChildrenInputTransparent(view, false);
                    }
                }
            }
        }
    }
}