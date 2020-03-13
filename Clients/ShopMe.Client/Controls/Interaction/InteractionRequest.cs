using System;
using System.Threading.Tasks;

namespace ShopMe.Client.Controls.Interaction
{
    public class InteractionRequest : IInteractionRequest
    {
        private WeakEventHandler<InteractionRequestEventArgs> raised;

        public event EventHandler<InteractionRequestEventArgs> Raised
        {
            add => raised = WeakEventHandler<InteractionRequestEventArgs>.Combine(raised, value);
            remove => raised = WeakEventHandler<InteractionRequestEventArgs>.Remove(raised, value);
        }

        public InteractionRequest()
        {
            raised = WeakEventHandler<InteractionRequestEventArgs>.Empty;
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
        public void Raise(TContext context, Action callback)
        {
            if (null == callback)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            DoRaise(new InteractionRequestEventArgs(context, callback));
        }
    }
}