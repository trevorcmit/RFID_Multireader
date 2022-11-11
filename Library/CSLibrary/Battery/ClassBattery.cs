using System;
using System.Text;
using CSLibrary.Barcode;
using CSLibrary.Barcode.Constants;
using CSLibrary.Barcode.Structures;


namespace CSLibrary {
	public partial class Battery {
		HighLevelInterface _deviceHandler;
		uint _pollingTime = 300; // 5 second
		bool _autoBatteryLevel = false;
		DateTime _nextTime = DateTime.MaxValue;

		internal Battery(HighLevelInterface handler) {
			_deviceHandler = handler;
		}

		public bool GetCurrentAutoBattryStatus () {
			return _autoBatteryLevel;
		}

		public void SetPollingTime (uint sec) {
			_pollingTime = sec; 
		}

		internal void EnableAutoBatteryLevel () {}

		internal void DisbleAutoBatteryLevel() {}

		internal void GetBatteryLevel() {
			_deviceHandler.notification.GetCurrentBatteryVoltage();
		}

		internal void Timer () {
			if (_deviceHandler.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT && !_autoBatteryLevel && _pollingTime != 0) {
				if (DateTime.Now >= _nextTime) {
					_nextTime = DateTime.Now.AddSeconds(_pollingTime);
					GetBatteryLevel();
				}
			}
		}

	}
}
