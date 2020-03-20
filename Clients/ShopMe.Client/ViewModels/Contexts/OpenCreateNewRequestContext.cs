using System.Threading.Tasks;
using ShopMe.Effects.Interaction;

namespace ShopMe.Client.ViewModels.Contexts
{
    public sealed class OpenCreateNewRequestContext : InteractionRequestContext
    {
        private readonly TaskCompletionSource<bool> completed;

        public OpenCreateNewRequestContext(TaskCompletionSource<bool> tcs)
        {
            completed = tcs ?? new TaskCompletionSource<bool>();
        }

        public void SetResult(string title)
        {
            completed.SetResult(true);
        }

        public void Cancel()
        {
            completed.SetResult(false);
        }
    }
}