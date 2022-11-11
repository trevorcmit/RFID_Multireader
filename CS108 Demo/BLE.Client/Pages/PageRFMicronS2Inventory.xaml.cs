using Xamarin.Forms;


namespace BLE.Client.Pages {
    public partial class PageRFMicroS2Inventory {
		public PageRFMicroS2Inventory() {
			InitializeComponent();
            liewViewTagData.ItemSelected += (sender, e) => {
                if (e.SelectedItem == null) return; // don't do anything if we just de-selected the row
                ((ListView)sender).SelectedItem = null; // de-select the row
            };
        }
    }
}
