using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace ShopMe.Client.Controls
{
    public abstract class WeakEventHandler<TEventArgs> where TEventArgs : EventArgs
    {
        public static readonly WeakEventHandler<TEventArgs> Empty;

        public abstract bool IsAlive
        {
            get;
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
        }

        /// <summary>
        /// 
        /// </summary>
        private class SingleEventHandler : WeakEventHandler<TEventArgs>
        {
            private readonly WeakReference target;
            private readonly MethodInfo method;

            public override bool IsAlive => target.IsAlive;

            public object Target => target.Target;

            public SingleEventHandler(EventHandler<TEventArgs> handler)
            {
                target = new WeakReference(handler.Target);
                method = handler.Method;
            }

            public override void Invoke(object sender, TEventArgs e)
            {
                if (false == target.IsAlive)
                {
                    return;
                }

                var handler = target.Target;
                var eventHandler = Delegate.CreateDelegate(typeof(EventHandler<TEventArgs>), method);

                eventHandler.DynamicInvoke(handler);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class CombinedEventHandler : WeakEventHandler<TEventArgs>
        {
            private readonly MethodInfo method;
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

            private CombinedEventHandler(MethodInfo method)
            {
                this.method = method;
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
                var eventHandler = Delegate.CreateDelegate(typeof(EventHandler<TEventArgs>), method);
                var targets = Handlers;

                foreach (var target in targets)
                {
                    eventHandler.DynamicInvoke(target);
                }
            }
        }
    }
}