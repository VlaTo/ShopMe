using System;
using System.Threading.Tasks;

namespace ShopMe.Client.Controls
{
    public sealed class InteractionRequest<TRequestContext> : IInteractionRequest<TRequestContext>
    {
        public event EventHandler<InteractionRequestEventArgs> Fired;

        public InteractionRequest()
        {
        }

        public Task Fire(TRequestContext context)
        {
            return Task.CompletedTask;
        }
    }
}