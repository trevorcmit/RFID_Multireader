using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace BLE.Client.Pages
{
    [ContentProperty (nameof(Source))]
    public class ImageResourceExtension : IMarkupExtension
    {
        public string Source {get; set;}
        public object ProvideValue (IServiceProvider serviceProvider)
        {
            if (Source == null) return null;

            // Do your translation lookup here, using whatever method you require
            var imageSource = ImageSource.FromResource(Source);
            return imageSource;
        }
    }
    
    public partial class PageRFMicroS3Inventory
    {
		public PageRFMicroS3Inventory()
        {
			InitializeComponent();

            liewViewTagData.ItemSelected += (sender, e) =>
            {
                if (e.SelectedItem == null) return;     // Don't do anything if we just de-selected the row
                ((ListView)sender).SelectedItem = null; // De-select the row
            };

            // liewViewTagData2.ItemSelected += (sender, e) => {
            //     if (e.SelectedItem == null) return;
            //     ((ListView)sender).SelectedItem = null;
            // };
        }
        
    }
}
