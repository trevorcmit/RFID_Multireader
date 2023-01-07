using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

// using System.Collections.ObjectModel;
// using System.ComponentModel;
// using System.IO;
// using System.Linq;
// using System.Windows.Input;
// using Xamarin;
// using Xamarin.Essentials;


namespace BLE.Client.Pages {
    [ContentProperty (nameof(Source))]
    public class ImageResourceExtension : IMarkupExtension {
        public string Source { get; set; }
        public object ProvideValue (IServiceProvider serviceProvider) {
            if (Source == null) return null;

            // Do your translation lookup here, using whatever method you require
            var imageSource = ImageSource.FromResource(Source);
            return imageSource;
        }

    }
    
    public partial class PageRFMicroS3Inventory {
		public PageRFMicroS3Inventory() {
			InitializeComponent();

            var pickerList1 = new List<string>{
                "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten"
            };

            var pickerList2 = new List<string>{
                "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten"
            };

            picker1.ItemsSource = pickerList1;
            picker2.ItemsSource = pickerList2;

            picker1.SelectedIndexChanged += (sender, args) =>
            {
                switch (picker1.SelectedIndex) {
                    case 0:
                        MessagingCenter.Send<PageRFMicroS3Inventory, string>(this, "Picker1", "0");
                        break;
                    case 1:
                        MessagingCenter.Send<PageRFMicroS3Inventory, string>(this, "Picker1", "1");
                        break;
                    case 2:
                        MessagingCenter.Send<PageRFMicroS3Inventory, string>(this, "Picker1", "2");
                        break;
                    case 3:
                        MessagingCenter.Send<PageRFMicroS3Inventory, string>(this, "Picker1", "3");
                        break;
                    case 4:
                        MessagingCenter.Send<PageRFMicroS3Inventory, string>(this, "Picker1", "4");
                        break;
                    case 5:
                        MessagingCenter.Send<PageRFMicroS3Inventory, string>(this, "Picker1", "5");
                        break;
                    case 6:
                        MessagingCenter.Send<PageRFMicroS3Inventory, string>(this, "Picker1", "6");
                        break;
                    case 7:
                        MessagingCenter.Send<PageRFMicroS3Inventory, string>(this, "Picker1", "7");
                        break;
                    case 8:
                        MessagingCenter.Send<PageRFMicroS3Inventory, string>(this, "Picker1", "8");
                        break;
                    case 9:
                        MessagingCenter.Send<PageRFMicroS3Inventory, string>(this, "Picker1", "9");
                        break;
                    default:
                        break;
                }
            };

        }

    }
}
