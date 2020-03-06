using System;
using Xamarin.Essentials;

namespace ShopMe.Client.Services
{
    internal sealed class MainThreadDispatcher : IInteractionDispatcher
    {
        public void Dispatch(Action action)
        {
            MainThread.BeginInvokeOnMainThread(action);
        }
    }
}