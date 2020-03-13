using System;

namespace ShopMe.Client.Controls.Interaction
{
    public interface IInteractionRequest
    {
        event EventHandler<InteractionRequestEventArgs> Raised;
    }
}