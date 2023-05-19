using System;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions.Contracts;
using System.Windows.Input;
using Xamarin.Forms;
using Plugin.BLE.Abstractions;
using Plugin.Settings.Abstractions;
using Plugin.Permissions.Abstractions;


namespace BLE.Client.ViewModels
{
    public class ViewModelMainMenu : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        readonly IPermissions _permissions;
        private IDevice _device;

        public string connectedButton1 { get; set; }
        public string connectedButton2 { get; set; }
        public string connectedButton3 { get; set; }
        public string connectedButton4 { get; set; }
        public string connectedButtonTextColor1 { get; set; } = "Red";
        public string connectedButtonTextColor2 { get; set; } = "Red";
        public string connectedButtonTextColor3 { get; set; } = "Red";
        public string connectedButtonTextColor4 { get; set; } = "Red";
        public string labelVoltage {get; set;}

        public ViewModelMainMenu(IBluetoothLE bluetoothLe, IAdapter adapter, IUserDialogs userDialogs, ISettings settings, IPermissions permissions) : base(adapter) {
            _userDialogs = userDialogs;
            _permissions = permissions;

            Adapter.DeviceConnectionLost += OnDeviceConnectionLost;

			OnSettingButtonCommand = new Command(OnSettingButtonClicked);
            OnConnectButtonCommand1 = new Command(OnConnectButtonClicked1);
            OnConnectButtonCommand2 = new Command(OnConnectButtonClicked2);
            OnConnectButtonCommand3 = new Command(OnConnectButtonClicked3);
            OnConnectButtonCommand4 = new Command(OnConnectButtonClicked4);
            OnRFMicroButtonCommand = new Command(OnRFMicroButtonClicked);    // SKIP SPECIAL FUNCTION SCREEN

            BleMvxApplication._reader1.OnReaderStateChanged += new EventHandler<CSLibrary.Events.OnReaderStateChangedEventArgs>(ReaderStateCChangedEvent1);
            BleMvxApplication._reader2.OnReaderStateChanged += new EventHandler<CSLibrary.Events.OnReaderStateChangedEventArgs>(ReaderStateCChangedEvent2);
            BleMvxApplication._reader3.OnReaderStateChanged += new EventHandler<CSLibrary.Events.OnReaderStateChangedEventArgs>(ReaderStateCChangedEvent3);
            BleMvxApplication._reader4.OnReaderStateChanged += new EventHandler<CSLibrary.Events.OnReaderStateChangedEventArgs>(ReaderStateCChangedEvent4);

            GetLocationPermission();
        }

        ~ViewModelMainMenu()
        {
            BleMvxApplication._reader1.OnReaderStateChanged -= new EventHandler<CSLibrary.Events.OnReaderStateChangedEventArgs>(ReaderStateCChangedEvent1);
            BleMvxApplication._reader2.OnReaderStateChanged -= new EventHandler<CSLibrary.Events.OnReaderStateChangedEventArgs>(ReaderStateCChangedEvent2);
            BleMvxApplication._reader3.OnReaderStateChanged -= new EventHandler<CSLibrary.Events.OnReaderStateChangedEventArgs>(ReaderStateCChangedEvent3);
            BleMvxApplication._reader4.OnReaderStateChanged -= new EventHandler<CSLibrary.Events.OnReaderStateChangedEventArgs>(ReaderStateCChangedEvent4);
        }

        // MUST be geant location permission
        private async void GetLocationPermission() {
            if (await _permissions.CheckPermissionStatusAsync(Permission.Location) != PermissionStatus.Granted) {
                if (Device.RuntimePlatform == Device.Android)
                    await _userDialogs.AlertAsync("This app collects location data in the background. In terms of the features using this location data in the background, this App collects location data when it is reading temperature RFID tag in the “Magnus S3 with GPS for Advantech” page.  The purpose of this is to correlate the RFID tag with the actual GNSS location of the tag.  In other words, this is to track the physical location of the logistics item tagged with the RFID tag.");
                await _permissions.RequestPermissionsAsync(Permission.Location);
            }
        }

        private void CheckConnection (int n)
        {
            // FOR READER #1, default _reader1 in BleMvxApplication
            if (n==1) {
                if (BleMvxApplication._reader1.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT) {
                    connectedButton1 = "Connected to " + BleMvxApplication._reader1.ReaderName + "/Select Another";
                    connectedButtonTextColor1 = "#00C261"; // formerly "Green"
                }
                else {
                    connectedButton1 = "Press to Scan/Connect Reader 1";
                    connectedButtonTextColor1 = "Red";
                }
                RaisePropertyChanged(() => connectedButton1);
                RaisePropertyChanged(() => connectedButtonTextColor1);
            }

            // FOR READER #2, _reader2 in BleMvxApplication
            if (n==2) {
                if (BleMvxApplication._reader2.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT) {
                    connectedButton2 = "Connected to " + BleMvxApplication._reader2.ReaderName + "/Select Another";
                    connectedButtonTextColor2 = "#00C261"; // formerly "Green"
                }
                else {
                    connectedButton2 = "Press to Scan/Connect Reader 2";
                    connectedButtonTextColor2 = "Red";
                }
                RaisePropertyChanged(() => connectedButton2);
                RaisePropertyChanged(() => connectedButtonTextColor2);
            }

            // FOR READER #3, _reader3 in BleMvxApplication
            if (n==3) {
                if (BleMvxApplication._reader3.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT) {
                    connectedButton3 = "Connected to " + BleMvxApplication._reader3.ReaderName + "/Select Another";
                    connectedButtonTextColor3 = "Green";
                }
                else {
                    connectedButton3 = "Press to Scan/Connect Reader 3";
                    connectedButtonTextColor3 = "Red";
                }
                RaisePropertyChanged(() => connectedButton3);
                RaisePropertyChanged(() => connectedButtonTextColor3);
            }

            // FOR READER #4, _reader4 in BleMvxApplication
            if (n==4) {
                if (BleMvxApplication._reader4.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT) {
                    connectedButton4 = "Connected to " + BleMvxApplication._reader4.ReaderName + "/Select Another";
                    connectedButtonTextColor4 = "Green";
                }
                else {
                    connectedButton4 = "Press to Scan/Connect Reader 4";
                    connectedButtonTextColor4 = "Red";
                }
                RaisePropertyChanged(() => connectedButton4);
                RaisePropertyChanged(() => connectedButtonTextColor4);
            }
        }

        public override void Resume() {
            base.Resume();
            BleMvxApplication._inventoryEntryPoint = 0;

            BleMvxApplication._reader1.notification.OnVoltageEvent += new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent1);
            BleMvxApplication._reader1.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent1);
            BleMvxApplication._reader1.rfid.OnStateChanged += new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent1);

            // ADDED FOR READER #2
            BleMvxApplication._reader2.notification.OnVoltageEvent += new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent2);
            BleMvxApplication._reader2.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent2);
            BleMvxApplication._reader2.rfid.OnStateChanged += new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent2);

            // ADDED FOR READER #3
            BleMvxApplication._reader3.notification.OnVoltageEvent += new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent3);
            BleMvxApplication._reader3.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent3);
            BleMvxApplication._reader3.rfid.OnStateChanged += new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent3);

            // ADDED FOR READER #4
            BleMvxApplication._reader4.notification.OnVoltageEvent += new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent4);
            BleMvxApplication._reader4.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent4);
            BleMvxApplication._reader4.rfid.OnStateChanged += new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent4);

            CheckConnection(1);
            CheckConnection(2);
            CheckConnection(3);
            CheckConnection(4);

            if (BleMvxApplication._reader1.rfid.GetModel() != CSLibrary.Constants.Machine.UNKNOWN)
                BleMvxApplication._reader1.rfid.CancelAllSelectCriteria();
            BleMvxApplication._reader1.rfid.Options.TagRanging.focus = false;
            BleMvxApplication._reader1.rfid.Options.TagRanging.fastid = false;
            BleMvxApplication._reader1.rfid.SetToStandbyMode();                   // for power saving

            // ADDED FOR READER #2
            if (BleMvxApplication._reader2.rfid.GetModel() != CSLibrary.Constants.Machine.UNKNOWN)
                BleMvxApplication._reader2.rfid.CancelAllSelectCriteria();
            BleMvxApplication._reader2.rfid.Options.TagRanging.focus = false;
            BleMvxApplication._reader2.rfid.Options.TagRanging.fastid = false;
            BleMvxApplication._reader2.rfid.SetToStandbyMode();

            // ADDED FOR READER #3
            if (BleMvxApplication._reader3.rfid.GetModel() != CSLibrary.Constants.Machine.UNKNOWN)
                BleMvxApplication._reader3.rfid.CancelAllSelectCriteria();
            BleMvxApplication._reader3.rfid.Options.TagRanging.focus = false;
            BleMvxApplication._reader3.rfid.Options.TagRanging.fastid = false;
            BleMvxApplication._reader3.rfid.SetToStandbyMode();

            // ADDED FOR READER #4
            if (BleMvxApplication._reader4.rfid.GetModel() != CSLibrary.Constants.Machine.UNKNOWN)
                BleMvxApplication._reader4.rfid.CancelAllSelectCriteria();
            BleMvxApplication._reader4.rfid.Options.TagRanging.focus = false;
            BleMvxApplication._reader4.rfid.Options.TagRanging.fastid = false;
            BleMvxApplication._reader4.rfid.SetToStandbyMode();
        }

        public override void Suspend() {
            BleMvxApplication._reader1.notification.OnKeyEvent -= new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent1);
            BleMvxApplication._reader1.notification.OnVoltageEvent -= new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent1);

            // ADDED FOR READER #2
            BleMvxApplication._reader2.notification.OnKeyEvent -= new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent2);
            BleMvxApplication._reader2.notification.OnVoltageEvent -= new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent2);

            // ADDED FOR READER #3
            BleMvxApplication._reader3.notification.OnKeyEvent -= new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent3);
            BleMvxApplication._reader3.notification.OnVoltageEvent -= new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent3);

            // ADDED FOR READER #4
            BleMvxApplication._reader4.notification.OnKeyEvent -= new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent4);
            BleMvxApplication._reader4.notification.OnVoltageEvent -= new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent4);
    
            base.Suspend();
        }

        protected override void InitFromBundle(IMvxBundle parameters) {
            base.InitFromBundle(parameters);
            _device = GetDeviceFromBundle(parameters);
            if (_device == null) { Close(this); }
		}


        void StateChangedEvent1(object sender, CSLibrary.Events.OnStateChangedEventArgs e) {
            if (e.state == CSLibrary.Constants.RFState.INITIALIZATION_COMPLETE) {
                BleMvxApplication._batteryLow = false;

                // Set Country and Region information
                if (BleMvxApplication._config1.RFID_Region == CSLibrary.Constants.RegionCode.UNKNOWN || BleMvxApplication._config1.readerModel != BleMvxApplication._reader1.rfid.GetModel()) {
                    BleMvxApplication._config1.readerModel = BleMvxApplication._reader1.rfid.GetModel();
                    BleMvxApplication._config1.RFID_Region = BleMvxApplication._reader1.rfid.SelectedRegionCode;

                    if (BleMvxApplication._reader1.rfid.IsFixedChannel) {
                        BleMvxApplication._config1.RFID_FrequenceSwitch = 1;
                        BleMvxApplication._config1.RFID_FixedChannel = BleMvxApplication._reader1.rfid.SelectedChannel;
                    }
                    else {
                        BleMvxApplication._config1.RFID_FrequenceSwitch = 0; // Hopping
                    }
                }

                // the library auto cancel the task if the setting no change
                switch (BleMvxApplication._config1.RFID_FrequenceSwitch) {
                    case 0:
                        BleMvxApplication._reader1.rfid.SetHoppingChannels(BleMvxApplication._config1.RFID_Region);
                        break;
                    case 1:
                        BleMvxApplication._reader1.rfid.SetFixedChannel(BleMvxApplication._config1.RFID_Region, BleMvxApplication._config1.RFID_FixedChannel);
                        break;
                    case 2:
                        BleMvxApplication._reader1.rfid.SetAgileChannels(BleMvxApplication._config1.RFID_Region);
                        break;
                }

                uint portNum = BleMvxApplication._reader1.rfid.GetAntennaPort();
                for (uint cnt = 0; cnt < portNum; cnt++) {
                    BleMvxApplication._reader1.rfid.SetAntennaPortState(cnt, BleMvxApplication._config1.RFID_AntennaEnable[cnt] ? CSLibrary.Constants.AntennaPortState.ENABLED : CSLibrary.Constants.AntennaPortState.DISABLED);
                    BleMvxApplication._reader1.rfid.SetPowerLevel(BleMvxApplication._config1.RFID_Antenna_Power[cnt], cnt);
                    BleMvxApplication._reader1.rfid.SetInventoryDuration(BleMvxApplication._config1.RFID_Antenna_Dwell[cnt], cnt);
                }

                if ((BleMvxApplication._reader1.bluetoothIC.GetFirmwareVersion() & 0x0F0000) != 0x030000) // ignore CS463
                if (BleMvxApplication._reader1.rfid.GetFirmwareVersion() < 0x0002061D || BleMvxApplication._reader1.siliconlabIC.GetFirmwareVersion() < 0x00010009 || BleMvxApplication._reader1.bluetoothIC.GetFirmwareVersion() < 0x0001000E) {
                    _userDialogs.AlertAsync("Firmware too old" + Environment.NewLine + 
                                            "Please upgrade firmware to at least :" + Environment.NewLine +
                                            "RFID Processor firmware: V2.6.29" + Environment.NewLine +
                                            "SiliconLab Firmware: V1.0.9" + Environment.NewLine +
                                            "Bluetooth Firmware: V1.0.14");
                }

                ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
                BleMvxApplication._reader1.battery.SetPollingTime(BleMvxApplication._config1.RFID_BatteryPollingTime);
                BleMvxApplication._reader1.rfid.SetToStandbyMode(); // for power saving
            }
        }

        void StateChangedEvent2(object sender, CSLibrary.Events.OnStateChangedEventArgs e) {
            if (e.state == CSLibrary.Constants.RFState.INITIALIZATION_COMPLETE) {
                BleMvxApplication._batteryLow = false;

                // Set Country and Region information
                if (BleMvxApplication._config2.RFID_Region == CSLibrary.Constants.RegionCode.UNKNOWN || BleMvxApplication._config2.readerModel != BleMvxApplication._reader2.rfid.GetModel()) {
                    BleMvxApplication._config2.readerModel = BleMvxApplication._reader2.rfid.GetModel();
                    BleMvxApplication._config2.RFID_Region = BleMvxApplication._reader2.rfid.SelectedRegionCode;

                    if (BleMvxApplication._reader2.rfid.IsFixedChannel) {
                        BleMvxApplication._config2.RFID_FrequenceSwitch = 1;
                        BleMvxApplication._config2.RFID_FixedChannel = BleMvxApplication._reader2.rfid.SelectedChannel;
                    }
                    else {
                        BleMvxApplication._config2.RFID_FrequenceSwitch = 0; // Hopping
                    }
                }

                // the library auto cancel the task if the setting no change
                switch (BleMvxApplication._config2.RFID_FrequenceSwitch) {
                    case 0:
                        BleMvxApplication._reader2.rfid.SetHoppingChannels(BleMvxApplication._config2.RFID_Region);
                        break;
                    case 1:
                        BleMvxApplication._reader2.rfid.SetFixedChannel(BleMvxApplication._config2.RFID_Region, BleMvxApplication._config2.RFID_FixedChannel);
                        break;
                    case 2:
                        BleMvxApplication._reader2.rfid.SetAgileChannels(BleMvxApplication._config2.RFID_Region);
                        break;
                }

                uint portNum = BleMvxApplication._reader2.rfid.GetAntennaPort();
                for (uint cnt = 0; cnt < portNum; cnt++) {
                    BleMvxApplication._reader2.rfid.SetAntennaPortState(cnt, BleMvxApplication._config2.RFID_AntennaEnable[cnt] ? CSLibrary.Constants.AntennaPortState.ENABLED : CSLibrary.Constants.AntennaPortState.DISABLED);
                    BleMvxApplication._reader2.rfid.SetPowerLevel(BleMvxApplication._config2.RFID_Antenna_Power[cnt], cnt);
                    BleMvxApplication._reader2.rfid.SetInventoryDuration(BleMvxApplication._config2.RFID_Antenna_Dwell[cnt], cnt);
                }

                if ((BleMvxApplication._reader2.bluetoothIC.GetFirmwareVersion() & 0x0F0000) != 0x030000) // ignore CS463
                if (BleMvxApplication._reader2.rfid.GetFirmwareVersion() < 0x0002061D || BleMvxApplication._reader2.siliconlabIC.GetFirmwareVersion() < 0x00010009 || BleMvxApplication._reader2.bluetoothIC.GetFirmwareVersion() < 0x0001000E) {
                    _userDialogs.AlertAsync("Firmware too old" + Environment.NewLine + 
                                            "Please upgrade firmware to at least :" + Environment.NewLine +
                                            "RFID Processor firmware: V2.6.29" + Environment.NewLine +
                                            "SiliconLab Firmware: V1.0.9" + Environment.NewLine +
                                            "Bluetooth Firmware: V1.0.14");
                }

                ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
                BleMvxApplication._reader2.battery.SetPollingTime(BleMvxApplication._config2.RFID_BatteryPollingTime);
                BleMvxApplication._reader2.rfid.SetToStandbyMode(); // for power saving
            }
        }

        void StateChangedEvent3(object sender, CSLibrary.Events.OnStateChangedEventArgs e) {
            if (e.state == CSLibrary.Constants.RFState.INITIALIZATION_COMPLETE) {
                BleMvxApplication._batteryLow = false;

                // Set Country and Region information
                if (BleMvxApplication._config3.RFID_Region == CSLibrary.Constants.RegionCode.UNKNOWN || BleMvxApplication._config3.readerModel != BleMvxApplication._reader3.rfid.GetModel()) {
                    BleMvxApplication._config3.readerModel = BleMvxApplication._reader3.rfid.GetModel();
                    BleMvxApplication._config3.RFID_Region = BleMvxApplication._reader3.rfid.SelectedRegionCode;

                    if (BleMvxApplication._reader3.rfid.IsFixedChannel) {
                        BleMvxApplication._config3.RFID_FrequenceSwitch = 1;
                        BleMvxApplication._config3.RFID_FixedChannel = BleMvxApplication._reader3.rfid.SelectedChannel;
                    }
                    else {
                        BleMvxApplication._config3.RFID_FrequenceSwitch = 0; // Hopping
                    }
                }

                // the library auto cancel the task if the setting no change
                switch (BleMvxApplication._config3.RFID_FrequenceSwitch) {
                    case 0:
                        BleMvxApplication._reader3.rfid.SetHoppingChannels(BleMvxApplication._config3.RFID_Region);
                        break;
                    case 1:
                        BleMvxApplication._reader3.rfid.SetFixedChannel(BleMvxApplication._config3.RFID_Region, BleMvxApplication._config3.RFID_FixedChannel);
                        break;
                    case 2:
                        BleMvxApplication._reader3.rfid.SetAgileChannels(BleMvxApplication._config3.RFID_Region);
                        break;
                }

                uint portNum = BleMvxApplication._reader3.rfid.GetAntennaPort();
                for (uint cnt = 0; cnt < portNum; cnt++) {
                    BleMvxApplication._reader3.rfid.SetAntennaPortState(cnt, BleMvxApplication._config3.RFID_AntennaEnable[cnt] ? CSLibrary.Constants.AntennaPortState.ENABLED : CSLibrary.Constants.AntennaPortState.DISABLED);
                    BleMvxApplication._reader3.rfid.SetPowerLevel(BleMvxApplication._config3.RFID_Antenna_Power[cnt], cnt);
                    BleMvxApplication._reader3.rfid.SetInventoryDuration(BleMvxApplication._config3.RFID_Antenna_Dwell[cnt], cnt);
                }

                if ((BleMvxApplication._reader3.bluetoothIC.GetFirmwareVersion() & 0x0F0000) != 0x030000) // ignore CS463
                if (BleMvxApplication._reader3.rfid.GetFirmwareVersion() < 0x0002061D || BleMvxApplication._reader3.siliconlabIC.GetFirmwareVersion() < 0x00010009 || BleMvxApplication._reader3.bluetoothIC.GetFirmwareVersion() < 0x0001000E) {
                    _userDialogs.AlertAsync("Firmware too old" + Environment.NewLine + 
                                            "Please upgrade firmware to at least :" + Environment.NewLine +
                                            "RFID Processor firmware: V2.6.29" + Environment.NewLine +
                                            "SiliconLab Firmware: V1.0.9" + Environment.NewLine +
                                            "Bluetooth Firmware: V1.0.14");
                }

                ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
                BleMvxApplication._reader3.battery.SetPollingTime(BleMvxApplication._config3.RFID_BatteryPollingTime);
                BleMvxApplication._reader3.rfid.SetToStandbyMode();
            }
        }

        void StateChangedEvent4(object sender, CSLibrary.Events.OnStateChangedEventArgs e) {
            if (e.state == CSLibrary.Constants.RFState.INITIALIZATION_COMPLETE) {
                BleMvxApplication._batteryLow = false;

                // Set Country and Region information
                if (BleMvxApplication._config4.RFID_Region == CSLibrary.Constants.RegionCode.UNKNOWN || BleMvxApplication._config4.readerModel != BleMvxApplication._reader4.rfid.GetModel()) {
                    BleMvxApplication._config4.readerModel = BleMvxApplication._reader4.rfid.GetModel();
                    BleMvxApplication._config4.RFID_Region = BleMvxApplication._reader4.rfid.SelectedRegionCode;

                    if (BleMvxApplication._reader4.rfid.IsFixedChannel) {
                        BleMvxApplication._config4.RFID_FrequenceSwitch = 1;
                        BleMvxApplication._config4.RFID_FixedChannel = BleMvxApplication._reader4.rfid.SelectedChannel;
                    }
                    else {
                        BleMvxApplication._config4.RFID_FrequenceSwitch = 0; // Hopping
                    }
                }

                // the library auto cancel the task if the setting no change
                switch (BleMvxApplication._config4.RFID_FrequenceSwitch) {
                    case 0:
                        BleMvxApplication._reader4.rfid.SetHoppingChannels(BleMvxApplication._config4.RFID_Region);
                        break;
                    case 1:
                        BleMvxApplication._reader4.rfid.SetFixedChannel(BleMvxApplication._config4.RFID_Region, BleMvxApplication._config4.RFID_FixedChannel);
                        break;
                    case 2:
                        BleMvxApplication._reader4.rfid.SetAgileChannels(BleMvxApplication._config4.RFID_Region);
                        break;
                }

                uint portNum = BleMvxApplication._reader4.rfid.GetAntennaPort();
                for (uint cnt = 0; cnt < portNum; cnt++) {
                    BleMvxApplication._reader4.rfid.SetAntennaPortState(cnt, BleMvxApplication._config4.RFID_AntennaEnable[cnt] ? CSLibrary.Constants.AntennaPortState.ENABLED : CSLibrary.Constants.AntennaPortState.DISABLED);
                    BleMvxApplication._reader4.rfid.SetPowerLevel(BleMvxApplication._config4.RFID_Antenna_Power[cnt], cnt);
                    BleMvxApplication._reader4.rfid.SetInventoryDuration(BleMvxApplication._config4.RFID_Antenna_Dwell[cnt], cnt);
                }

                if ((BleMvxApplication._reader4.bluetoothIC.GetFirmwareVersion() & 0x0F0000) != 0x030000) // ignore CS463
                if (BleMvxApplication._reader4.rfid.GetFirmwareVersion() < 0x0002061D || BleMvxApplication._reader4.siliconlabIC.GetFirmwareVersion() < 0x00010009 || BleMvxApplication._reader4.bluetoothIC.GetFirmwareVersion() < 0x0001000E) {
                    _userDialogs.AlertAsync("Firmware too old" + Environment.NewLine + 
                                            "Please upgrade firmware to at least :" + Environment.NewLine +
                                            "RFID Processor firmware: V2.6.29" + Environment.NewLine +
                                            "SiliconLab Firmware: V1.0.9" + Environment.NewLine +
                                            "Bluetooth Firmware: V1.0.14");
                }

                ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
                BleMvxApplication._reader4.battery.SetPollingTime(BleMvxApplication._config4.RFID_BatteryPollingTime);
                BleMvxApplication._reader4.rfid.SetToStandbyMode();
            }
        }

        void ReaderStateCChangedEvent1(object sender, CSLibrary.Events.OnReaderStateChangedEventArgs e) {
            InvokeOnMainThread(() => {
                Trace.Message(e.type.ToString());
                switch (e.type) {
                    case CSLibrary.Constants.ReaderCallbackType.COMMUNICATION_ERROR:
                        {
                            _userDialogs.AlertAsync("BLE protocol error, Please reset reader #1");
                        }
                        break;

                    case CSLibrary.Constants.ReaderCallbackType.CONNECTION_LOST: break;
                    default: break;
                }
                CheckConnection(1);
            });
        }

        void ReaderStateCChangedEvent2(object sender, CSLibrary.Events.OnReaderStateChangedEventArgs e) {
            InvokeOnMainThread(() => {
                Trace.Message(e.type.ToString());
                switch (e.type) {
                    case CSLibrary.Constants.ReaderCallbackType.COMMUNICATION_ERROR:
                        {
                            _userDialogs.AlertAsync("BLE protocol error, Please reset reader #2");
                        }
                        break;

                    case CSLibrary.Constants.ReaderCallbackType.CONNECTION_LOST: break;
                    default: break;
                }
                CheckConnection(2);
            });
        }

        void ReaderStateCChangedEvent3(object sender, CSLibrary.Events.OnReaderStateChangedEventArgs e) {
            InvokeOnMainThread(() => {
                Trace.Message(e.type.ToString());
                switch (e.type) {
                    case CSLibrary.Constants.ReaderCallbackType.COMMUNICATION_ERROR:
                        {
                            _userDialogs.AlertAsync("BLE protocol error, Please reset reader #3");
                        }
                        break;
                    case CSLibrary.Constants.ReaderCallbackType.CONNECTION_LOST: break;
                    default: break;
                }
                CheckConnection(3);
            });
        }

        void ReaderStateCChangedEvent4(object sender, CSLibrary.Events.OnReaderStateChangedEventArgs e) {
            InvokeOnMainThread(() => {
                Trace.Message(e.type.ToString());
                switch (e.type) {
                    case CSLibrary.Constants.ReaderCallbackType.COMMUNICATION_ERROR:
                        {
                            _userDialogs.AlertAsync("BLE protocol error, Please reset reader #4");
                        }
                        break;
                    case CSLibrary.Constants.ReaderCallbackType.CONNECTION_LOST: break;
                    default: break;
                }
                CheckConnection(4);
            });
        }

        DateTime _keyPressStartTime;

        void HotKeys_OnKeyEvent1(object sender, CSLibrary.Notification.HotKeyEventArgs e) {
            if (e.KeyCode == CSLibrary.Notification.Key.BUTTON) {
                if (e.KeyDown) { _keyPressStartTime = DateTime.Now; }
                else {
                    double duration = (DateTime.Now - _keyPressStartTime).TotalMilliseconds;

                    for (int cnt = 0; cnt < BleMvxApplication._config1.RFID_Shortcut.Length; cnt++) {
                        if (duration >= BleMvxApplication._config1.RFID_Shortcut[cnt].DurationMin && duration <= BleMvxApplication._config1.RFID_Shortcut[cnt].DurationMax) {
                            switch (BleMvxApplication._config1.RFID_Shortcut[cnt].Function) {
                                case CONFIG.MAINMENUSHORTCUT.FUNCTION.INVENTORY:
                                    BleMvxApplication._inventoryEntryPoint = 0;
                                    OnInventoryButtonClicked();
                                    break;
                                case CONFIG.MAINMENUSHORTCUT.FUNCTION.BARCODE:
                                    BleMvxApplication._inventoryEntryPoint = 1;
                                    OnInventoryButtonClicked();
                                    break;
                            }
                            break;
                        }
                    }
                }
            }
        }

        void HotKeys_OnKeyEvent2(object sender, CSLibrary.Notification.HotKeyEventArgs e) {
            if (e.KeyCode == CSLibrary.Notification.Key.BUTTON) {
                if (e.KeyDown) {
                    _keyPressStartTime = DateTime.Now;
                }
                else {
                    double duration = (DateTime.Now - _keyPressStartTime).TotalMilliseconds;

                    for (int cnt = 0; cnt < BleMvxApplication._config2.RFID_Shortcut.Length; cnt++) {
                        if (duration >= BleMvxApplication._config2.RFID_Shortcut[cnt].DurationMin && duration <= BleMvxApplication._config2.RFID_Shortcut[cnt].DurationMax) {
                            switch (BleMvxApplication._config2.RFID_Shortcut[cnt].Function) {
                                case CONFIG.MAINMENUSHORTCUT.FUNCTION.INVENTORY:
                                    BleMvxApplication._inventoryEntryPoint = 0;
                                    OnInventoryButtonClicked();
                                    break;
                                case CONFIG.MAINMENUSHORTCUT.FUNCTION.BARCODE:
                                    BleMvxApplication._inventoryEntryPoint = 1;
                                    OnInventoryButtonClicked();
                                    break;
                            }
                            break;
                        }
                    }
                }
            }
        }

        void HotKeys_OnKeyEvent3(object sender, CSLibrary.Notification.HotKeyEventArgs e) {
            if (e.KeyCode == CSLibrary.Notification.Key.BUTTON) {
                if (e.KeyDown) { _keyPressStartTime = DateTime.Now; }
                else {
                    double duration = (DateTime.Now - _keyPressStartTime).TotalMilliseconds;

                    for (int cnt = 0; cnt < BleMvxApplication._config3.RFID_Shortcut.Length; cnt++) {
                        if (duration >= BleMvxApplication._config3.RFID_Shortcut[cnt].DurationMin && duration <= BleMvxApplication._config3.RFID_Shortcut[cnt].DurationMax) {
                            switch (BleMvxApplication._config3.RFID_Shortcut[cnt].Function) {
                                case CONFIG.MAINMENUSHORTCUT.FUNCTION.INVENTORY:
                                    BleMvxApplication._inventoryEntryPoint = 0;
                                    OnInventoryButtonClicked();
                                    break;
                                case CONFIG.MAINMENUSHORTCUT.FUNCTION.BARCODE:
                                    BleMvxApplication._inventoryEntryPoint = 1;
                                    OnInventoryButtonClicked();
                                    break;
                            }
                            break;
                        }
                    }
                }
            }
        }

        void HotKeys_OnKeyEvent4(object sender, CSLibrary.Notification.HotKeyEventArgs e) {
            if (e.KeyCode == CSLibrary.Notification.Key.BUTTON) {
                if (e.KeyDown) { _keyPressStartTime = DateTime.Now; }
                else {
                    double duration = (DateTime.Now - _keyPressStartTime).TotalMilliseconds;

                    for (int cnt = 0; cnt < BleMvxApplication._config4.RFID_Shortcut.Length; cnt++) {
                        if (duration >= BleMvxApplication._config4.RFID_Shortcut[cnt].DurationMin && duration <= BleMvxApplication._config4.RFID_Shortcut[cnt].DurationMax) {
                            switch (BleMvxApplication._config4.RFID_Shortcut[cnt].Function) {
                                case CONFIG.MAINMENUSHORTCUT.FUNCTION.INVENTORY:
                                    BleMvxApplication._inventoryEntryPoint = 0;
                                    OnInventoryButtonClicked();
                                    break;
                                case CONFIG.MAINMENUSHORTCUT.FUNCTION.BARCODE:
                                    BleMvxApplication._inventoryEntryPoint = 1;
                                    OnInventoryButtonClicked();
                                    break;
                            }
                            break;
                        }
                    }
                }
            }
        }

        bool _firstTimeBatteryLowAlert = true;

        void VoltageEvent1(object sender, CSLibrary.Notification.VoltageEventArgs e) {
			if (e.Voltage == 0xffff) {
				labelVoltage = "CS108 Bat. ERROR"; //			3.98v
			}
			else {
                double voltage = (double)e.Voltage / 1000;
                {
                    var batlow = ClassBattery.BatteryLow(voltage);

                    if (BleMvxApplication._batteryLow && batlow == ClassBattery.BATTERYLEVELSTATUS.NORMAL) {
                        BleMvxApplication._batteryLow = false;
                    }
                    else
                    if (!BleMvxApplication._batteryLow && batlow != ClassBattery.BATTERYLEVELSTATUS.NORMAL) {
                        BleMvxApplication._batteryLow = true;
                        if (batlow == ClassBattery.BATTERYLEVELSTATUS.LOW) _userDialogs.AlertAsync("20% Battery Life Left, Please Recharge CS108 or Replace Freshly Charged CS108B");
                    }
                }

                switch (BleMvxApplication._config1.BatteryLevelIndicatorFormat) {
                    case 0:
                        labelVoltage = "CS108 Bat. " + voltage.ToString("0.000") + "v"; //			v
                        break;
                    default:
                        labelVoltage = "CS108 Bat. " + ClassBattery.Voltage2Percent(voltage).ToString("0") + "%" + " " + voltage.ToString("0.000") + "v"; //			%
                        break;
                }
            }
            RaisePropertyChanged(() => labelVoltage);
		}

        void VoltageEvent2(object sender, CSLibrary.Notification.VoltageEventArgs e) {
			if (e.Voltage == 0xffff) {
				labelVoltage = "CS108 Bat. ERROR"; //			3.98v
			}
			else {
                double voltage = (double)e.Voltage / 1000;
                {
                    var batlow = ClassBattery.BatteryLow(voltage);

                    if (BleMvxApplication._batteryLow && batlow == ClassBattery.BATTERYLEVELSTATUS.NORMAL) {
                        BleMvxApplication._batteryLow = false;
                    }
                    else
                    if (!BleMvxApplication._batteryLow && batlow != ClassBattery.BATTERYLEVELSTATUS.NORMAL) {
                        BleMvxApplication._batteryLow = true;
                        if (batlow == ClassBattery.BATTERYLEVELSTATUS.LOW) _userDialogs.AlertAsync("20% Battery Life Left, Please Recharge CS108 or Replace Freshly Charged CS108B");
                    }
                }

                switch (BleMvxApplication._config2.BatteryLevelIndicatorFormat) {
                    case 0:
                        labelVoltage = "CS108 Bat. " + voltage.ToString("0.000") + "v"; //			v
                        break;

                    default:
                        labelVoltage = "CS108 Bat. " + ClassBattery.Voltage2Percent(voltage).ToString("0") + "%" + " " + voltage.ToString("0.000") + "v"; //			%
                        break;
                }
            }
            RaisePropertyChanged(() => labelVoltage);
		}

        void VoltageEvent3(object sender, CSLibrary.Notification.VoltageEventArgs e) {
			if (e.Voltage == 0xffff) {
				labelVoltage = "CS108 Bat. ERROR";
			}
			else {
                double voltage = (double)e.Voltage / 1000;
                {
                    var batlow = ClassBattery.BatteryLow(voltage);

                    if (BleMvxApplication._batteryLow && batlow == ClassBattery.BATTERYLEVELSTATUS.NORMAL) {
                        BleMvxApplication._batteryLow = false;
                    }
                    else
                    if (!BleMvxApplication._batteryLow && batlow != ClassBattery.BATTERYLEVELSTATUS.NORMAL) {
                        BleMvxApplication._batteryLow = true;
                        if (batlow == ClassBattery.BATTERYLEVELSTATUS.LOW) _userDialogs.AlertAsync("20% Battery Life Left, Please Recharge CS108 or Replace Freshly Charged CS108B");
                    }
                }

                switch (BleMvxApplication._config3.BatteryLevelIndicatorFormat) {
                    case 0:
                        labelVoltage = "CS108 Bat. " + voltage.ToString("0.000") + "v"; //			v
                        break;

                    default:
                        labelVoltage = "CS108 Bat. " + ClassBattery.Voltage2Percent(voltage).ToString("0") + "%" + " " + voltage.ToString("0.000") + "v";
                        break;
                }
            }
            RaisePropertyChanged(() => labelVoltage);
		}

        void VoltageEvent4(object sender, CSLibrary.Notification.VoltageEventArgs e) {
			if (e.Voltage == 0xffff) { labelVoltage = "CS108 Bat. ERROR"; }
			else {
                double voltage = (double)e.Voltage / 1000;
                {
                    var batlow = ClassBattery.BatteryLow(voltage);

                    if (BleMvxApplication._batteryLow && batlow == ClassBattery.BATTERYLEVELSTATUS.NORMAL) {
                        BleMvxApplication._batteryLow = false;
                    }
                    else
                    if (!BleMvxApplication._batteryLow && batlow != ClassBattery.BATTERYLEVELSTATUS.NORMAL) {
                        BleMvxApplication._batteryLow = true;
                        if (batlow == ClassBattery.BATTERYLEVELSTATUS.LOW) _userDialogs.AlertAsync("20% Battery Life Left, Please Recharge CS108 or Replace Freshly Charged CS108B");
                    }
                }

                switch (BleMvxApplication._config4.BatteryLevelIndicatorFormat) {
                    case 0:
                        labelVoltage = "CS108 Bat. " + voltage.ToString("0.000") + "v"; //			v
                        break;

                    default:
                        labelVoltage = "CS108 Bat. " + ClassBattery.Voltage2Percent(voltage).ToString("0") + "%" + " " + voltage.ToString("0.000") + "v";
                        break;
                }
            }
            RaisePropertyChanged(() => labelVoltage);
		}

		// public ICommand OnInventoryButtonCommand { protected set; get; }

        void OnInventoryButtonClicked() {
            if (BleMvxApplication._reader1.BLEBusy) {
                _userDialogs.ShowSuccess("Configuring Reader, Please Wait", 1000);
            }
            else {
                if (BleMvxApplication._reader1.Status == CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT) {
                    ShowConnectionWarringMessage();
                    return;
                }
            }
        }

        public ICommand OnRFMicroButtonCommand {protected set; get;}

        void OnRFMicroButtonClicked() {
            if (BleMvxApplication._reader1.Status == CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT) {
                ShowConnectionWarringMessage();
                return;
            }
            ShowViewModel<ViewModelRFMicroSetting>(new MvxBundle());
        }

		public ICommand OnSettingButtonCommand {protected set; get;}

        void OnSettingButtonClicked() {
            if (BleMvxApplication._reader1.BLEBusy) {
                _userDialogs.ShowSuccess("Configuring Reader, Please Wait", 1000);
            }
            else {
                if (BleMvxApplication._reader1.Status == CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT) {
                    ShowConnectionWarringMessage();
                    return;
                }
                ShowViewModel<ViewModelSetting>(new MvxBundle());
            }
        }

        public ICommand OnConnectButtonCommand1 { protected set; get; }
        public ICommand OnConnectButtonCommand2 { protected set; get; }
        public ICommand OnConnectButtonCommand3 { protected set; get; }
        public ICommand OnConnectButtonCommand4 { protected set; get; }

        void OnConnectButtonClicked1() {
            if (BleMvxApplication._reader1.BLEBusy) {
                _userDialogs.ShowSuccess("Configuring Reader, Please Wait", 1000);
                return;
            }

            // for Geiger and Read/Write
            BleMvxApplication._SELECT_EPC = "";
            BleMvxApplication._SELECT_PC = 3000;

            // for PreFilter
            BleMvxApplication._PREFILTER_MASK_EPC = "";
            BleMvxApplication._PREFILTER_MASK_Offset = 0;
            BleMvxApplication._PREFILTER_MASK_Truncate = 0;
            BleMvxApplication._PREFILTER_Enable = false;

            // for Post Filter
            BleMvxApplication._POSTFILTER_MASK_EPC = "";
            BleMvxApplication._POSTFILTER_MASK_Offset = 0;
            BleMvxApplication._POSTFILTER_MASK_MatchNot = false;
            BleMvxApplication._POSTFILTER_MASK_Enable = false;

            ShowViewModel<DeviceList1ViewModel>(new MvxBundle());
            CheckConnection(1);
        }

        void OnConnectButtonClicked2() {
            if (BleMvxApplication._reader2.BLEBusy) {
                _userDialogs.ShowSuccess("Configuring Reader, Please Wait", 1000);
                return;
            }

            // for Geiger and Read/Write
            BleMvxApplication._SELECT_EPC = "";
            BleMvxApplication._SELECT_PC = 3000;

            // for PreFilter
            BleMvxApplication._PREFILTER_MASK_EPC = "";
            BleMvxApplication._PREFILTER_MASK_Offset = 0;
            BleMvxApplication._PREFILTER_MASK_Truncate = 0;
            BleMvxApplication._PREFILTER_Enable = false;

            // for Post Filter
            BleMvxApplication._POSTFILTER_MASK_EPC = "";
            BleMvxApplication._POSTFILTER_MASK_Offset = 0;
            BleMvxApplication._POSTFILTER_MASK_MatchNot = false;
            BleMvxApplication._POSTFILTER_MASK_Enable = false;

            ShowViewModel<DeviceList2ViewModel>(new MvxBundle());
            CheckConnection(2);
        }

        void OnConnectButtonClicked3() {
            if (BleMvxApplication._reader3.BLEBusy) {
                _userDialogs.ShowSuccess("Configuring Reader, Please Wait", 1000);
                return;
            }

            // for Geiger and Read/Write
            BleMvxApplication._SELECT_EPC = "";
            BleMvxApplication._SELECT_PC = 3000;

            // for PreFilter
            BleMvxApplication._PREFILTER_MASK_EPC = "";
            BleMvxApplication._PREFILTER_MASK_Offset = 0;
            BleMvxApplication._PREFILTER_MASK_Truncate = 0;
            BleMvxApplication._PREFILTER_Enable = false;

            // for Post Filter
            BleMvxApplication._POSTFILTER_MASK_EPC = "";
            BleMvxApplication._POSTFILTER_MASK_Offset = 0;
            BleMvxApplication._POSTFILTER_MASK_MatchNot = false;
            BleMvxApplication._POSTFILTER_MASK_Enable = false;

            ShowViewModel<DeviceList3ViewModel>(new MvxBundle());
            CheckConnection(3);
        }

        void OnConnectButtonClicked4() {
            if (BleMvxApplication._reader4.BLEBusy) {
                _userDialogs.ShowSuccess("Configuring Reader, Please Wait", 1000);
                return;
            }

            // for Geiger and Read/Write
            BleMvxApplication._SELECT_EPC = "";
            BleMvxApplication._SELECT_PC = 3000;

            // for PreFilter
            BleMvxApplication._PREFILTER_MASK_EPC = "";
            BleMvxApplication._PREFILTER_MASK_Offset = 0;
            BleMvxApplication._PREFILTER_MASK_Truncate = 0;
            BleMvxApplication._PREFILTER_Enable = false;

            // for Post Filter
            BleMvxApplication._POSTFILTER_MASK_EPC = "";
            BleMvxApplication._POSTFILTER_MASK_Offset = 0;
            BleMvxApplication._POSTFILTER_MASK_MatchNot = false;
            BleMvxApplication._POSTFILTER_MASK_Enable = false;

            ShowViewModel<DeviceList4ViewModel>(new MvxBundle());
            CheckConnection(4);
        }

        async void ShowConnectionWarringMessage ()
        {
            string connectWarringMsg = "Reader NOT connected\n\nPlease connect to reader first!";
            _userDialogs.ShowSuccess (connectWarringMsg, 2500);
        }

        private void OnDeviceConnectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e) {
            CheckConnection(1);
            CheckConnection(2);
            CheckConnection(3);
            CheckConnection(4);
        }

    }
}
