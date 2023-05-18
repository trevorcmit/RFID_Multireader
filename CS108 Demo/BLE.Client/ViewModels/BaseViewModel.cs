// Default BaseViewModel Imports
using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using Plugin.BLE.Abstractions.Contracts;

// New Imports for Bluetooth Autoconnect
using Acr.UserDialogs;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Extensions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;


namespace BLE.Client.ViewModels
{
    public class BaseViewModel : MvxViewModel
    {
        ////////////////////////////////////////////////////////////////////////////
        // Original BaseViewModel Functions/Vars ///////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////
        protected readonly IAdapter Adapter;

        protected const string DeviceIdKey = "DeviceIdNavigationKey";
        protected const string ServiceIdKey = "ServiceIdNavigationKey";
        protected const string CharacteristicIdKey = "CharacteristicIdNavigationKey";
        protected const string DescriptorIdKey = "DescriptorIdNavigationKey";

        public BaseViewModel(IAdapter adapter) {
            Adapter = adapter;
        }

        public virtual void Resume() {
            Mvx.Trace("Resume {0}", GetType().Name);
        }

        public virtual void Suspend() {
            Mvx.Trace("Suspend {0}", GetType().Name);
        }

        protected override void InitFromBundle(IMvxBundle parameters) {
            base.InitFromBundle(parameters);
            Bundle = parameters;
        }

        protected IMvxBundle Bundle {get; private set;}

        protected IDevice GetDeviceFromBundle(IMvxBundle parameters) {
            if (!parameters.Data.ContainsKey(DeviceIdKey)) return null;
            var deviceId = parameters.Data[DeviceIdKey];

            return Adapter.ConnectedDevices.FirstOrDefault(d => d.Id.ToString().Equals(deviceId));

        }

        protected Task<IService> GetServiceFromBundleAsync(IMvxBundle parameters) {

            var device = GetDeviceFromBundle(parameters);
            if (device == null || !parameters.Data.ContainsKey(ServiceIdKey)) {
                return Task.FromResult<IService>(null);
            }

            var serviceId = parameters.Data[ServiceIdKey];
            return device.GetServiceAsync(Guid.Parse(serviceId));
        }

        protected async Task<ICharacteristic> GetCharacteristicFromBundleAsync(IMvxBundle parameters) {
            var service = await GetServiceFromBundleAsync(parameters);
            if (service == null || !parameters.Data.ContainsKey(CharacteristicIdKey)) {
                return null;
            }
            var characteristicId = parameters.Data[CharacteristicIdKey];
            return await service.GetCharacteristicAsync(Guid.Parse(characteristicId));
        }

        protected async Task<IDescriptor> GetDescriptorFromBundleAsync(IMvxBundle parameters) {
            var characteristic = await GetCharacteristicFromBundleAsync(parameters);
            if (characteristic == null || !parameters.Data.ContainsKey(DescriptorIdKey)) {
                return null;
            }
            var descriptorId = parameters.Data[DescriptorIdKey];
            return await characteristic.GetDescriptorAsync(Guid.Parse(descriptorId));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////


        ////////////////////////////////////////////////////////////////////////////
        // ViewModelS3 Inventory Section ///////////////////////////////////////////

        public double getTempF(UInt16 temp, UInt64 CalCode)
        {
            return (getTemperature(temp, CalCode) * 1.8 + 32.0);
        }

        public double getTempC(UInt16 temp, UInt64 CalCode)
        {
            return getTemperature(temp, CalCode);
        }

        public double getTemperature(UInt16 temp, UInt64 CalCode)
        {
            int crc = (int)(CalCode >> 48) & 0xffff;
            int calCode1 = (int)(CalCode >> 36) & 0x0fff;
            int calTemp1 = (int)(CalCode >> 25) & 0x07ff;
            int calCode2 = (int)(CalCode >> 13) & 0x0fff;
            int calTemp2 = (int)(CalCode >> 2) & 0x7FF;
            int calVer = (int)(CalCode & 0x03);

            double fTemperature = temp;
            fTemperature = ((double)calTemp2 - (double)calTemp1) * (fTemperature - (double)calCode1);
            fTemperature /= ((double)(calCode2) - (double)calCode1);
            fTemperature += (double)calTemp1;
            fTemperature -= 800;
            fTemperature /= 10;
            return fTemperature;
        }

        public void VoltageEvent1(object sender, CSLibrary.Notification.VoltageEventArgs e) {}
        public void VoltageEvent2(object sender, CSLibrary.Notification.VoltageEventArgs e) {}
        public void VoltageEvent3(object sender, CSLibrary.Notification.VoltageEventArgs e) {}
        public void VoltageEvent4(object sender, CSLibrary.Notification.VoltageEventArgs e) {}

        public void SetConfigPower()
        {
            if (BleMvxApplication._reader1.rfid.GetAntennaPort()==1) {
                if (BleMvxApplication._config1.RFID_PowerSequencing_NumberofPower == 0) {
                    BleMvxApplication._reader1.rfid.SetPowerSequencing(0);
                    BleMvxApplication._reader1.rfid.SetPowerLevel(BleMvxApplication._config1.RFID_Antenna_Power[0]);
                }
                else BleMvxApplication._reader1.rfid.SetPowerSequencing(BleMvxApplication._config1.RFID_PowerSequencing_NumberofPower, BleMvxApplication._config1.RFID_PowerSequencing_Level, BleMvxApplication._config1.RFID_PowerSequencing_DWell);
            }
            else {
                for (uint cnt = BleMvxApplication._reader1.rfid.GetAntennaPort() - 1; cnt >= 0; cnt--) {
                    BleMvxApplication._reader1.rfid.SetPowerLevel(BleMvxApplication._config1.RFID_Antenna_Power[cnt], cnt);
                }
            }

            if (BleMvxApplication._reader2.rfid.GetAntennaPort()==1) {
                if (BleMvxApplication._config2.RFID_PowerSequencing_NumberofPower == 0) {
                    BleMvxApplication._reader2.rfid.SetPowerSequencing(0);
                    BleMvxApplication._reader2.rfid.SetPowerLevel(BleMvxApplication._config2.RFID_Antenna_Power[0]);
                }
                else BleMvxApplication._reader2.rfid.SetPowerSequencing(BleMvxApplication._config2.RFID_PowerSequencing_NumberofPower, BleMvxApplication._config2.RFID_PowerSequencing_Level, BleMvxApplication._config2.RFID_PowerSequencing_DWell);
            }
            else {
                for (uint cnt = BleMvxApplication._reader2.rfid.GetAntennaPort() - 1; cnt >= 0; cnt--) {
                    BleMvxApplication._reader2.rfid.SetPowerLevel(BleMvxApplication._config2.RFID_Antenna_Power[cnt], cnt);
                }
            }
        }

        public void SetPower(int index, int readerID)
        {
            switch (index)
            {
                case 2:
                    if (readerID==1)
                    { 
                        BleMvxApplication._reader1.rfid.SetPowerSequencing(0);
                        BleMvxApplication._reader1.rfid.SetPowerLevel(300); 
                    }
                    else if (readerID==2)
                    {
                        BleMvxApplication._reader2.rfid.SetPowerSequencing(0);
                        BleMvxApplication._reader2.rfid.SetPowerLevel(300);
                    }
                    else if (readerID==3)
                    {
                        BleMvxApplication._reader3.rfid.SetPowerSequencing(0);
                        BleMvxApplication._reader3.rfid.SetPowerLevel(300);
                    }
                    else if (readerID==4)
                    {
                        BleMvxApplication._reader4.rfid.SetPowerSequencing(0);
                        BleMvxApplication._reader4.rfid.SetPowerLevel(300);
                    }
                    break;
                case 4:
                    SetConfigPower();
                    break;
                default:
                    break;
            }
        }

        ////////////////////////////////////////////////////////////////////////////

    }
}