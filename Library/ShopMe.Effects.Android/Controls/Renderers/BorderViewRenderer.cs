using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Graphics;
using ShopMe.Effects;
using ShopMe.Effects.Android.Controls.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(BorderView), typeof(BorderViewRenderer))]

namespace ShopMe.Effects.Android.Controls.Renderers
{
    public class BorderViewRenderer : VisualElementRenderer<BorderView>
    {
        private static readonly string[] updatePropertyNames;

        public BorderViewRenderer(Context context)
            : base(context)
        {
        }

        static BorderViewRenderer()
        {
            updatePropertyNames = new[]
            {
                BorderView.BorderColorProperty.PropertyName,
                BorderView.BorderWidthProperty.PropertyName,
                BorderView.CornerRadiusProperty.PropertyName,
                VisualElement.BackgroundColorProperty.PropertyName
            };
        }

        public static void Init()
        {
            ;
        }

        protected override void DispatchDraw(Canvas canvas)
        {
            canvas.Save();
            //canvas.Save(SaveFlags.Clip);
            BorderRendererVisual.SetClipPath(this, canvas);
            
            base.DispatchDraw(canvas);
            
            canvas.Restore();
        }

        protected override void OnElementChanged(ElementChangedEventArgs<BorderView> e)
        {
            base.OnElementChanged(e);

            if (null == e.NewElement)
            {
                return;
            }

            BorderRendererVisual.UpdateBackground(Element, this);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (updatePropertyNames.Contains(e.PropertyName))
            {
                BorderRendererVisual.UpdateBackground(Element, this);
            }
        }
    }
}