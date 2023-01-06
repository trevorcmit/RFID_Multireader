using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.OS;
using Java.Util;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Extensions;
using Object = Java.Lang.Object;
using Trace = Plugin.BLE.Abstractions.Trace;
using Android.App;


namespace Plugin.BLE.Android {
    public class Adapter : AdapterBase {
        private readonly BluetoothManager _bluetoothManager;
        private readonly BluetoothAdapter _bluetoothAdapter;
        private readonly Api18BleScanCallback _api18ScanCallback;
        private readonly Api21BleScanCallback _api21ScanCallback;

        public override IList<IDevice> ConnectedDevices => ConnectedDeviceRegistry.Values.ToList();

        public Dictionary<string, IDevice> ConnectedDeviceRegistry { get; }

        public Adapter(BluetoothManager bluetoothManager)
        {
            _bluetoothManager = bluetoothManager;
            _bluetoothAdapter = bluetoothManager.Adapter;

            ConnectedDeviceRegistry = new Dictionary<string, IDevice>();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop) {
                _api21ScanCallback = new Api21BleScanCallback(this);
            }
            else {
                _api18ScanCallback = new Api18BleScanCallback(this);
            }
        }

        protected override Task StartScanningForDevicesNativeAsync(Guid[] serviceUuids, bool allowDuplicatesKey, CancellationToken scanCancellationToken) {
            DiscoveredDevices.Clear(); // clear out the list

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop) {
                StartScanningOld(serviceUuids);
            }
            else {
                StartScanningNew(serviceUuids);
            }

            return Task.FromResult(true);
        }

        private void StartScanningOld(Guid[] serviceUuids) {
            var hasFilter = serviceUuids?.Any() ?? false;
            UUID[] uuids = null;
            if (hasFilter)
            {
                uuids = serviceUuids.Select(u => UUID.FromString(u.ToString())).ToArray();
            }
            Trace.Message("Adapter < 21: Starting a scan for devices.");
#pragma warning disable 618
            _bluetoothAdapter.StartLeScan(uuids, _api18ScanCallback);
#pragma warning restore 618
        }

        private void StartScanningNew(Guid[] serviceUuids) {
            var hasFilter = serviceUuids?.Any() ?? false;
            List<ScanFilter> scanFilters = null;

            if (hasFilter) {
                scanFilters = new List<ScanFilter>();
                foreach (var serviceUuid in serviceUuids)
                {
                    var sfb = new ScanFilter.Builder();
                    sfb.SetServiceUuid(ParcelUuid.FromString(serviceUuid.ToString()));
                    scanFilters.Add(sfb.Build());
                }
            }

            var ssb = new ScanSettings.Builder();
            ssb.SetScanMode(ScanMode.ToNative());
            //ssb.SetCallbackType(ScanCallbackType.AllMatches);

            if (_bluetoothAdapter.BluetoothLeScanner != null)
            {
                Trace.Message($"Adapter >=21: Starting a scan for devices. ScanMode: {ScanMode}");
                if (hasFilter)
                {
                    Trace.Message($"ScanFilters: {string.Join(", ", serviceUuids)}");
                }
                _bluetoothAdapter.BluetoothLeScanner.StartScan(scanFilters, ssb.Build(), _api21ScanCallback);
            }
            else
            {
                Trace.Message("Adapter >= 21: Scan failed. Bluetooth is probably off");
            }
        }

        protected override void StopScanNative()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                Trace.Message("Adapter < 21: Stopping the scan for devices.");
#pragma warning disable 618
                _bluetoothAdapter.StopLeScan(_api18ScanCallback);
#pragma warning restore 618
            }
            else
            {
                Trace.Message("Adapter >= 21: Stopping the scan for devices.");
                _bluetoothAdapter.BluetoothLeScanner?.StopScan(_api21ScanCallback);
            }
        }

        protected override Task ConnectToDeviceNativeAsync(IDevice device, ConnectParameters connectParameters,
            CancellationToken cancellationToken)
        {
            ((Device)device).Connect(connectParameters);
            return Task.CompletedTask;
        }

        protected override void DisconnectDeviceNative(IDevice device) { ((Device)device).Disconnect(); }

        public override async Task<IDevice> ConnectToKnownDeviceAsync(Guid deviceGuid, ConnectParameters connectParameters = default(ConnectParameters), CancellationToken cancellationToken = default(CancellationToken))
        {
            var macBytes = deviceGuid.ToByteArray().Skip(10).Take(6).ToArray();
            var nativeDevice = _bluetoothAdapter.GetRemoteDevice(macBytes);

            var device = new Device(this, nativeDevice, null, 0, new byte[] { });

            await ConnectToDeviceAsync(device, connectParameters, cancellationToken);
            return device;
        }

        public override List<IDevice> GetSystemConnectedOrPairedDevices(Guid[] services = null)
        {
            if (services != null)
            {
                Trace.Message("Caution: GetSystemConnectedDevices does not take into account the 'services' parameter on Android.");
            }

            //add dualMode type too as they are BLE too ;)
            var connectedDevices = _bluetoothManager.GetConnectedDevices(ProfileType.Gatt).Where(d => d.Type == BluetoothDeviceType.Le || d.Type == BluetoothDeviceType.Dual);

            var bondedDevices = _bluetoothAdapter.BondedDevices.Where(d => d.Type == BluetoothDeviceType.Le || d.Type == BluetoothDeviceType.Dual);

            return connectedDevices.Union(bondedDevices, new DeviceComparer()).Select(d => new Device(this, d, null, 0)).Cast<IDevice>().ToList();
        }

        private class DeviceComparer : IEqualityComparer<BluetoothDevice>
        {
            public bool Equals(BluetoothDevice x, BluetoothDevice y)
            {
                return x.Address == y.Address;
            }

            public int GetHashCode(BluetoothDevice obj)
            {
                return obj.GetHashCode();
            }
        }


        public class Api18BleScanCallback : Object, BluetoothAdapter.ILeScanCallback {
            private readonly Adapter _adapter;

            public Api18BleScanCallback(Adapter adapter)
            {
                _adapter = adapter;
            }

            public void OnLeScan(BluetoothDevice bleDevice, int rssi, byte[] scanRecord) {
                Trace.Message("Adapter.LeScanCallback: " + bleDevice.Name);
                _adapter.HandleDiscoveredDevice(new Device(_adapter, bleDevice, null, rssi, scanRecord));
            }
        }


        public class Api21BleScanCallback : ScanCallback {
            private readonly Adapter _adapter;
            public Api21BleScanCallback(Adapter adapter) {
                _adapter = adapter;
            }

            public override void OnScanFailed(ScanFailure errorCode) {
                Trace.Message("Adapter: Scan failed with code {0}", errorCode);
                base.OnScanFailed(errorCode);
            }

            public override void OnScanResult(ScanCallbackType callbackType, ScanResult result) {
                base.OnScanResult(callbackType, result);
                var device = new Device(_adapter, result.Device, null, result.Rssi, result.ScanRecord.GetBytes());
                _adapter.HandleDiscoveredDevice(device);
            }
        }

    }
}



