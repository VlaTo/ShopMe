namespace ShopMe.Effects.Interaction
{
    public class InteractionRequestContext
    {
        public static readonly InteractionRequestContext Empty;

        protected InteractionRequestContext()
        {
        }

        static InteractionRequestContext()
        {
            Empty = new EmptyRequestContext();
        }

        private sealed class EmptyRequestContext : InteractionRequestContext
        {
            public EmptyRequestContext()
            {
            }
        }
    }
}