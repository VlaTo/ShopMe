using System;
using System.Reactive.Disposables;
using ShopMe.Application.Observable.Collections;

namespace ShopMe.Application.Observable.Extensions
{
    public static class ObservableCollectionExtensions
    {
        public static IDisposable Subscribe<T>(this IObservableCollection<T> collection, 
            Action<T> onAdded,
            Action<T> onRemoved, 
            Action<T> onUpdated = null, 
            Action onCompleted = null)
        {
            return Disposable.Empty;
        }
    }
}