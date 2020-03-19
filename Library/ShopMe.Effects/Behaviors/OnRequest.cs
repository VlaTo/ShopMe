using System;
using ShopMe.Effects.Interaction;
using Xamarin.Forms;

namespace ShopMe.Effects.Behaviors
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public sealed class RequestEventArgs<TContext> : EventArgs
        where TContext : InteractionRequestContext
    {
        /// <summary>
        /// 
        /// </summary>
        public TContext Context
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public Action Callback
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="callback"></param>
        public RequestEventArgs(TContext context, Action callback)
        {
            Context = context;
            Callback = callback;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IOnRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        void Invoke(InteractionRequestEventArgs e);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public sealed class OnRequest<TContext> : BindableObject, IOnRequest
        where TContext : InteractionRequestContext
    {
        private WeakEventHandler<RequestEventArgs<TContext>> raised;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<RequestEventArgs<TContext>> Raised
        {
            add => raised = WeakEventHandler.Combine(raised, value);
            remove => raised = WeakEventHandler.Remove(raised, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public OnRequest()
        {
            raised = WeakEventHandler.Empty<RequestEventArgs<TContext>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void Invoke(InteractionRequestEventArgs e)
        {
            if (e.Context is TContext context)
            {
                raised.Invoke(this, new RequestEventArgs<TContext>(context, e.Callback));
            }
        }
    }
}
