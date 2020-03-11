using System;
using System.Threading.Tasks;

namespace ShopMe.Client.Controls
{
    public sealed class InteractionRequestEventArgs : EventArgs
    {

    }

    public interface IInteractionRequest
    {
        
    }

    public interface IInteractionRequest<in TRequestContext> : IInteractionRequest
    {
        event EventHandler<InteractionRequestEventArgs> Fired;

        Task Fire(TRequestContext context);
    }
}