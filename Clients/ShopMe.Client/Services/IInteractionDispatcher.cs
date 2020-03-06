using System;

namespace ShopMe.Client.Services
{
    public interface IInteractionDispatcher
    {
        void Dispatch(Action action);
    }
}