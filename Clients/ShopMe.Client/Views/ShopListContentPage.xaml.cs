using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShopMe.Client.Views
{
    [QueryProperty(nameof(ShopListId), "id")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShopListContentPage
    {
        public string ShopListId
        {
            get; 
            set;
        }

        public ShopListContentPage()
        {
            InitializeComponent();
        }
    }
}