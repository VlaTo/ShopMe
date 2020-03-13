using System;
using Prism.Behaviors;
using Prism.Common;
using Prism.Navigation;
using Xamarin.Forms;

namespace ShopMe.Client.Controls.Behaviors
{
    public sealed class ShellPageAwareBehavior : BehaviorBase<Shell>
    {
        protected override void OnAttachedTo(Shell bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.Navigated += OnNavigated;
        }

        protected override void OnDetachingFrom(Shell bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.Navigated -= OnNavigated;
        }

        private void OnNavigated(object sender, ShellNavigatedEventArgs e)
        {
            PageUtilities.InvokeViewAndViewModelAction<IInitialize>(AssociatedObject, aware => aware.Initialize(null));
        }
    }
}