using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace BLE.Client.Pages {
    [ContentProperty (nameof(Source))]
    public class ImageResourceExtension : IMarkupExtension {
        public string Source { get; set; }
        public object ProvideValue (IServiceProvider serviceProvider) {
            if (Source == null)
                return null;
            var imageSource = ImageSource.FromResource(Source);
            return imageSource;
        }

    }
    
    public partial class PageRFMicroS3Inventory {
		public PageRFMicroS3Inventory() {
			InitializeComponent();

            liewViewTagData.ItemSelected += (sender, e) => {
                if (e.SelectedItem == null) return; // don't do anything if we just de-selected the row
                ((ListView)sender).SelectedItem = null; // de-select the row
            };
        }

    }
}
