using System;

namespace ShopMe.Effects.Interaction
{
    public interface IInteractionRequest
    {
        event EventHandler<InteractionRequestEventArgs> Raised;
    }
}