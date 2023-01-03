using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;


namespace CSLibrary {
    public partial class HighLevelInterface {
        // for bluetooth Connection
        IAdapter _adapter;
        IDevice _device;
        IService _service;
        ICharacteristic _characteristicWrite;
        ICharacteristic _characteristicUpdate;

        int BLE_Init() { return 0; }

        public async Task<string> ConnectAsync(IAdapter adapter, IDevice device)
        {
            if (_readerState != READERSTATE.DISCONNECT)
                return "False, Readerstate is not Disconnect";   // Reader can not reconnect

            try {
                _service = await device.GetServiceAsync(Guid.Parse("00009800-0000-1000-8000-00805f9b34fb"));
                if (_service == null)
                    return "False, Can not find service";
            }
            catch (Exception ex) {}

            _readerState = READERSTATE.IDLE;

            _adapter = adapter;
            _device = device;

            _adapter.DeviceConnectionLost -= OnDeviceConnectionLost;
            _adapter.DeviceConnectionLost += OnDeviceConnectionLost;

            try {
                _characteristicWrite = await _service.GetCharacteristicAsync(Guid.Parse("00009900-0000-1000-8000-00805f9b34fb"));
                _characteristicUpdate = await _service.GetCharacteristicAsync(Guid.Parse("00009901-0000-1000-8000-00805f9b34fb"));
            }
            catch (Exception ex) {
                CSLibrary.Debug.WriteLine("Cannot set characters");
            }

            _characteristicUpdate.ValueUpdated -= BLE_Recv;
            _characteristicUpdate.ValueUpdated += BLE_Recv;

            await _characteristicUpdate.StartUpdatesAsync();

            _readerState = READERSTATE.IDLE;
            BTTimer = new Timer(TimerFunc, this, 0, 1000);

            HardwareInit();

            // return true;
            return "True, Finished to the End";
        }

        public async Task<string> DisconnectAsync()
        {
            try {
                if (Status != READERSTATE.IDLE) 
                    return "false, reader state is not idle";

                if (_readerState != READERSTATE.DISCONNECT) {
                    BARCODEPowerOff();
                    WhenBLEFinish(ClearConnection);

                    _readerState = READERSTATE.DISCONNECT;

                    return "if != disconnect";
                }
                else {
                    await ClearConnection();
                    return "else case";
                }
            }
            catch (Exception ex) {
                Debug.WriteLine("Disconnect error " + ex.Message.ToString());
            }
            return "true, finished to the end";
        }

        private async Task<bool> BLE_Send (byte[] data) {
            return await _characteristicWrite.WriteAsync(data);
        }

        private async void BLE_Recv(object sender, CharacteristicUpdatedEventArgs characteristicUpdatedEventArgs) {
            try {
                byte[] data = characteristicUpdatedEventArgs.Characteristic.Value;
                CSLibrary.Debug.WriteBytes("BT data received", data);

                CharacteristicOnValueUpdated(data);
            }
            catch (Exception ex) {
                Debug.WriteLine("Program execption error, please check!!! error message : " + ex.Message);
            }
        }

        private void CharacteristicOnWriteUpdated(object sender, CharacteristicUpdatedEventArgs characteristicUpdatedEventArgs) {
            CSLibrary.Debug.WriteBytes("BT: Write data success updated", characteristicUpdatedEventArgs.Characteristic.Value);
        }

        private void OnStateChanged(object sender, BluetoothStateChangedArgs e) {}

        private void OnDeviceConnectionLost(object sender, DeviceErrorEventArgs e) {
            if (e.Device.Id == _device.Id) {
                ConnectLostAsync();
            }
        }

        public async Task<string> ConnectLostAsync() {
            _readerState = READERSTATE.READYFORDISCONNECT;

            _characteristicUpdate.ValueUpdated -= BLE_Recv;
            _adapter.DeviceConnectionLost -= OnDeviceConnectionLost;

            _characteristicUpdate = null;
            _characteristicWrite = null;
            _service = null;

            try {
                if (_device.State == DeviceState.Connected) {
                    await _adapter.DisconnectDeviceAsync(_device);
                }
            }
            catch (Exception ex) {}

            _device = null;
            _readerState = READERSTATE.DISCONNECT;
            FireReaderStateChangedEvent(new Events.OnReaderStateChangedEventArgs(null, Constants.ReaderCallbackType.CONNECTION_LOST));

            // ADDED RETURN TO MAKE AWAIT-ABLE
            return "Completed ConnectLostAsync";
        }

        public async Task ClearConnection() {
            _readerState = READERSTATE.READYFORDISCONNECT;

            await _characteristicUpdate.StopUpdatesAsync(); // Stop Timer;

            _characteristicUpdate.ValueUpdated -= BLE_Recv;
            _adapter.DeviceConnectionLost -= OnDeviceConnectionLost;

            _characteristicUpdate = null;
            _characteristicWrite = null;
            _service = null;

            try {
                if (_device.State == DeviceState.Connected) {
                    await _adapter.DisconnectDeviceAsync(_device);
                }
            }
            catch (Exception ex) {}

            _device = null;
            _readerState = READERSTATE.DISCONNECT;
        }

        // ADDED: Set the reader state to disconnect for auto-reconnect
        public string Get_ReaderState_String() 
        {
            return _readerState.ToString();
        }

    }
}
