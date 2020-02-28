using System;

namespace ShopMe.Application.Observable.Collections
{
    public interface ICollectionObserver<in T> : IDisposable
    {
        void OnAdded(T item);

        void OnRemoved(T item);

        void OnUpdated(T item);
    }
}