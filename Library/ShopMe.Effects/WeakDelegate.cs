using System;
using System.Reflection;

namespace ShopMe.Effects
{
    internal sealed class WeakDelegate<TDelegate> : IEquatable<TDelegate>
        where TDelegate : Delegate
    {
        private readonly WeakReference target;
        private readonly MethodInfo method;

        public bool IsAlive => null == target || target.IsAlive;

        public WeakDelegate(Delegate @delegate)
        {
            if (null != @delegate.Target)
            {
                target = new WeakReference(@delegate.Target);
            }

            method = @delegate.Method;
        }

        public TDelegate GetDelegate() => CreateDelegate();

        public void Invoke(params object[] parameters)
        {
            var handler = CreateDelegate();
            handler.DynamicInvoke(parameters);
        }

        public bool Equals(TDelegate other)
        {
            var @delegate = (Delegate) (object) other;
            return null != @delegate && @delegate.Target == target.Target && @delegate.Method.Equals(method);
        }

        public static explicit operator TDelegate(WeakDelegate<TDelegate> @this) => @this.GetDelegate();

        private TDelegate CreateDelegate()
        {
            if (null == target)
            {
                return (TDelegate) Delegate.CreateDelegate(typeof(TDelegate), method);
            }

            return (TDelegate) Delegate.CreateDelegate(typeof(TDelegate), target.Target, method);
        }
    }
}