using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;

namespace ShopMe.Application.Observable.Collections
{
    internal sealed class ObservableCollection<T> : CollectionBase, IObservableCollection<T>
    {
        private readonly List<Tuple<ICollectionObserver<T>, IDisposable>> observers;
        private readonly object gate;

        public ObservableCollection()
        {
            gate = new object();
            observers = new List<Tuple<ICollectionObserver<T>, IDisposable>>();
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer)
        {
            if (null == observer)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            IDisposable disposable;

            lock (gate)
            {
                if (false == TryGetDisposable(observer, out disposable))
                {
                    disposable = Disposable.Create(observer, RemoveObserver);
                    observers.Add(new Tuple<ICollectionObserver<T>, IDisposable>(observer, disposable));
                }
            }

            return disposable;
        }

        public void Add(T item)
        {
            InnerList.Add(item);
            CallObservers(observer => observer.OnAdded(item));
        }

        private bool TryGetDisposable(ICollectionObserver<T> observer, out IDisposable disposable)
        {
            var index = observers.FindIndex(tuple => tuple.Item1 == observer);

            if (0 > index)
            {
                disposable = Disposable.Empty;
                return false;
            }

            disposable = observers[index].Item2;

            return true;
        }

        private void RemoveObserver(ICollectionObserver<T> observer)
        {
            IDisposable disposable;

            lock (gate)
            {
                var index = observers.FindIndex(tuple => tuple.Item1 == observer);

                if (0 > index)
                {
                    return ;
                }

                disposable = observers[index].Item2;

                observers.RemoveAt(index);
            }

            disposable.Dispose();
        }

        private void CallObservers(Action<ICollectionObserver<T>> action)
        {
            ICollectionObserver<T>[] handlers;

            lock (gate)
            {
                handlers = observers.Select(tuple => tuple.Item1).ToArray();
            }

            foreach (var handler in handlers)
            {
                action.Invoke(handler);
            }
        }
    }
}