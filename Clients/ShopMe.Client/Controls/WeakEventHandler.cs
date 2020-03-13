using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Unity;

namespace ShopMe.Client.Controls
{
    public abstract class WeakEventHandler<TEventArgs> where TEventArgs : EventArgs
    {
        public static readonly WeakEventHandler<TEventArgs> Empty;

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

        static WeakEventHandler()
        {
            Empty = new EmptyEventHandler();

            //EventHandler<EventArgs>.Combine(Delegate a, Delegate b)
            //EventHandler<EventArgs>.CreateDelegate(Type type, MethodInfo method)
            //temp.Method
            //temp.Target
            //temp.Invoke();
        }

        public abstract void Invoke(object sender, TEventArgs e);

        public static WeakEventHandler<TEventArgs> Combine(WeakEventHandler<TEventArgs> source, EventHandler<TEventArgs> handler)
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
                case EmptyEventHandler _:
                {
                    return new SingleEventHandler(handler);
                }

                case SingleEventHandler target:
                {
                    return new CombinedEventHandler(target, handler);
                }

                case CombinedEventHandler combined:
                {
                    if (ReferenceEquals(Empty, handler))
                    {
                        return source;
                    }

                    combined.Combine(handler);

                    return combined;
                }
            }

            return Empty;
        }

        public static WeakEventHandler<TEventArgs> Remove(WeakEventHandler<TEventArgs> source, EventHandler<TEventArgs> handler)
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
                case SingleEventHandler target:
                {
                    if (ReferenceEquals(target.Target, handler.Target))
                    {
                        return Empty;
                    }

                    return target;
                }

                case CombinedEventHandler combined:
                {
                    if (ReferenceEquals(Empty, handler))
                    {
                        return source;
                    }

                    combined.Remove(handler);

                    return combined;
                }
            }

            return Empty;
        }

        private Delegate CreateDelegate(WeakReference target)
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

        /// <summary>
        /// 
        /// </summary>
        private class EmptyEventHandler : WeakEventHandler<TEventArgs>
        {
            public override bool IsAlive => false;

            public override void Invoke(object sender, TEventArgs e)
            {
                ;
            }

            public EmptyEventHandler()
                : base(null)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class SingleEventHandler : WeakEventHandler<TEventArgs>
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
        private class CombinedEventHandler : WeakEventHandler<TEventArgs>
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

            public CombinedEventHandler(SingleEventHandler source, EventHandler<TEventArgs> handler)
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