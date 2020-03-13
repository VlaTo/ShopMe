using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShopMe.Client.Views
{
    //[QueryProperty(nameof(Id), "id")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShopListContentPage
    {
        public long Id
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