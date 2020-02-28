using System;
using ShopMe.Application.Observable.Collections;

namespace ShopMe.Application.Observable
{
    public static class ObservableCollection
    {
        public static IObservableCollection<T> Create<T>(Action<ICollectionObserver<T>> creator)
        {
            throw new NotImplementedException();

            /*if (null == creator)
            {
                throw new ArgumentNullException(nameof(creator));
            }

            ICollectionObserver<T> observer = null;

            creator.Invoke(observer);*/
        }
    }
}