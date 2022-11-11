using System;
using System.Collections.Generic;
using wclCommon;
using wclBluetooth;


namespace CSLibrary {
    public partial class DeviceFinder {
        static private wclBluetoothManager Manager;
        static internal wclBluetoothRadio Radio;

        public class DeviceFinderArgs : EventArgs {
            private DeviceInfomation _data;

            /// <param name="data"></param>
            public DeviceFinderArgs(DeviceInfomation data) {
                _data = data;
            }

            /// <summary>
            /// Device finder information
            /// </summary>
            public DeviceInfomation Found {
                get { return _data; }
                set { _data = value; }
            }
        }

        public class DeviceInfomation {
            public uint ID;
            public string deviceName;
            public long macAdd;
            public object nativeDeviceInformation;
        }

	    static List<long> _deviceDB = new List<long>();
        static public event EventHandler<DeviceFinderArgs> OnSearchCompleted;

        static public void SearchDevice() {
            if (Manager == null) {
                Manager = new wclBluetoothManager();
                Manager.OnDeviceFound += new wclBluetoothDeviceEvent(Manager_OnDeviceFound);
                Manager.OnDiscoveringCompleted += new wclBluetoothResultEvent(Manager_OnDiscoveringCompleted);

                Manager.Open();

                Radio = GetRadio();
            }

            if (Radio != null) {
                Int32 Res = Radio.Discover(3, wclBluetoothDiscoverKind.dkBle);
                if (Res != wclErrors.WCL_E_SUCCESS)
                    CSLibrary.Debug.WriteLine ("Error starting discovering: 0x" + Res.ToString("X8"));
            }
        }

        static private wclBluetoothRadio GetRadio() {
            // It is a little bit complex here:
            // As Microsoft Bluetooth driver is always available in the system
            // there can be 2 radios retruned if other than MS driver also installed.
            // This rountine checks that and returns second radio in such case.
            if (Manager.Count == 1) return Manager[0];

            // Other driver also installed.
            for (Int32 i = 0; i < Manager.Count; i++)
                if (Manager[i].Api != wclBluetoothApi.baMicrosoft)
                    // Return first non MS.
                    return Manager[i];

            return null;
        }

        static public void Stop() {}

	    static public void ClearDeviceList() {
		    _deviceDB.Clear ();
	    }

        static public object GetDeviceInformation(int id) {
            if (id < _deviceDB.Count)
                return _deviceDB[id];

            return null;
        }

        static public object GetDeviceInformation (string readername) {
		    foreach (long item in _deviceDB) {
                string name;

                Radio.GetRemoteName(item, out name);

                if (name == readername) return item;
		    }

		    return null;		
	    }

	    static public List<long> GetAllDeviceInformation () {
		    return _deviceDB;
	    }

        static void Manager_OnDiscoveringCompleted(object Sender, wclBluetoothRadio Radio, int Error) {
            System.Console.WriteLine("Discovering completed");
        }

        static void Manager_OnDeviceFound(object Sender, wclBluetoothRadio Radio, long Address) {
            wclBluetoothDeviceType DevType = wclBluetoothDeviceType.dtMixed;
            Int32 Res = Radio.GetRemoteDeviceType(Address, out DevType);

            if (Res != wclErrors.WCL_E_SUCCESS)
                CSLibrary.Debug.WriteLine ("Error: 0x" + Res.ToString("X8"));
            else
                switch (DevType) {
                    case wclBluetoothDeviceType.dtBle:
                        {
                            CSLibrary.DeviceFinder.DeviceInfomation di = new CSLibrary.DeviceFinder.DeviceInfomation();
                            string DevName;
                            sbyte RSSI;

                            Res = Radio.GetRemoteRssi(Address, out RSSI);
                            if (Res != wclErrors.WCL_E_SUCCESS)
                                RSSI = 0;
                            Res = Radio.GetRemoteName(Address, out DevName);
                            if (Res != wclErrors.WCL_E_SUCCESS)
                                di.deviceName = DevName = "Error: 0x" + Res.ToString("X8");
                            else
                                di.deviceName = DevName;

                            di.ID = (uint)_deviceDB.Count;
                            di.macAdd = Address;
                            di.nativeDeviceInformation = null;

                            _deviceDB.Add(Address);

                            RaiseEvent<DeviceFinderArgs>(OnSearchCompleted, new DeviceFinderArgs(di));
                        }
                        break;
                }
        }

        static private void RaiseEvent<T>(EventHandler<T> eventHandler, T e) where T : EventArgs {
            if (eventHandler != null) {
                eventHandler(null, e);
            }
            return;
        }

    }
}
