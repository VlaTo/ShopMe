using System;

namespace ShopMe.Effects.Interaction
{
    /// <summary>
    /// 
    /// </summary>
    public class InteractionRequest : IInteractionRequest
    {
        private WeakEventHandler<InteractionRequestEventArgs> raised;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<InteractionRequestEventArgs> Raised
        {
            add => raised = WeakEventHandler.Combine(raised, value);
            remove => raised = WeakEventHandler.Remove(raised, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public InteractionRequest()
        {
            raised = WeakEventHandler.Empty<InteractionRequestEventArgs>();
        }

        protected void DoRaise(InteractionRequestEventArgs e)
        {
            raised.Invoke(this, e);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class InteractionRequest<TContext> : InteractionRequest
        where TContext : InteractionRequestContext
    {
        private static Action Stub => () => { };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="callback"></param>
        public void Raise(TContext context, Action callback)
        {
            if (null == callback)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            DoRaise(new InteractionRequestEventArgs(context, callback));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void Raise(TContext context) => Raise(context, Stub);
    }
}