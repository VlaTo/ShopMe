using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShopMe.Client.Views
{
    [QueryProperty(nameof(ShopListId), "id")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShopListContentPage
    {
        [System.ComponentModel.TypeConverter(typeof(Int64Converter))]
        public long ShopListId
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