using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace ShopMe.Effects
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    public abstract class WeakEventHandler<TEventArgs>
        where TEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract bool IsAlive
        {
            get;
        }

        protected MethodInfo MethodInfo
        {
            get;
        }

        protected WeakEventHandler(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void Invoke(object sender, TEventArgs e);

        protected Delegate CreateDelegate(WeakReference target)
        {
            if (null == target)
            {
                return Delegate.CreateDelegate(typeof(EventHandler<TEventArgs>), MethodInfo);
            }

            return Delegate.CreateDelegate(
                typeof(EventHandler<TEventArgs>),
                target.Target,
                MethodInfo
            );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class WeakEventHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <returns></returns>
        public static WeakEventHandler<TEventArgs> Empty<TEventArgs>()
            where TEventArgs : EventArgs
            =>
            EmptyEventHandler<TEventArgs>.Empty;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="source"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static WeakEventHandler<TEventArgs> Combine<TEventArgs>(WeakEventHandler<TEventArgs> source, EventHandler<TEventArgs> handler)
            where TEventArgs : EventArgs
        {
            if (null == source)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (null == handler)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            switch (source)
            {
                case EmptyEventHandler<TEventArgs> _:
                    {
                        return new SingleEventHandler<TEventArgs>(handler);
                    }

                case SingleEventHandler<TEventArgs> target:
                    {
                        return new CombinedEventHandler<TEventArgs>(target, handler);
                    }

                case CombinedEventHandler<TEventArgs> combined:
                    {
                        if (ReferenceEquals(Empty<TEventArgs>(), handler))
                        {
                            return source;
                        }

                        combined.Combine(handler);

                        return combined;
                    }
            }

            return Empty<TEventArgs>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="source"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static WeakEventHandler<TEventArgs> Remove<TEventArgs>(WeakEventHandler<TEventArgs> source, EventHandler<TEventArgs> handler)
            where TEventArgs : EventArgs
        {
            if (null == source)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (null == handler)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            switch (source)
            {
                case SingleEventHandler<TEventArgs> target:
                    {
                        if (ReferenceEquals(target.Target, handler.Target))
                        {
                            return Empty<TEventArgs>();
                        }

                        return target;
                    }

                case CombinedEventHandler<TEventArgs> combined:
                    {
                        if (ReferenceEquals(Empty<TEventArgs>(), handler))
                        {
                            return source;
                        }

                        combined.Remove(handler);

                        return combined;
                    }
            }

            return Empty<TEventArgs>();
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class EmptyEventHandler<TEventArgs> : WeakEventHandler<TEventArgs>
            where TEventArgs : EventArgs
        {
            public static readonly EmptyEventHandler<TEventArgs> Empty;

            public override bool IsAlive => false;

            public override void Invoke(object sender, TEventArgs e)
            {
                ;
            }

            private EmptyEventHandler()
                : base(null)
            {
            }

            static EmptyEventHandler()
            {
                Empty = new EmptyEventHandler<TEventArgs>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class SingleEventHandler<TEventArgs> : WeakEventHandler<TEventArgs>
            where TEventArgs : EventArgs
        {
            private readonly WeakReference reference;

            public override bool IsAlive => reference.IsAlive;

            public object Target => reference.Target;

            public SingleEventHandler(EventHandler<TEventArgs> handler)
                : base(handler.Method)
            {
                reference = new WeakReference(handler.Target);
            }

            public override void Invoke(object sender, TEventArgs e)
            {
                if (false == reference.IsAlive)
                {
                    return;
                }

                var handler = CreateDelegate(reference);

                handler.DynamicInvoke(sender, e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class CombinedEventHandler<TEventArgs> : WeakEventHandler<TEventArgs>
            where TEventArgs : EventArgs
        {
            private readonly List<WeakReference> handlers;
            private readonly object gate;

            public override bool IsAlive
            {
                get
                {
                    lock (gate)
                    {
                        for (var index = 0; index < handlers.Count;)
                        {
                            if (false == handlers[index].IsAlive)
                            {
                                handlers.RemoveAt(index);
                                continue;
                            }

                            return true;
                        }
                    }

                    return false;
                }
            }

            public IReadOnlyCollection<WeakReference> Handlers
            {
                get
                {
                    var result = new List<WeakReference>();

                    lock (gate)
                    {
                        for (var index = 0; index < handlers.Count;)
                        {
                            if (false == handlers[index].IsAlive)
                            {
                                handlers.RemoveAt(index);
                                continue;
                            }

                            result.Add(handlers[index]);
                            index++;
                        }
                    }

                    return new ReadOnlyCollection<WeakReference>(result);
                }
            }

            public CombinedEventHandler(SingleEventHandler<TEventArgs> source, EventHandler<TEventArgs> handler)
                : this(handler.Method)
            {
                if (source.IsAlive)
                {
                    handlers.Add(new WeakReference(source.Target));
                }

                handlers.Add(new WeakReference(handler.Target));
            }

            private CombinedEventHandler(MethodInfo methodInfo)
                : base(methodInfo)
            {
                gate = new object();
                handlers = new List<WeakReference>();
            }

            public void Combine(EventHandler<TEventArgs> handler)
            {
                lock (gate)
                {
                    handlers.Add(new WeakReference(handler.Target));
                }
            }

            public void Remove(EventHandler<TEventArgs> handler)
            {
                lock (gate)
                {
                    var index = handlers.FindIndex(x => x.Target.Equals(handler.Target));

                    if (0 > index)
                    {
                        return;
                    }

                    handlers.RemoveAt(index);
                }
            }

            public override void Invoke(object sender, TEventArgs e)
            {
                foreach (var target in Handlers)
                {
                    var handler = CreateDelegate(target);
                    handler.DynamicInvoke(sender, e);
                }
            }
        }
    }
}