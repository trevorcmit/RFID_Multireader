using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace BLE.Client.Pages {
	public partial class PageSettingAntenna {
        class ANTENNAOPTION {
            private global::Xamarin.Forms.Switch switchAntennaEnable;
            private global::Xamarin.Forms.Entry entryPower;
            private global::Xamarin.Forms.Entry entryDwell;
        }

        public PageSettingAntenna() {
            InitializeComponent();

            // the page only support 4 ports
            if (BleMvxApplication._reader1.rfid.GetAntennaPort() != 4) return;
            if (BleMvxApplication._reader2.rfid.GetAntennaPort() != 4) return;
            if (BleMvxApplication._reader3.rfid.GetAntennaPort() != 4) return;
            if (BleMvxApplication._reader4.rfid.GetAntennaPort() != 4) return;
            if (Device.RuntimePlatform == Device.iOS) {
                this.Icon = new FileImageSource();
                this.Icon.File = "icons8-Settings-50-3-30x30.png";
            }

            ANTENNAOPTION[] antennaOptions = new ANTENNAOPTION[BleMvxApplication._reader1.rfid.AntennaList.Count];

            switchAntenna1Enable.IsToggled = BleMvxApplication._config1.RFID_AntennaEnable[0];
            switchAntenna2Enable.IsToggled = BleMvxApplication._config1.RFID_AntennaEnable[1];
            switchAntenna3Enable.IsToggled = BleMvxApplication._config1.RFID_AntennaEnable[2];
            switchAntenna4Enable.IsToggled = BleMvxApplication._config1.RFID_AntennaEnable[3];
            switchAntenna1Enable.IsToggled = BleMvxApplication._config2.RFID_AntennaEnable[0];
            switchAntenna2Enable.IsToggled = BleMvxApplication._config2.RFID_AntennaEnable[1];
            switchAntenna3Enable.IsToggled = BleMvxApplication._config2.RFID_AntennaEnable[2];
            switchAntenna4Enable.IsToggled = BleMvxApplication._config2.RFID_AntennaEnable[3];
            switchAntenna1Enable.IsToggled = BleMvxApplication._config3.RFID_AntennaEnable[0];
            switchAntenna2Enable.IsToggled = BleMvxApplication._config3.RFID_AntennaEnable[1];
            switchAntenna3Enable.IsToggled = BleMvxApplication._config3.RFID_AntennaEnable[2];
            switchAntenna4Enable.IsToggled = BleMvxApplication._config3.RFID_AntennaEnable[3];
            switchAntenna1Enable.IsToggled = BleMvxApplication._config4.RFID_AntennaEnable[0];
            switchAntenna2Enable.IsToggled = BleMvxApplication._config4.RFID_AntennaEnable[1];
            switchAntenna3Enable.IsToggled = BleMvxApplication._config4.RFID_AntennaEnable[2];
            switchAntenna4Enable.IsToggled = BleMvxApplication._config4.RFID_AntennaEnable[3];

            entryPower1.Text = BleMvxApplication._config1.RFID_Antenna_Power[0].ToString();
            entryPower2.Text = BleMvxApplication._config1.RFID_Antenna_Power[1].ToString();
            entryPower3.Text = BleMvxApplication._config1.RFID_Antenna_Power[2].ToString();
            entryPower4.Text = BleMvxApplication._config1.RFID_Antenna_Power[3].ToString();
            entryPower1.Text = BleMvxApplication._config2.RFID_Antenna_Power[0].ToString();
            entryPower2.Text = BleMvxApplication._config2.RFID_Antenna_Power[1].ToString();
            entryPower3.Text = BleMvxApplication._config2.RFID_Antenna_Power[2].ToString();
            entryPower4.Text = BleMvxApplication._config2.RFID_Antenna_Power[3].ToString();
            entryPower1.Text = BleMvxApplication._config3.RFID_Antenna_Power[0].ToString();
            entryPower2.Text = BleMvxApplication._config3.RFID_Antenna_Power[1].ToString();
            entryPower3.Text = BleMvxApplication._config3.RFID_Antenna_Power[2].ToString();
            entryPower4.Text = BleMvxApplication._config3.RFID_Antenna_Power[3].ToString();
            entryPower1.Text = BleMvxApplication._config4.RFID_Antenna_Power[0].ToString();
            entryPower2.Text = BleMvxApplication._config4.RFID_Antenna_Power[1].ToString();
            entryPower3.Text = BleMvxApplication._config4.RFID_Antenna_Power[2].ToString();
            entryPower4.Text = BleMvxApplication._config4.RFID_Antenna_Power[3].ToString();

            entryDwell1.Text = BleMvxApplication._config1.RFID_Antenna_Dwell[0].ToString();
            entryDwell2.Text = BleMvxApplication._config1.RFID_Antenna_Dwell[1].ToString();
            entryDwell3.Text = BleMvxApplication._config1.RFID_Antenna_Dwell[2].ToString();
            entryDwell4.Text = BleMvxApplication._config1.RFID_Antenna_Dwell[3].ToString();
            entryDwell1.Text = BleMvxApplication._config2.RFID_Antenna_Dwell[0].ToString();
            entryDwell2.Text = BleMvxApplication._config2.RFID_Antenna_Dwell[1].ToString();
            entryDwell3.Text = BleMvxApplication._config2.RFID_Antenna_Dwell[2].ToString();
            entryDwell4.Text = BleMvxApplication._config2.RFID_Antenna_Dwell[3].ToString();
            entryDwell1.Text = BleMvxApplication._config3.RFID_Antenna_Dwell[0].ToString();
            entryDwell2.Text = BleMvxApplication._config3.RFID_Antenna_Dwell[1].ToString();
            entryDwell3.Text = BleMvxApplication._config3.RFID_Antenna_Dwell[2].ToString();
            entryDwell4.Text = BleMvxApplication._config3.RFID_Antenna_Dwell[3].ToString();
            entryDwell1.Text = BleMvxApplication._config4.RFID_Antenna_Dwell[0].ToString();
            entryDwell2.Text = BleMvxApplication._config4.RFID_Antenna_Dwell[1].ToString();
            entryDwell3.Text = BleMvxApplication._config4.RFID_Antenna_Dwell[2].ToString();
            entryDwell4.Text = BleMvxApplication._config4.RFID_Antenna_Dwell[3].ToString();
        }

        protected override void OnAppearing()
        {
            if (BleMvxApplication._settingPage1TagPopulationChanged)
            {
                BleMvxApplication._settingPage1TagPopulationChanged = false;
            }
            base.OnAppearing();
        }

        public async void btnOKClicked(object sender, EventArgs e) {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            BleMvxApplication._config1.RFID_AntennaEnable[0] = switchAntenna1Enable.IsToggled;
            BleMvxApplication._config1.RFID_AntennaEnable[1] = switchAntenna2Enable.IsToggled;
            BleMvxApplication._config1.RFID_AntennaEnable[2] = switchAntenna3Enable.IsToggled;
            BleMvxApplication._config1.RFID_AntennaEnable[3] = switchAntenna4Enable.IsToggled;
            BleMvxApplication._config2.RFID_AntennaEnable[0] = switchAntenna1Enable.IsToggled;
            BleMvxApplication._config2.RFID_AntennaEnable[1] = switchAntenna2Enable.IsToggled;
            BleMvxApplication._config2.RFID_AntennaEnable[2] = switchAntenna3Enable.IsToggled;
            BleMvxApplication._config2.RFID_AntennaEnable[3] = switchAntenna4Enable.IsToggled;
            BleMvxApplication._config3.RFID_AntennaEnable[0] = switchAntenna1Enable.IsToggled;
            BleMvxApplication._config3.RFID_AntennaEnable[1] = switchAntenna2Enable.IsToggled;
            BleMvxApplication._config3.RFID_AntennaEnable[2] = switchAntenna3Enable.IsToggled;
            BleMvxApplication._config3.RFID_AntennaEnable[3] = switchAntenna4Enable.IsToggled;
            BleMvxApplication._config4.RFID_AntennaEnable[0] = switchAntenna1Enable.IsToggled;
            BleMvxApplication._config4.RFID_AntennaEnable[1] = switchAntenna2Enable.IsToggled;
            BleMvxApplication._config4.RFID_AntennaEnable[2] = switchAntenna3Enable.IsToggled;
            BleMvxApplication._config4.RFID_AntennaEnable[3] = switchAntenna4Enable.IsToggled;

            BleMvxApplication._config1.RFID_Antenna_Power[0] = uint.Parse(entryPower1.Text);
            BleMvxApplication._config1.RFID_Antenna_Power[1] = uint.Parse(entryPower2.Text);
            BleMvxApplication._config1.RFID_Antenna_Power[2] = uint.Parse(entryPower3.Text);
            BleMvxApplication._config1.RFID_Antenna_Power[3] = uint.Parse(entryPower4.Text);
            BleMvxApplication._config2.RFID_Antenna_Power[0] = uint.Parse(entryPower1.Text);
            BleMvxApplication._config2.RFID_Antenna_Power[1] = uint.Parse(entryPower2.Text);
            BleMvxApplication._config2.RFID_Antenna_Power[2] = uint.Parse(entryPower3.Text);
            BleMvxApplication._config2.RFID_Antenna_Power[3] = uint.Parse(entryPower4.Text);
            BleMvxApplication._config3.RFID_Antenna_Power[0] = uint.Parse(entryPower1.Text);
            BleMvxApplication._config3.RFID_Antenna_Power[1] = uint.Parse(entryPower2.Text);
            BleMvxApplication._config3.RFID_Antenna_Power[2] = uint.Parse(entryPower3.Text);
            BleMvxApplication._config3.RFID_Antenna_Power[3] = uint.Parse(entryPower4.Text);
            BleMvxApplication._config4.RFID_Antenna_Power[0] = uint.Parse(entryPower1.Text);
            BleMvxApplication._config4.RFID_Antenna_Power[1] = uint.Parse(entryPower2.Text);
            BleMvxApplication._config4.RFID_Antenna_Power[2] = uint.Parse(entryPower3.Text);
            BleMvxApplication._config4.RFID_Antenna_Power[3] = uint.Parse(entryPower4.Text);

            BleMvxApplication._config1.RFID_Antenna_Dwell[0] = uint.Parse(entryDwell1.Text);
            BleMvxApplication._config1.RFID_Antenna_Dwell[1] = uint.Parse(entryDwell2.Text);
            BleMvxApplication._config1.RFID_Antenna_Dwell[2] = uint.Parse(entryDwell3.Text);
            BleMvxApplication._config1.RFID_Antenna_Dwell[3] = uint.Parse(entryDwell4.Text);
            BleMvxApplication._config2.RFID_Antenna_Dwell[0] = uint.Parse(entryDwell1.Text);
            BleMvxApplication._config2.RFID_Antenna_Dwell[1] = uint.Parse(entryDwell2.Text);
            BleMvxApplication._config2.RFID_Antenna_Dwell[2] = uint.Parse(entryDwell3.Text);
            BleMvxApplication._config2.RFID_Antenna_Dwell[3] = uint.Parse(entryDwell4.Text);
            BleMvxApplication._config3.RFID_Antenna_Dwell[0] = uint.Parse(entryDwell1.Text);
            BleMvxApplication._config3.RFID_Antenna_Dwell[1] = uint.Parse(entryDwell2.Text);
            BleMvxApplication._config3.RFID_Antenna_Dwell[2] = uint.Parse(entryDwell3.Text);
            BleMvxApplication._config3.RFID_Antenna_Dwell[3] = uint.Parse(entryDwell4.Text);
            BleMvxApplication._config4.RFID_Antenna_Dwell[0] = uint.Parse(entryDwell1.Text);
            BleMvxApplication._config4.RFID_Antenna_Dwell[1] = uint.Parse(entryDwell2.Text);
            BleMvxApplication._config4.RFID_Antenna_Dwell[2] = uint.Parse(entryDwell3.Text);
            BleMvxApplication._config4.RFID_Antenna_Dwell[3] = uint.Parse(entryDwell4.Text);

            BleMvxApplication.SaveConfig(1);
            BleMvxApplication.SaveConfig(2);
            BleMvxApplication.SaveConfig(3);
            BleMvxApplication.SaveConfig(4);

            for (uint cnt=0; cnt<4; cnt++) {
                BleMvxApplication._reader1.rfid.SetAntennaPortState(cnt, BleMvxApplication._config1.RFID_AntennaEnable[cnt] ? CSLibrary.Constants.AntennaPortState.ENABLED : CSLibrary.Constants.AntennaPortState.DISABLED);
                BleMvxApplication._reader1.rfid.SetPowerLevel(BleMvxApplication._config1.RFID_Antenna_Power[cnt], cnt);
                BleMvxApplication._reader1.rfid.SetInventoryDuration(BleMvxApplication._config1.RFID_Antenna_Dwell[cnt], cnt);
                BleMvxApplication._reader2.rfid.SetAntennaPortState(cnt, BleMvxApplication._config2.RFID_AntennaEnable[cnt] ? CSLibrary.Constants.AntennaPortState.ENABLED : CSLibrary.Constants.AntennaPortState.DISABLED);
                BleMvxApplication._reader2.rfid.SetPowerLevel(BleMvxApplication._config2.RFID_Antenna_Power[cnt], cnt);
                BleMvxApplication._reader2.rfid.SetInventoryDuration(BleMvxApplication._config2.RFID_Antenna_Dwell[cnt], cnt);
                BleMvxApplication._reader3.rfid.SetAntennaPortState(cnt, BleMvxApplication._config3.RFID_AntennaEnable[cnt] ? CSLibrary.Constants.AntennaPortState.ENABLED : CSLibrary.Constants.AntennaPortState.DISABLED);
                BleMvxApplication._reader3.rfid.SetPowerLevel(BleMvxApplication._config3.RFID_Antenna_Power[cnt], cnt);
                BleMvxApplication._reader3.rfid.SetInventoryDuration(BleMvxApplication._config3.RFID_Antenna_Dwell[cnt], cnt);
                BleMvxApplication._reader4.rfid.SetAntennaPortState(cnt, BleMvxApplication._config4.RFID_AntennaEnable[cnt] ? CSLibrary.Constants.AntennaPortState.ENABLED : CSLibrary.Constants.AntennaPortState.DISABLED);
                BleMvxApplication._reader4.rfid.SetPowerLevel(BleMvxApplication._config4.RFID_Antenna_Power[cnt], cnt);
                BleMvxApplication._reader4.rfid.SetInventoryDuration(BleMvxApplication._config4.RFID_Antenna_Dwell[cnt], cnt);
            }
        }

    }
}
