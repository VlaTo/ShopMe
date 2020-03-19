using System;
using System.Collections.Generic;
using Android.Views;

namespace ShopMe.Effects.Android.Controls.GestureCollectors
{
    internal static class TouchCollector
    {
        private static Dictionary<View, List<Action<View.TouchEventArgs>>> Collection
        {
            get;
        }

        private static View activeView;

        static TouchCollector()
        {
            Collection = new Dictionary<View, List<Action<View.TouchEventArgs>>>();
        }

        public static void Add(View view, Action<View.TouchEventArgs> action)
        {
            if (Collection.ContainsKey(view))
            {
                Collection[view].Add(action);
            }
            else
            {
                view.Touch += ActionActivator;
                Collection.Add(view, new List<Action<View.TouchEventArgs>> { action });
            }
        }

        public static void Delete(View view, Action<View.TouchEventArgs> action)
        {
            if (false == Collection.ContainsKey(view))
            {
                return;
            }

            var actions = Collection[view];
            actions.Remove(action);

            if (0 < actions.Count)
            {
                return;
            }

            view.Touch -= ActionActivator;

            Collection.Remove(view);
        }

        private static void ActionActivator(object sender, View.TouchEventArgs e)
        {
            var view = (View)sender;

            if (false == Collection.ContainsKey(view) || (null != activeView && activeView != view))
            {
                return;
            }

            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                {
                    activeView = view;
                    view.PlaySoundEffect(SoundEffects.Click);
                    
                    break;
                }

                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                {
                    activeView = null;
                    e.Handled = true;

                    break;
                }
            }

            var actions = Collection[view].ToArray();

            foreach (var action in actions)
            {
                action?.Invoke(e);
            }
        }
    }
}