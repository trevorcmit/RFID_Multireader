using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using wclCommon;
using wclBluetooth;


namespace CSLibrary {
    class IDevice {
        public string Name = "";
    }

    public partial class HighLevelInterface {
        // for bluetooth Connection
        private wclGattClient Client;

        private wclGattCharacteristic[] FCharacteristics;
        private wclGattDescriptor[] FDescriptors;
        private wclGattService[] FServices;

        wclGattService CS108Service;
        wclGattCharacteristic UpdateCharacteristic;
        wclGattCharacteristic WriteCharacteristic;

        IDevice _device = new IDevice();

        /// <returns></returns>
        int BLE_Init() {
            Client = new wclGattClient();

            Client.OnCharacteristicChanged += new wclGattCharacteristicChangedEvent(Client_OnCharacteristicChanged);
            Client.OnConnect += new wclCommunication.wclClientConnectionConnectEvent(Client_OnConnect);
            Client.OnDisconnect += new wclCommunication.wclClientConnectionDisconnectEvent(Client_OnDisconnect);

            // In real application you should always analize the result code.
            // In this demo we assume that all is always OK.
            Cleanup();
            return 0;
        }

        private void Cleanup() {
            FCharacteristics = null;
            FDescriptors = null;
            FServices = null;
        }

        public async Task<bool> ConnectAsync(object macAdd) {
            if (_readerState != READERSTATE.DISCONNECT)
                return false; // reader can not reconnect

            Int32 Res;

            Client.Address = (long)macAdd;
            Res = Client.Connect(DeviceFinder.Radio);
            if (Res != wclErrors.WCL_E_SUCCESS) {
                CSLibrary.Debug.WriteLine("Error: 0x" + Res.ToString("X8"));
                return false;
            }
            return true;
        }

        void Client_OnConnect(object Sender, int Error) {
            CSLibrary.Debug.WriteLine ("Connected");

            int Res;

            // Get Services
            FServices = null;
            Res = Client.ReadServices(wclGattOperationFlag.goNone, out FServices);
            if (Res != wclErrors.WCL_E_SUCCESS) {
                CSLibrary.Debug.WriteLine ("ReadServices Error: 0x" + Res.ToString("X8"));
                return;
            }

            if (FServices == null) {
                CSLibrary.Debug.WriteLine("ReadServices API Fail");
                return;
            }

            foreach (wclGattService Service in FServices) {
                if (Service.Uuid.IsShortUuid)
                    if (Service.Uuid.ShortUuid == 0x9800)
                        CS108Service = Service;
            }

            // Get Chatacteristics
            Res = Client.ReadCharacteristics(CS108Service, wclGattOperationFlag.goReadFromDevice, out FCharacteristics);
            if (Res != wclErrors.WCL_E_SUCCESS) {
                CSLibrary.Debug.WriteLine("ReadCharacteristics Error: 0x" + Res.ToString("X8"));
                return;
            }

            if (FCharacteristics == null)
                return;

            foreach (wclGattCharacteristic Character in FCharacteristics) {
                String s;
                if (Character.Uuid.IsShortUuid)
                    if (Character.Uuid.ShortUuid == 0x9900)
                        WriteCharacteristic = Character;
                    else if (Character.Uuid.ShortUuid == 0x9901)
                        UpdateCharacteristic = Character;
            }


            Res = Client.Subscribe(UpdateCharacteristic);
            if (Res != wclErrors.WCL_E_SUCCESS) {
                CSLibrary.Debug.WriteLine("Subscribe Error: 0x" + Res.ToString("X8"));
            }

            Res = Client.WriteClientConfiguration(UpdateCharacteristic, true, wclGattOperationFlag.goNone); // for library 7.3.9.0 above 
            if (Res != wclErrors.WCL_E_SUCCESS) {
                CSLibrary.Debug.WriteLine("WriteClientConfiguration Error: 0x" + Res.ToString("X8"));
            }

            _readerState = READERSTATE.IDLE;
            BTTimer = new Timer(TimerFunc, this, 0, 1000);

            HardwareInit();
        }


        public async Task<bool> DisconnectAsync() {
            if (Status != READERSTATE.IDLE)
                return false;

            BARCODEPowerOff();
            WhenBLEFinish(ClearConnection);

            return true;
        }

        private async Task<bool> BLE_Send(byte[] data) {
            Int32 Res = Client.WriteCharacteristicValue(WriteCharacteristic, data);
            if (Res != wclErrors.WCL_E_SUCCESS)
            {
                CSLibrary.Debug.WriteLine("Error: 0x" + Res.ToString("X8"));
                return false;
            }

            return true;
        }

        void Client_OnCharacteristicChanged(object Sender, ushort Handle, byte[] Value) {
            if (Value != null)
                if (Value.Length > 0) {
                    CharacteristicOnValueUpdated(Value);
                }
        }

        void Client_OnDisconnect(object Sender, int Reason) {}

        async Task ClearConnection() {
            _readerState = READERSTATE.READYFORDISCONNECT;

            Int32 Res = wclErrors.WCL_E_MB_NOT_CREATED, errCnt = 0;

            while (Res != wclErrors.WCL_E_SUCCESS && errCnt++ < 20) {
                Res = Client.Disconnect();
                CSLibrary.Debug.WriteLine("Disconnect Error: 0x" + Res.ToString("X8"));
            }

            _readerState = READERSTATE.DISCONNECT;
        }

    }
}
