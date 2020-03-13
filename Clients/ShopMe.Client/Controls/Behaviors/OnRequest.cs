using ShopMe.Client.Controls.Interaction;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ShopMe.Client.Controls.Behaviors
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public sealed class RequestEventArgs<TContext> : EventArgs
        where TContext : InteractionRequestContext
    {
        public TContext Context
        {
            get;
        }

        public Action Callback
        {
            get;
        }

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

        public event EventHandler<RequestEventArgs<TContext>> Raised
        {
            add => raised = WeakEventHandler<RequestEventArgs<TContext>>.Combine(raised, value);
            remove => raised = WeakEventHandler<RequestEventArgs<TContext>>.Remove(raised, value);
        }

        public OnRequest()
        {
            raised = WeakEventHandler<RequestEventArgs<TContext>>.Empty;
        }

        public void Invoke(InteractionRequestEventArgs e)
        {
            if (e.Context is TContext context)
            {
                raised.Invoke(this, new RequestEventArgs<TContext>(context, e.Callback));
            }
        }
    }
}
