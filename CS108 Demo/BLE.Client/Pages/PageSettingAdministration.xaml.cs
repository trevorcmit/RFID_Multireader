using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
using Xamarin.Forms;
// using Xamarin.Forms.Xaml;


namespace BLE.Client.Pages {
	public partial class PageSettingAdministration {
        bool entryReaderNameModified = false;

        public PageSettingAdministration() {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS) {
                this.Icon = new FileImageSource();
                this.Icon.File = "icons8-Settings-50-2-30x30.png";
            }

            switch (BleMvxApplication._config1.BatteryLevelIndicatorFormat) {
                case 0:
                    buttonBatteryLevelFormat.Text = "Voltage";
                    break;

                default:
                    buttonBatteryLevelFormat.Text = "Percentage";
                    break;
            }

            switchInventoryAlertSound.IsToggled = BleMvxApplication._config1.RFID_InventoryAlertSound;

            F1.Text = BleMvxApplication._config1.RFID_Shortcut[0].Function.ToString();
            F1MinTime.Text = BleMvxApplication._config1.RFID_Shortcut[0].DurationMin.ToString();
            F1MaxTime.Text = BleMvxApplication._config1.RFID_Shortcut[0].DurationMax.ToString();
            F2.Text = BleMvxApplication._config1.RFID_Shortcut[1].Function.ToString();
            F2MinTime.Text = BleMvxApplication._config1.RFID_Shortcut[1].DurationMin.ToString();
            F2MaxTime.Text = BleMvxApplication._config1.RFID_Shortcut[1].DurationMax.ToString();

            entryReaderName.Text = BleMvxApplication._reader1.ReaderName;
            labelReaderModel.Text = "Reader Model : " + BleMvxApplication._reader1.rfid.GetModelName() + BleMvxApplication._reader1.rfid.GetCountryCode();

            switchNewTagLocation.IsToggled = BleMvxApplication._config1.RFID_NewTagLocation;
            switchShareDataFormat.IsToggled = (BleMvxApplication._config1.RFID_ShareFormat == 0) ? false : true;
            switchRSSIDBm.IsToggled = BleMvxApplication._config1.RFID_DBm;
            switchSavetoCloud.IsToggled = BleMvxApplication._config1.RFID_SavetoCloud;
            switchhttpProtocol.IsToggled = (BleMvxApplication._config1.RFID_CloudProtocol == 0) ? false : true;
            entryServerIP.Text = BleMvxApplication._config1.RFID_IPAddress;
            switchVibration.IsToggled = BleMvxApplication._config1.RFID_Vibration;
            switchVibrationTag.IsToggled = BleMvxApplication._config1.RFID_VibrationTag;
            entryVibrationWindow.Text = BleMvxApplication._config1.RFID_VibrationWindow.ToString();
            entryVibrationTime.Text = BleMvxApplication._config1.RFID_VibrationTime.ToString();
        }

        protected override void OnAppearing() {
            base.OnAppearing();
        }

        public async void btnOKClicked(object sender, EventArgs e) {
            int cnt;

            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            switch (buttonBatteryLevelFormat.Text) {
                case "Voltage":
                    BleMvxApplication._config1.BatteryLevelIndicatorFormat = 0;
                    break;

                default:
                    BleMvxApplication._config1.BatteryLevelIndicatorFormat = 1;
                    break;
            }

            BleMvxApplication._config1.RFID_InventoryAlertSound = switchInventoryAlertSound.IsToggled;

            BleMvxApplication._config1.RFID_Shortcut[0].Function = (CONFIG.MAINMENUSHORTCUT.FUNCTION)Enum.Parse(typeof(CONFIG.MAINMENUSHORTCUT.FUNCTION), F1.Text);
            BleMvxApplication._config1.RFID_Shortcut[0].DurationMin = uint.Parse(F1MinTime.Text);
            BleMvxApplication._config1.RFID_Shortcut[0].DurationMax = uint.Parse(F1MaxTime.Text);
            BleMvxApplication._config1.RFID_Shortcut[1].Function = (CONFIG.MAINMENUSHORTCUT.FUNCTION)Enum.Parse(typeof(CONFIG.MAINMENUSHORTCUT.FUNCTION), F2.Text);
            BleMvxApplication._config1.RFID_Shortcut[1].DurationMin = uint.Parse(F2MinTime.Text);
            BleMvxApplication._config1.RFID_Shortcut[1].DurationMax = uint.Parse(F2MaxTime.Text);

            BleMvxApplication._config1.RFID_DBm = switchRSSIDBm.IsToggled;
            //BleMvxApplication._config.RFID_SavetoFile = switchSavetoFile.IsToggled;
            BleMvxApplication._config1.RFID_SavetoCloud = switchSavetoCloud.IsToggled;
            BleMvxApplication._config1.RFID_CloudProtocol = switchhttpProtocol.IsToggled ? 1 : 0;
            BleMvxApplication._config1.RFID_IPAddress = entryServerIP.Text;

            BleMvxApplication._config1.RFID_NewTagLocation = switchNewTagLocation.IsToggled;
            BleMvxApplication._config1.RFID_ShareFormat = switchShareDataFormat.IsToggled ? 1 : 0;

            //BleMvxApplication._config.RFID_TagDelayTime = int.Parse(entryTagDelay.Text);
            //BleMvxApplication._config.RFID_InventoryDuration = UInt32.Parse(entryInventoryDuration.Text);

            BleMvxApplication._config1.RFID_Vibration = switchVibration.IsToggled;
            BleMvxApplication._config1.RFID_VibrationTag = switchVibrationTag.IsToggled;
            BleMvxApplication._config1.RFID_VibrationWindow = UInt32.Parse(entryVibrationWindow.Text);
            BleMvxApplication._config1.RFID_VibrationTime = UInt32.Parse(entryVibrationTime.Text);

            //BleMvxApplication._config.RFID_BatteryPollingTime = uint.Parse(entryBatteryIntervalTime.Text);

            BleMvxApplication.SaveConfig(1);

            if (entryReaderNameModified) {
                BleMvxApplication._reader1.bluetoothIC.SetDeviceName (entryReaderName.Text);
                entryReaderNameModified = false;
                await DisplayAlert("New Reader Name effective after reset CS108", "", null, "OK");
            }
        }

        public async void buttonBatteryLevelFormatClicked(object sender, EventArgs e) {
            var answer = await DisplayActionSheet("View Battery Level Format", "Cancel", null, "Voltage", "Percentage");

            if (answer != null && answer !="Cancel") buttonBatteryLevelFormat.Text = answer;
        }

        public void btnBarcodeResetClicked(object sender, EventArgs e) {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            if (BleMvxApplication._reader1.barcode.state == CSLibrary.BarcodeReader.STATE.NOTVALID) {
                DisplayAlert(null, "Barcode module not exists", "OK");
                return;
            }

            if (BleMvxApplication._reader2.barcode.state == CSLibrary.BarcodeReader.STATE.NOTVALID) {
                DisplayAlert(null, "Barcode module not exists", "OK");
                return;
            }

            if (BleMvxApplication._reader3.barcode.state == CSLibrary.BarcodeReader.STATE.NOTVALID) {
                DisplayAlert(null, "Barcode module not exists", "OK");
                return;
            }

            if (BleMvxApplication._reader4.barcode.state == CSLibrary.BarcodeReader.STATE.NOTVALID) {
                DisplayAlert(null, "Barcode module not exists", "OK");
                return;
            }

            BleMvxApplication._reader1.barcode.FactoryReset();
            BleMvxApplication._reader2.barcode.FactoryReset();
            BleMvxApplication._reader3.barcode.FactoryReset();
            BleMvxApplication._reader4.barcode.FactoryReset();
        }

        public async void btnConfigResetClicked(object sender, EventArgs e) {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);
            BleMvxApplication.ResetConfig(1);
            BleMvxApplication._reader1.rfid.SetDefaultChannel();

            BleMvxApplication._config1.RFID_Region = BleMvxApplication._reader1.rfid.SelectedRegionCode;

            if (BleMvxApplication._reader1.rfid.IsFixedChannel) {
                BleMvxApplication._config1.RFID_FrequenceSwitch = 1;
                BleMvxApplication._config1.RFID_FixedChannel = BleMvxApplication._reader1.rfid.SelectedChannel;
            }
            else {
                BleMvxApplication._config1.RFID_FrequenceSwitch = 0; // Hopping
            }

            BleMvxApplication.SaveConfig(1);
        }

        public async void btnGetSerialNumber(object sender, EventArgs e) {
            BleMvxApplication._reader1.siliconlabIC.GetSerialNumber();
        }

        public async void btnFunctionSelectedClicked(object sender, EventArgs e) {
            var answer = await DisplayActionSheet(null, BLE.Client.CONFIG.MAINMENUSHORTCUT.FUNCTION.NONE.ToString(), null, BLE.Client.CONFIG.MAINMENUSHORTCUT.FUNCTION.INVENTORY.ToString(), BLE.Client.CONFIG.MAINMENUSHORTCUT.FUNCTION.BARCODE.ToString());

            Button b = (Button)sender;
            b.Text = answer;
        }

        public async void entryReaderNameCompleted(object sender, EventArgs e) {
            entryReaderNameModified = true;
        }

        public async void btnCSLCloudClicked(object sender, EventArgs e) {
            switchhttpProtocol.IsToggled = false;
            //entryServerIP.Text = "https://www.convergence.com.hk:29090/WebServiceRESTs/1.0/req";
            entryServerIP.Text = "https://democloud.convergence.com.hk:29090/WebServiceRESTs/1.0/req";
        }

    }
}
