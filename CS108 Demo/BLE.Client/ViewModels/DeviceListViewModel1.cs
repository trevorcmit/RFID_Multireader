using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using BLE.Client.Extensions;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using Plugin.Permissions.Abstractions;
using Plugin.Settings.Abstractions;


namespace BLE.Client.ViewModels {
    public class DeviceListViewModel : BaseViewModel {
        private readonly IBluetoothLE _bluetoothLe;
        private readonly IUserDialogs _userDialogs;
        private readonly ISettings _settings;
        private Guid _previousGuid;
        private CancellationTokenSource _cancellationTokenSource;

        public IList<IService> Services {get; private set;}
        public IDescriptor Descriptor {get; private set;}

        private string _version;
        public string version {get; set;}

        public Guid PreviousGuid {
            get {return _previousGuid;}
            set {
                _previousGuid = value;
                _settings.AddOrUpdateValue("lastguid", _previousGuid.ToString());
                RaisePropertyChanged();
                RaisePropertyChanged(() => ConnectToPreviousCommand);
            }
        }

        public MvxCommand RefreshCommand => new MvxCommand(() => TryStartScanning(true));
        public MvxCommand<DeviceListItemViewModel> DisconnectCommand => new MvxCommand<DeviceListItemViewModel>(DisconnectDevice);

        public MvxCommand<DeviceListItemViewModel> ConnectDisposeCommand => new MvxCommand<DeviceListItemViewModel>(ConnectAndDisposeDevice);

        public ObservableCollection<DeviceListItemViewModel> Devices {get; set;} = new ObservableCollection<DeviceListItemViewModel>();
        public bool IsRefreshing => Adapter.IsScanning;
        public bool IsStateOn => _bluetoothLe.IsOn;
        public string StateText => GetStateText();
        public DeviceListItemViewModel SelectedDevice {
            get {return null;}
            set {
                if (value != null) {
                    HandleSelectedDevice(value);
                }
                RaisePropertyChanged();
            }
        }

        public MvxCommand StopScanCommand => new MvxCommand(() => {
            try {
                Devices.Clear();
                _cancellationTokenSource.Cancel();
                CleanupCancellationToken();
                RaisePropertyChanged(() => IsRefreshing);
            }
            catch (Exception ex) {
                CSLibrary.Debug.WriteLine("can not stop _cancellationTokenSource");
            }
        }, () => _cancellationTokenSource != null);

        public DeviceListViewModel(IBluetoothLE bluetoothLe, IAdapter adapter, IUserDialogs userDialogs, ISettings settings, IPermissions permissions) : base(adapter) {
            _bluetoothLe = bluetoothLe;
            _userDialogs = userDialogs;
            _settings = settings;

            _bluetoothLe.StateChanged += OnStateChanged;
            Adapter.DeviceDiscovered += OnDeviceDiscovered;
            Adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            Adapter.DeviceDisconnected += OnDeviceDisconnected;
            Adapter.DeviceConnectionLost += OnDeviceConnectionLost;

            BleMvxApplication._reader.DisconnectAsync();
        }

        private Task GetPreviousGuidAsync() {
            return Task.Run(() => {
                var guidString = _settings.GetValueOrDefault("lastguid", null);
                PreviousGuid = !string.IsNullOrEmpty(guidString) ? Guid.Parse(guidString) : Guid.Empty;
            });
        }

        private void OnDeviceConnectionLost(object sender, DeviceErrorEventArgs e) {
            Devices.FirstOrDefault(d => d.Id == e.Device.Id)?.Update();
            _userDialogs.HideLoading();
            _userDialogs.ErrorToast("Error", $"Connection LOST {e.Device.Name} Please reconnect reader", TimeSpan.FromMilliseconds(5000));
        }

        private void OnStateChanged(object sender, BluetoothStateChangedArgs e) {
            RaisePropertyChanged(nameof(IsStateOn));
            RaisePropertyChanged(nameof(StateText));
        }

        private string GetStateText() {
            try {
                switch (_bluetoothLe.State) {
                    case BluetoothState.Unavailable:
                        return "BLE is not available on this device.";
                    case BluetoothState.Unauthorized:
                        return "You are not allowed to use BLE.";
                    case BluetoothState.TurningOn:
                        return "BLE is warming up, please wait.";
                    case BluetoothState.On:
                        return "BLE is on.";
                    case BluetoothState.TurningOff:
                        return "BLE is turning off. That's sad!";
                    case BluetoothState.Off:
                        if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
                            _userDialogs.Alert("Please put finger at bottom of screen and swipe up “Control Center” and turn on Bluetooth.  If Bluetooth is already on, turn it off and on again");
                        return "BLE is off. Turn it on!";
                }
            }
            catch (Exception ex) {}
            return "Unknown BLE state.";
        }

        bool _scanAgain = true;

        private void Adapter_ScanTimeoutElapsed(object sender, EventArgs e) {
            RaisePropertyChanged(() => IsRefreshing);

            CleanupCancellationToken();

            if (_scanAgain) ScanForDevices();
        }

        private void OnDeviceDiscovered(object sender, DeviceEventArgs args) {
            try {
                bool CS108Service = false;

                // CS108 filter
                switch (Xamarin.Forms.Device.RuntimePlatform) {
                    case Xamarin.Forms.Device.UWP:
                        if (args.Device.AdvertisementRecords.Count == 0) CS108Service = true;
                        break;

                    default:
                        if (args.Device.AdvertisementRecords.Count < 1)
                            return;

                        foreach (AdvertisementRecord service in args.Device.AdvertisementRecords) {
                            if (service.Data.Length == 2) {
                                if (service.Data[0] == 0x98 && service.Data[1] == 0x00) {
                                    CS108Service = true;
                                    break;
                                }
                            }
                        }
                        break;
                }

                if (!CS108Service) return;

                AddOrUpdateDevice(args.Device);
            }
            catch (Exception ex) {
                CSLibrary.Debug.WriteLine("Can not handle desconvered device");
            }
        }

        private void AddOrUpdateDevice(IDevice device) {
            InvokeOnMainThread(() => {
                try {
                    var vm = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
                    if (vm != null) {
                        vm.Update(device);
                    }
                    else {
                        Devices.Add(new DeviceListItemViewModel(device));
                    }
                }
                catch (Exception ex) {
                    CSLibrary.Debug.WriteLine("Can not add device");
                }
            });
        }

        public override async void Resume() {
            try {
                base.Resume();
                await GetPreviousGuidAsync();
                TryStartScanning();
                GetSystemConnectedOrPairedDevices();
            }
            catch (Exception ex) {
                CSLibrary.Debug.WriteLine("Device Resume Error");
            }
        }

        private void GetSystemConnectedOrPairedDevices() {
            try {
                var guid = Guid.Parse("0000180d-0000-1000-8000-00805f9b34fb");

                SystemDevices = Adapter.GetSystemConnectedOrPairedDevices(new[] { guid }).Select(d => new DeviceListItemViewModel(d)).ToList();
                RaisePropertyChanged(() => SystemDevices);
            }
            catch (Exception ex) {
                Trace.Message("Failed to retreive system connected devices. {0}", ex.Message);
            }
        }

        public List<DeviceListItemViewModel> SystemDevices { get; private set; }

        public override void Suspend() {
            try {
                base.Suspend();

                Adapter.StopScanningForDevicesAsync();
                RaisePropertyChanged(() => IsRefreshing);
            }
            catch (Exception ex)
            {
                CSLibrary.Debug.WriteLine("Device Suspend error");
            }
        }

        private async void TryStartScanning(bool refresh = false) {
            if (IsStateOn && (refresh || !Devices.Any()) && !IsRefreshing) {
                Devices.Clear();
                ScanForDevices();
            }
        }

        private async void ScanForDevices() {
            try {
                _cancellationTokenSource = new CancellationTokenSource();
                RaisePropertyChanged(() => StopScanCommand);

                RaisePropertyChanged(() => IsRefreshing);
                Adapter.ScanMode = ScanMode.LowLatency;
                await Adapter.StartScanningForDevicesAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex) {
                CSLibrary.Debug.WriteLine("Can not Scan devices");
            }
        }

        private void CleanupCancellationToken() {
            try {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                RaisePropertyChanged(() => StopScanCommand);

                if (_scanAgain) ScanForDevices();
            }
            catch (Exception ex) {
                CSLibrary.Debug.WriteLine("Can not stop _cancellationTokenSource");
            }
        }

        private async void DisconnectDevice(DeviceListItemViewModel device) {
            if (BleMvxApplication._reader.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT)
                BleMvxApplication._reader.DisconnectAsync();

            try {
                if (!device.IsConnected) return;
                _userDialogs.ShowLoading($"Disconnecting {device.Name}...");
                await Adapter.DisconnectDeviceAsync(device.Device);
            }
            catch (Exception ex) {
                _userDialogs.Alert(ex.Message, "Disconnect error");
            }
            finally {
                device.Update();
                _userDialogs.HideLoading();
            }
        }

        private async void HandleSelectedDevice(DeviceListItemViewModel devices) {
            try {
                if (await ConnectDeviceAsync(devices)) {
                    // Connect to CS108

                    var device = Adapter.ConnectedDevices.FirstOrDefault(d => d.Id.Equals(devices.Device.Id));

                    if (device == null) return;

                    Connect(device);
                    Close(this);
                }
            }
            catch (Exception ex) {
                _userDialogs.Alert(ex.Message, "Disconnect error");
            }

        }

        private async Task<bool> ConnectDeviceAsync(DeviceListItemViewModel device, bool showPrompt = true) {
            if (showPrompt && !await _userDialogs.ConfirmAsync($"Connect to device '{device.Name}'?")) {
                return false;
            }

            try {
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                // Default CONNECTPARAMETERS
                // ConnectParameters connectParameters = new ConnectParameters();

                // New connect parameters forcing Reconnect on Android
                ConnectParameters connectParameters = new ConnectParameters(true, false);
                await Adapter.ConnectToDeviceAsync(device.Device, connectParameters, tokenSource.Token);

                _userDialogs.ShowSuccess($"Initializing Reader, Please Wait.", 8000);

                PreviousGuid = device.Device.Id;

                return true;
            }
            catch (Exception ex) {
                _userDialogs.Alert(ex.Message, "Connection error");
                Mvx.Trace(ex.Message);
                return false;
            }
            finally {
                device.Update();
            }
        }

        private async void Connect(IDevice _device) {
            Trace.Message("device name :" + _device.Name);

            await BleMvxApplication._reader.ConnectAsync(Adapter, _device);

            Trace.Message("load config");

            bool LoadSuccess = await BleMvxApplication.LoadConfig(_device.Id.ToString());
            BleMvxApplication._config.readerID = _device.Id.ToString();
        }

        public MvxCommand ConnectToPreviousCommand => new MvxCommand(ConnectToPreviousDeviceAsync, CanConnectToPrevious);

        private async void ConnectToPreviousDeviceAsync() {
            IDevice device;
            try {
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                ConnectParameters connectParameters = new ConnectParameters();

                var config = new ProgressDialogConfig() {
                    Title = $"Searching for '{PreviousGuid}'",
                    CancelText = "Cancel",
                    IsDeterministic = false,
                    OnCancel = tokenSource.Cancel
                };

                using (var progress = _userDialogs.Progress(config)) {
                    progress.Show();

                    device = await Adapter.ConnectToKnownDeviceAsync(PreviousGuid, connectParameters, tokenSource.Token);
                }

                var deviceItem = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
                if (deviceItem == null) {
                    deviceItem = new DeviceListItemViewModel(device);
                    Devices.Add(deviceItem);
                }
                else {
                    deviceItem.Update(device);
                }
            }
            catch (Exception ex) {
                _userDialogs.ShowError(ex.Message, 5000);
                return;
            }
        }

        private bool CanConnectToPrevious() {
            return PreviousGuid != default(Guid);
        }

        private async void ConnectAndDisposeDevice(DeviceListItemViewModel item) {
            try {
                using (item.Device) {
                    await Adapter.ConnectToDeviceAsync(item.Device);
                    item.Update();
                }
            }
            catch (Exception ex) {
                _userDialogs.Alert(ex.Message, "Failed to connect and dispose.");
            }
            finally {
                _userDialogs.HideLoading();
            }
        }

        private void OnDeviceDisconnected(object sender, DeviceEventArgs e) {
            Devices.FirstOrDefault(d => d.Id == e.Device.Id)?.Update();
            _userDialogs.HideLoading();
            _userDialogs.Toast($"Disconnected {e.Device.Name}");
        }

        public MvxCommand<DeviceListItemViewModel> CopyGuidCommand => new MvxCommand<DeviceListItemViewModel>(device => {
            PreviousGuid = device.Id;
        });

    }
}
