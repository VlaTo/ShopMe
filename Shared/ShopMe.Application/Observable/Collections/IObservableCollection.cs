using System;

namespace ShopMe.Application.Observable.Collections
{
    public interface IObservableCollection<out T>
    {
        IDisposable Subscribe(ICollectionObserver<T> observer);
    }
}