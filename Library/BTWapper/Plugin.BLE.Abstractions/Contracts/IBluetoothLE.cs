using System;
using Plugin.BLE.Abstractions.EventArgs;


namespace Plugin.BLE.Abstractions.Contracts {
    public interface IBluetoothLE {
        /// <summary>
        /// Occurs when <see cref="State"/> has changed.
        /// </summary>
        event EventHandler<BluetoothStateChangedArgs> StateChanged;

        BluetoothState State {get;}

        bool IsAvailable {get;}

        /// <summary>
        /// Indicates whether the bluetooth adapter is turned on or not.
        /// <c>true</c> if <see cref="State"/> is <c>BluetoothState.On</c>
        /// </summary>
        bool IsOn {get;}

        IAdapter Adapter {get;}
    }
}