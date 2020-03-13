using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using Prism.Behaviors;
using ShopMe.Client.Controls.Interaction;
using Xamarin.Forms;

namespace ShopMe.Client.Controls
{
    public sealed class InteractionRequestedEventArgs : EventArgs
    {

    }

    public sealed class InteractionBehavior : BehaviorBase<VisualElement>
    {
        public static readonly BindableProperty InteractionRequestProperty;

        private IInteractionRequest interactionRequest;

        public IInteractionRequest InteractionRequest
        {
            get => (IInteractionRequest) GetValue(InteractionRequestProperty);
            set => SetValue(InteractionRequestProperty, value);
        }

        public event EventHandler<InteractionRequestEventArgs> InteractionRequested;

        static InteractionBehavior()
        {
            InteractionRequestProperty = BindableProperty.Create(
                nameof(InteractionRequest),
                typeof(IInteractionRequest),
                typeof(InteractionBehavior),
                propertyChanged: OnInteractionRequestPropertyChanged
            );
        }

        /*protected override void OnAttachedTo(VisualElement bindable)
        {
            base.OnAttachedTo(bindable);
            //bindable.BindingContextChanged += OnBindingContextChanged;
            //UpdateBindingContext(bindable.BindingContext);
        }

        protected override void OnDetachingFrom(VisualElement bindable)
        {
            base.OnDetachingFrom(bindable);
            //bindable.BindingContextChanged -= OnBindingContextChanged;
            //UpdateBindingContext(null);
        }

        private void UpdateBindingContext(object bindingContext)
        {
            if (null != currentBindingContext)
            {
                ;
            }

            currentBindingContext = bindingContext;

            if (null != currentBindingContext)
            {
                AssignBinding();
            }
        }

        private void AssignBinding()
        {
            if (null == currentBindingContext)
            {
                return;
            }

            var property = currentBindingContext.GetType().GetRuntimeProperty("Interaction");

            if (null == property)
            {
                return;
            }

            var interaction = property.GetValue(currentBindingContext) as IInteractionRequest;

            if (null != interaction)
            {
                interaction.Raised += OnInteractionRaised;
            }

            if (currentBindingContext is INotifyPropertyChanged notify)
            {
                notify.PropertyChanged += OnTargetPropertyChanged;
            }
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            if (ReferenceEquals(currentBindingContext, AssociatedObject.BindingContext))
            {
                return;
            }

            UpdateBindingContext(AssociatedObject.BindingContext);
        }

        private void OnTargetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnInteractionRaised(object sender, InteractionRequestEventArgs e)
        {
            Debug.WriteLine("[InteractionBehavior.OnInteractionRaised] ");
        }*/

        private void OnInteractionRequestChanged(IInteractionRequest value)
        {
            if (null != interactionRequest)
            {
                interactionRequest.Raised -= OnInteractionRequestRaised;
            }

            interactionRequest = value;

            if (null != interactionRequest)
            {
                interactionRequest.Raised += OnInteractionRequestRaised;
            }
        }

        private void DoInteractionRequested(InteractionRequestEventArgs e)
        {
            var handler = InteractionRequested;

            if (null != handler)
            {
                handler.Invoke(this, e);
            }
        }

        private void OnInteractionRequestRaised(object sender, InteractionRequestEventArgs e)
        {
            DoInteractionRequested(e);
        }

        private static void OnInteractionRequestPropertyChanged(BindableObject source, object previous, object current)
        {
            ((InteractionBehavior) source).OnInteractionRequestChanged((IInteractionRequest) current);
        }
    }
}