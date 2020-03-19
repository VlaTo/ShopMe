using System.Linq;
using Android.Graphics;
using Android.Graphics.Drawables;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace ShopMe.Effects.Android.Controls.Renderers
{
    internal static class BorderRendererVisual
    {
        public static void UpdateBackground(BorderView touchView, global::Android.Views.View view)
        {
            var borderWidth = touchView.BorderWidth;
            var context = view.Context;

            GradientDrawable strokeDrawable = null;

            if (0 < borderWidth)
            {
                strokeDrawable = new GradientDrawable();
                strokeDrawable.SetColor(touchView.BackgroundColor.ToAndroid());

                strokeDrawable.SetStroke((int) context.ToPixels(borderWidth), touchView.BorderColor.ToAndroid());
                strokeDrawable.SetCornerRadius(context.ToPixels(touchView.CornerRadius));
            }

            var backgroundDrawable = new GradientDrawable();

            backgroundDrawable.SetColor(touchView.BackgroundColor.ToAndroid());
            backgroundDrawable.SetCornerRadius(context.ToPixels(touchView.CornerRadius));

            if (null != strokeDrawable)
            {
                var ld = new LayerDrawable(new Drawable[]
                {
                    strokeDrawable, 
                    backgroundDrawable
                });

                ld.SetLayerInset(
                    1,
                    (int) context.ToPixels(borderWidth),
                    (int) context.ToPixels(borderWidth),
                    (int) context.ToPixels(borderWidth),
                    (int) context.ToPixels(borderWidth)
                );
                
                //view.SetBackgroundDrawable(ld);
                view.Background = ld;
            }
            else
            {
                //view.SetBackgroundDrawable(backgroundDrawable);
                view.Background = backgroundDrawable;
            }

            view.SetPadding(
                (int) context.ToPixels(borderWidth + touchView.Padding.Left),
                (int) context.ToPixels(borderWidth + touchView.Padding.Top),
                (int) context.ToPixels(borderWidth + touchView.Padding.Right),
                (int) context.ToPixels(borderWidth + touchView.Padding.Bottom)
            );
        }

        public static void SetClipPath(BorderViewRenderer renderer, Canvas canvas)
        {
            var clipPath = new Path();
            var radius = renderer.Context.ToPixels(renderer.Element.CornerRadius) - renderer.Context.ToPixels((float) ThickestSide(renderer.Element.Padding));

            var w = renderer.Width;
            var h = renderer.Height;

            clipPath.AddRoundRect(new RectF(
                    renderer.ViewGroup.PaddingLeft,
                    renderer.ViewGroup.PaddingTop,
                    w - renderer.ViewGroup.PaddingRight,
                    h - renderer.ViewGroup.PaddingBottom),
                radius,
                radius,
                Path.Direction.Cw);

            canvas.ClipPath(clipPath);
        }

        private static double ThickestSide(Thickness thickness)
        {
            return new[] {
                thickness.Left,
                thickness.Top,
                thickness.Right,
                thickness.Bottom
            }.Max();
        }
    }
}