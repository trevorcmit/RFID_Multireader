using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Connectivity;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;


namespace Plugin.BLE.UWP {
    public class Adapter : AdapterBase {
        private BluetoothLEHelper _bluetoothHelper;
        private BluetoothLEAdvertisementWatcher _BleWatcher;
        private IList<ulong> _prevScannedDevices;
        public IDictionary<string, IDevice> ConnectedDeviceRegistry { get; }
        public override IList<IDevice> ConnectedDevices => ConnectedDeviceRegistry.Values.ToList();
        
        public Adapter (BluetoothLEHelper bluetoothHelper) {
            _bluetoothHelper = bluetoothHelper;
            ConnectedDeviceRegistry = new Dictionary<string, IDevice>();
        }

        protected override Task StartScanningForDevicesNativeAsync(Guid[] serviceUuids, bool allowDuplicatesKey, CancellationToken scanCancellationToken) {
            var hasFilter = serviceUuids?.Any() ?? false;
            DiscoveredDevices.Clear();
            _BleWatcher = new BluetoothLEAdvertisementWatcher();
            _BleWatcher.ScanningMode = BluetoothLEScanningMode.Active;
            _prevScannedDevices = new List<ulong>();
            Trace.Message("Starting a scan for devices.");
            if (hasFilter) {
                foreach (var uuid in serviceUuids) {
                    _BleWatcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(uuid);
                }
                Trace.Message($"ScanFilters: {string.Join(", ", serviceUuids)}");
            }

            if (allowDuplicatesKey) {
                _BleWatcher.Received += DeviceFoundAsyncDuplicate;
            }
            else {
                _BleWatcher.Received += DeviceFoundAsync;
            }
            _BleWatcher.Start();
            return Task.FromResult(true);
        }

        protected override void StopScanNative() {
            Trace.Message("Stopping the scan for devices");
            _BleWatcher.Stop();
            _BleWatcher = null;
        }

        protected async override Task ConnectToDeviceNativeAsync(IDevice device, ConnectParameters connectParameters, CancellationToken cancellationToken) {
            Trace.Message($"Connecting to device with ID:  {device.Id.ToString()}");

            ObservableBluetoothLEDevice nativeDevice = device.NativeDevice as ObservableBluetoothLEDevice;
            if (nativeDevice == null)
                return;

            var uwpDevice = (Device)device;
            uwpDevice.ConnectionStatusChanged += Device_ConnectionStatusChanged;

            await nativeDevice.ConnectAsync();

            if (!ConnectedDeviceRegistry.ContainsKey(uwpDevice.Id.ToString())) ConnectedDeviceRegistry.Add(uwpDevice.Id.ToString(), device);
        }

        private void Device_ConnectionStatusChanged(Device device, BluetoothConnectionStatus status) {
            if (status == BluetoothConnectionStatus.Connected) HandleConnectedDevice(device);
            else                                               HandleDisconnectedDevice(true, device);
        }

        protected override void DisconnectDeviceNative(IDevice device) {
            Trace.Message($"Disconnected from device with ID:  {device.Id.ToString()}");
            ConnectedDeviceRegistry.Remove(device.Id.ToString());
        }

        public async override Task<IDevice> ConnectToKnownDeviceAsync(Guid deviceGuid, ConnectParameters connectParameters, CancellationToken cancellationToken) {
            var guidString = deviceGuid.ToString("N").Substring(20);
            ulong bluetoothAddr = Convert.ToUInt64(guidString, 16);
            var nativeDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddr);
            var currDevice = new Device(this, nativeDevice, 0, guidString);
            await ConnectToDeviceAsync(currDevice);
            return currDevice;
        }

        public override List<IDevice> GetSystemConnectedOrPairedDevices(Guid[] services = null) {
            //currently no way to retrieve paired and connected devices on windows without using an async method. 
            Trace.Message("Returning devices connected by this app only");
            return (List<IDevice>) ConnectedDevices;
        }
        
        /// <param name="adv">The advertisement to parse</param>
        /// <returns>List of generic advertisement records</returns>
        public static List<AdvertisementRecord> ParseAdvertisementData(BluetoothLEAdvertisement adv) {
            var advList = adv.DataSections;
            var records = new List<AdvertisementRecord>();
            foreach (var data in advList) {
                var type = data.DataType;
                if (type == BluetoothLEAdvertisementDataTypes.ManufacturerSpecificData) {
                    records.Add(new AdvertisementRecord(AdvertisementRecordType.ManufacturerSpecificData, data.Data.ToArray()));
                }
            }
            return records;
        }

        /// <param name="watcher">The bluetooth advertisement watcher currently being used</param>
        /// <param name="btAdv">The advertisement recieved by the watcher</param>
        private async void DeviceFoundAsync(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs btAdv) {
            if (!_prevScannedDevices.Contains(btAdv.BluetoothAddress)) {
                _prevScannedDevices.Add(btAdv.BluetoothAddress);
                BluetoothLEDevice currDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(btAdv.BluetoothAddress);
                if (currDevice != null)     //make sure advertisement bluetooth address actually returns a device
                {
                    var device = new Device(this, currDevice, btAdv.RawSignalStrengthInDBm, btAdv.BluetoothAddress.ToString(), ParseAdvertisementData(btAdv.Advertisement));
                    Trace.Message("DiscoveredPeripheral: {0} Id: {1}", device.Name, device.Id);
                    this.HandleDiscoveredDevice(device);
                }
                return;
            }
        }

        /// <param name="watcher">The bluetooth advertisement watcher currently being used</param>
        /// <param name="btAdv">The advertisement recieved by the watcher</param>
        private async void DeviceFoundAsyncDuplicate(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs btAdv) {
            BluetoothLEDevice currDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(btAdv.BluetoothAddress);
            if (currDevice != null) {
                var device = new Device(this, currDevice, btAdv.RawSignalStrengthInDBm, btAdv.BluetoothAddress.ToString(), ParseAdvertisementData(btAdv.Advertisement));
                Trace.Message("DiscoveredPeripheral: {0} Id: {1}", device.Name, device.Id);
                this.HandleDiscoveredDevice(device);
            }
            return;
        }

    }
}