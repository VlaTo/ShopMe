using System.Collections.Generic;
using Prism.Behaviors;
using ShopMe.Effects.Interaction;
using Xamarin.Forms;

namespace ShopMe.Effects.Behaviors
{
    /// <summary>
    /// 
    /// </summary>
    [ContentProperty(nameof(Requests))]
    public sealed class InteractionBehavior : BehaviorBase<VisualElement>
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly BindableProperty InteractionRequestProperty;

        private IInteractionRequest interactionRequest;

        /// <summary>
        /// 
        /// </summary>
        public IInteractionRequest InteractionRequest
        {
            get => (IInteractionRequest) GetValue(InteractionRequestProperty);
            set => SetValue(InteractionRequestProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<IOnRequest> Requests
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public InteractionBehavior()
        {
            Requests = new List<IOnRequest>();
        }

        static InteractionBehavior()
        {
            InteractionRequestProperty = BindableProperty.Create(
                nameof(InteractionRequest),
                typeof(IInteractionRequest),
                typeof(InteractionBehavior),
                propertyChanged: OnInteractionRequestPropertyChanged
            );
        }

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
            for (var index = 0; index < Requests.Count; index++)
            {
                var request = Requests[index];
                request.Invoke(e);
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