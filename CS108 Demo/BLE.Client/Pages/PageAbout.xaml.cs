﻿using System;
using Xamarin.Forms;
using Xamarin.Essentials;


namespace BLE.Client.Pages
{
    public partial class PageAbout
    {
        public PageAbout() {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS) {
                this.Icon = new FileImageSource();
                this.Icon.File = "icons8-Settings-50-4-30x30.png";
            }

            labelAppVer.Text          = "Application Version " + DependencyService.Get<IAppVersion>().GetVersion() + "-" + DependencyService.Get<IAppVersion>().GetBuild().ToString();
            labelLibVer.Text          = "Library Version " + BleMvxApplication._reader1.GetVersion().ToString();
            labelBtFwVer.Text         = "Bluetooth Firmware Version " + Version2String(BleMvxApplication._reader1.bluetoothIC.GetFirmwareVersion());
            labelRFIDFwVer.Text       = "RFID Firmware Version " + Version2String(BleMvxApplication._reader1.rfid.GetFirmwareVersion());
            labelSiliconlabFwVer.Text = "SiliconLab IC Firmware Version " + Version2String(BleMvxApplication._reader1.siliconlabIC.GetFirmwareVersion());
            labelPcbVer.Text          = "Main Board PCB Version " + GetPCBVersion ();
            labelSerialNumber.Text    = "CS108 Serial Number " + BleMvxApplication._reader1.siliconlabIC.GetSerialNumberSync();
        }

        string Version2String(uint ver) {
            return string.Format("{0}.{1}.{2}", (ver >> 16) & 0xff, (ver >> 8) & 0xff, ver & 0xff);
        }

        string GetPCBVersion () {
            try {
                var ver = BleMvxApplication._reader1.siliconlabIC.GetPCBVersion();

                if (ver.Substring(2, 1) != "0") return ver.Substring(0, 1) + "." + ver.Substring(1, 2);
                else                            return ver.Substring(0, 1) + "." + ver.Substring(1, 1);
            }
            catch(Exception ex) {
                return "No PCB Version";
            }
        }

        public async void buttonOpenPrivacypolicyClicked(object sender, EventArgs args) {
            await Launcher.OpenAsync(new Uri("https://www.convergence.com.hk/apps-privacy-policy/"));
        }

    }
}
