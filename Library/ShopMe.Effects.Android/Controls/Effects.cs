using ShopMe.Effects.Android.Controls.Renderers;

namespace ShopMe.Effects.Android.Controls
{
    public static class Effects
    {
        public static void Init()
        {
            TouchEffectPlatform.Init();
            //CommandsPlatform.Init();
            BorderViewRenderer.Init();
        }
    }
}