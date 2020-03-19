using System;
using Android.Animation;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using ShopMe.Effects;
using ShopMe.Effects.Android.Controls;
using ShopMe.Effects.Android.Controls.GestureCollectors;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;
using ListView = Android.Widget.ListView;
using ScrollView = Android.Widget.ScrollView;
using View = Android.Views.View;

[assembly: ExportEffect(typeof(TouchEffectPlatform), nameof(TouchEffect))]

namespace ShopMe.Effects.Android.Controls
{
    public class TouchEffectPlatform : PlatformEffect
    {
        private Color effectColor;
        private byte effectAlpha;
        private RippleDrawable ripple;
        private FrameLayout viewOverlay;
        private ObjectAnimator animator;

        public bool EnableRipple => Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop;

        public bool IsDisposed => null == (Container as IVisualElementRenderer)?.Element;

        public View View => Control ?? Container;

        public static void Init()
        {
            var temp = nameof(TouchEffect);
            ;
        }

        protected override void OnAttached()
        {
            if (Control is ListView || Control is ScrollView)
            {
                return;
            }

            View.Clickable = true;
            View.LongClickable = true;

            viewOverlay = new FrameLayout(Container.Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(-1, -1),
                Clickable = false,
                Focusable = false,
            };
            Container.LayoutChange += OnViewLayoutChange;

            if (EnableRipple)
            {
                viewOverlay.Background = CreateRipple(effectColor);
            }

            SetEffectColor();
            TouchCollector.Add(View, OnViewTouch);

            Container.AddView(viewOverlay);
            viewOverlay.BringToFront();
        }

        protected override void OnDetached()
        {
            if (IsDisposed)
            {
                return;
            }

            Container.RemoveView(viewOverlay);

            viewOverlay.Pressed = false;
            viewOverlay.Foreground = null;
            viewOverlay.Dispose();

            Container.LayoutChange -= OnViewLayoutChange;

            if (EnableRipple)
            {
                ripple?.Dispose();
            }

            TouchCollector.Delete(View, OnViewTouch);
        }

        private void OnViewTouch(View.TouchEventArgs args)
        {
            switch (args.Event.Action)
            {
                case MotionEventActions.Down:
                {
                    if (EnableRipple)
                    {
                        ForceStartRipple(args.Event.GetX(), args.Event.GetY());
                    }
                    else
                    {
                        BringLayer();
                    }

                    break;
                }

                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                {
                    if (IsDisposed)
                    {
                        return;
                    }

                    if (EnableRipple)
                    {
                        ForceEndRipple();
                    }
                    else
                    {
                        TapAnimation(250, effectAlpha, 0);
                    }

                    break;
                }
            }
        }

        private void OnViewLayoutChange(object sender, View.LayoutChangeEventArgs _)
        {
            var group = (ViewGroup) sender;

            if (group == null || IsDisposed)
            {
                return;
            }

            viewOverlay.Right = group.Width;
            viewOverlay.Bottom = group.Height;
        }
        
        private RippleDrawable CreateRipple(Color color)
        {
            if (Element is Layout)
            {
                var mask = new ColorDrawable(Color.White);
                ripple = new RippleDrawable(GetPressedColorSelector(color), null, mask);
                
                return ripple;
            }

            var background = View.Background;

            if (null == background)
            {
                var mask = new ColorDrawable(Color.White);
                ripple = new RippleDrawable(GetPressedColorSelector(color), null, mask);

                return ripple;
            }

            if (background is RippleDrawable)
            {
                ripple = (RippleDrawable) background.GetConstantState().NewDrawable();
                ripple.SetColor(GetPressedColorSelector(color));

                return ripple;
            }

            ripple = new RippleDrawable(GetPressedColorSelector(color), background, null);

            return ripple;
        }
        private void ForceStartRipple(float x, float y)
        {
            if (IsDisposed || !(viewOverlay.Background is RippleDrawable drawable))
            {
                return;
            }

            viewOverlay.BringToFront();

            drawable.SetHotspot(x, y);

            viewOverlay.Pressed = true;
        }

        private void ForceEndRipple()
        {
            if (IsDisposed)
            {
                return;
            }

            viewOverlay.Pressed = false;
        }

        private void BringLayer()
        {
            if (IsDisposed)
            {
                return;
            }

            ClearAnimation();

            viewOverlay.BringToFront();

            var color = effectColor;
            color.A = effectAlpha;
            
            viewOverlay.SetBackgroundColor(color);
        }

        private void TapAnimation(long duration, byte startAlpha, byte endAlpha)
        {
            if (IsDisposed)
            {
                return;
            }

            viewOverlay.BringToFront();

            var start = effectColor;
            var end = effectColor;

            start.A = startAlpha;
            end.A = endAlpha;

            ClearAnimation();

            animator = ObjectAnimator.OfObject(
                viewOverlay,
                "BackgroundColor",
                new ArgbEvaluator(),
                start.ToArgb(),
                end.ToArgb()
            );

            animator.RepeatCount = 0;
            animator.RepeatMode = ValueAnimatorRepeatMode.Restart;
            animator.AnimationEnd += AnimationOnAnimationEnd;
            animator.SetDuration(duration);
            animator.Start();
        }

        private void AnimationOnAnimationEnd(object sender, EventArgs eventArgs)
        {
            if (IsDisposed)
            {
                return;
            }

            ClearAnimation();
        }

        private void ClearAnimation()
        {
            if (null == animator)
            {
                return;
            }

            animator.AnimationEnd -= AnimationOnAnimationEnd;
            animator.Cancel();
            animator.Dispose();
            
            animator = null;
        }

        private void SetEffectColor()
        {
            var color = TouchEffect.GetColor(Element);

            if (Xamarin.Forms.Color.Default == color)
            {
                return;
            }

            effectColor = color.ToAndroid();
            effectAlpha = (byte) (effectColor.A == 255 ? 80 : effectColor.A);

            if (EnableRipple)
            {
                ripple.SetColor(GetPressedColorSelector(effectColor));
            }
        }

        private static ColorStateList GetPressedColorSelector(int pressedColor)
        {
            return new ColorStateList(
                new[] { new int[] { } },
                new[] { pressedColor, });
        }
    }
}