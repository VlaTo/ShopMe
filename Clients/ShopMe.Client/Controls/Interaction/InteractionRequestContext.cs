namespace ShopMe.Client.Controls.Interaction
{
    public class InteractionRequestContext
    {
        public static readonly InteractionRequestContext Empty;

        protected InteractionRequestContext()
        {
        }

        static InteractionRequestContext()
        {
            Empty = new InteractionRequestContext();
        }
    }
}