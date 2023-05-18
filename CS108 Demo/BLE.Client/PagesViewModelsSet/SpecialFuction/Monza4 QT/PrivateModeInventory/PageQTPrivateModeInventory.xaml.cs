using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;


namespace BLE.Client.Pages
{
    public partial class PageQTPrivateModeInventory
    {
		public PageQTPrivateModeInventory()
        {
			InitializeComponent();
		}

        public async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var answer = await DisplayAlert("Select Tag", "Selected Tag for Read/Write and Geiger search", "OK", "Cancel");
            if (answer) {}
        }

    }
}
