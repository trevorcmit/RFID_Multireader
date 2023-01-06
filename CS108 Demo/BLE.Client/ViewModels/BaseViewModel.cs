// Default BaseViewModel Imports
using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using Plugin.BLE.Abstractions.Contracts;

// New Imports for Bluetooth Autoconnect
using System.Collections.ObjectModel;
using System.ComponentModel;
using Acr.UserDialogs;



namespace BLE.Client.ViewModels {
    public class BaseViewModel : MvxViewModel {
        protected readonly IAdapter Adapter;
        protected const string DeviceIdKey = "DeviceIdNavigationKey";
        protected const string ServiceIdKey = "ServiceIdNavigationKey";
        protected const string CharacteristicIdKey = "CharacteristicIdNavigationKey";
        protected const string DescriptorIdKey = "DescriptorIdNavigationKey";

        // New Private _userDialogs for Bluetooth Autoconnect
        private readonly IUserDialogs _userDialogs;


        /////////////////////////////////////////////////////////////////
        ////////// Section for Marine Mountain Deployment 1/13 //////////
        /////////////////////////////////////////////////////////////////
        public string EPC_Prefix = "E282403E000207D6F977";  // Prefix for used S3 tag type

        public int Person;                                  // Person ID


        /////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////


        // Moved from DeviceListViewModel to the BaseViewModel for all ViewModels to inherit
        public ObservableCollection<DeviceListItemViewModel> Devices { get; set; } = new ObservableCollection<DeviceListItemViewModel>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected private Guid _ConnectionGuid;  // Global ConnectionGuid variable to reconnect in any window
        public Guid ConnectionGuid {
            get => _ConnectionGuid; 
            set { _ConnectionGuid = value; OnPropertyChanged("ConnectionGuid"); }
        }


        // public DeviceListItemViewModel _ConnectionDevice; // Global ConnectionDevice variable to reconnect in any window

        public IDevice ConnectionDevice { get; set; }
        // public DeviceListItemViewModel ConnectionDevice { get; set; }

        protected private string _ConnectionDeviceName;  // Global ConnectionGuid variable to reconnect in any window
        public virtual string ConnectionDeviceName {
            get => _ConnectionDeviceName; 
            set { _ConnectionDeviceName = value; OnPropertyChanged("ConnectionDeviceName"); }
        }






        public BaseViewModel(IAdapter adapter) { Adapter = adapter; }

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

        protected IMvxBundle Bundle { get; private set; }

        protected IDevice GetDeviceFromBundle(IMvxBundle parameters) {
            if (!parameters.Data.ContainsKey(DeviceIdKey)) return null;
            var deviceId = parameters.Data[DeviceIdKey];

            return Adapter.ConnectedDevices.FirstOrDefault(d => d.Id.ToString().Equals(deviceId));
        }

        protected Task<IService> GetServiceFromBundleAsync(IMvxBundle parameters) {

            var device = GetDeviceFromBundle(parameters);
            if (device == null || !parameters.Data.ContainsKey(ServiceIdKey)) { return Task.FromResult<IService>(null); }

            var serviceId = parameters.Data[ServiceIdKey];
            return device.GetServiceAsync(Guid.Parse(serviceId));
        }

        protected async Task<ICharacteristic> GetCharacteristicFromBundleAsync(IMvxBundle parameters) {
            var service = await GetServiceFromBundleAsync(parameters);
            if (service == null || !parameters.Data.ContainsKey(CharacteristicIdKey)) { return null; }
            var characteristicId = parameters.Data[CharacteristicIdKey];
            return await service.GetCharacteristicAsync(Guid.Parse(characteristicId));
        }

        protected async Task<IDescriptor> GetDescriptorFromBundleAsync(IMvxBundle parameters) {
            var characteristic = await GetCharacteristicFromBundleAsync(parameters);
            if (characteristic == null || !parameters.Data.ContainsKey(DescriptorIdKey)) { return null; }
            var descriptorId = parameters.Data[DescriptorIdKey];
            return await characteristic.GetDescriptorAsync(Guid.Parse(descriptorId));
        }





        //////////////////////////////////////////////
        ///////////// NEW GLOBAL METHODS /////////////
        //////////////////////////////////////////////

        //<summary>
        // This method is called from the DeviceListViewModel to connect to a device
        //</summary>
        public async void Connect(IDevice _device)
        {
            // Trace.Message("device name :" + _device.Name);
            string BLE_result = await BleMvxApplication._reader.ConnectAsync(Adapter, _device);
            // Trace.Message("load config");

            bool LoadSuccess = await BleMvxApplication.LoadConfig(_device.Id.ToString());
            BleMvxApplication._config.readerID = _device.Id.ToString();

            // ONLY FOR VISIBILITY
            _ConnectionDeviceName = "Connection Complete, " + BLE_result + ", " + LoadSuccess.ToString();
            RaisePropertyChanged(() => ConnectionDeviceName);
        }

        //////////////////////////////////////////////



        /////////////////////////////////////////////////////////
        // Moved from ViewModelRFMicroS3Inventory to declutter //
        /////////////////////////////////////////////////////////
        public async void InventorySetting() {
            switch (BleMvxApplication._config.RFID_FrequenceSwitch) {
                case 0:
                    BleMvxApplication._reader.rfid.SetHoppingChannels(BleMvxApplication._config.RFID_Region);
                    break;
                case 1:
                    BleMvxApplication._reader.rfid.SetFixedChannel(BleMvxApplication._config.RFID_Region, BleMvxApplication._config.RFID_FixedChannel);
                    break;
                case 2:
                    BleMvxApplication._reader.rfid.SetAgileChannels(BleMvxApplication._config.RFID_Region);
                    break;
            }
            BleMvxApplication._reader.rfid.Options.TagRanging.flags = CSLibrary.Constants.SelectFlags.ZERO;

            // Setting 1
            SetPower(BleMvxApplication._rfMicro_Power);

            // Setting 3
            BleMvxApplication._config.RFID_DynamicQParms.toggleTarget = (BleMvxApplication._rfMicro_Target == 2) ? 1U : 0U;
            BleMvxApplication._config.RFID_DynamicQParms.retryCount = 5; // for RFMicro special setting
            BleMvxApplication._reader.rfid.SetDynamicQParms(BleMvxApplication._config.RFID_DynamicQParms);
            BleMvxApplication._config.RFID_DynamicQParms.retryCount = 0; // reset to normal

            // Setting 4
            BleMvxApplication._config.RFID_FixedQParms.toggleTarget = (BleMvxApplication._rfMicro_Target == 2) ? 1U : 0U;
            BleMvxApplication._config.RFID_FixedQParms.retryCount = 5; // for RFMicro special setting
            BleMvxApplication._reader.rfid.SetFixedQParms(BleMvxApplication._config.RFID_FixedQParms);
            BleMvxApplication._config.RFID_FixedQParms.retryCount = 0; // reset to normal

            // Setting 2
            BleMvxApplication._reader.rfid.SetOperationMode(BleMvxApplication._config.RFID_OperationMode);
            BleMvxApplication._reader.rfid.SetTagGroup(CSLibrary.Constants.Selected.ASSERTED, BleMvxApplication._config.RFID_TagGroup.session, (BleMvxApplication._rfMicro_Target != 1) ? CSLibrary.Constants.SessionTarget.A : CSLibrary.Constants.SessionTarget.B);
            BleMvxApplication._reader.rfid.SetCurrentSingulationAlgorithm(BleMvxApplication._config.RFID_Algorithm);
            BleMvxApplication._reader.rfid.SetCurrentLinkProfile(BleMvxApplication._config.RFID_Profile);

            // Select RFMicro S3 filter
            {
                CSLibrary.Structures.SelectCriterion extraSlecetion = new CSLibrary.Structures.SelectCriterion();

                extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.ASLINVA_DSLINVB, 0);
                extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.TID, 0, 28, new byte[] { 0xe2, 0x82, 0x40, 0x30 });
                BleMvxApplication._reader.rfid.SetSelectCriteria(0, extraSlecetion);

                // Set OCRSSI Limit
                extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.NOTHING_DSLINVB, 0);
                extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0xd0, 8, new byte[] { (byte)(0x20 | BleMvxApplication._rfMicro_minOCRSSI) });
                BleMvxApplication._reader.rfid.SetSelectCriteria(1, extraSlecetion);

                extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.NOTHING_DSLINVB, 0);
                extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0xd0, 8, new byte[] { (byte)(BleMvxApplication._rfMicro_maxOCRSSI) });
                BleMvxApplication._reader.rfid.SetSelectCriteria(2, extraSlecetion);

                // Temperature and Sensor code
                extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.NOTHING_DSLINVB, 0);
                extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0xe0, 0, new byte[] { 0x00 });
                BleMvxApplication._reader.rfid.SetSelectCriteria(3, extraSlecetion);

                BleMvxApplication._reader.rfid.Options.TagRanging.flags |= CSLibrary.Constants.SelectFlags.SELECT;
            }

            // Multi bank inventory
            BleMvxApplication._reader.rfid.Options.TagRanging.multibanks = 2;
            BleMvxApplication._reader.rfid.Options.TagRanging.bank1 = CSLibrary.Constants.MemoryBank.BANK0;
            BleMvxApplication._reader.rfid.Options.TagRanging.offset1 = 12; // Address C
            BleMvxApplication._reader.rfid.Options.TagRanging.count1 = 3;
            BleMvxApplication._reader.rfid.Options.TagRanging.bank2 = CSLibrary.Constants.MemoryBank.USER;
            BleMvxApplication._reader.rfid.Options.TagRanging.offset2 = 8;
            BleMvxApplication._reader.rfid.Options.TagRanging.count2 = 4;
            BleMvxApplication._reader.rfid.Options.TagRanging.compactmode = false;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PRERANGING);
        }

        public void SetPower(int index)
        {
            switch (index) {
                case 0:
                    BleMvxApplication._reader.rfid.SetPowerSequencing(0);
                    BleMvxApplication._reader.rfid.SetPowerLevel(160);
                    break;
                case 1:
                    BleMvxApplication._reader.rfid.SetPowerSequencing(0);
                    BleMvxApplication._reader.rfid.SetPowerLevel(230);
                    break;
                // ####### ACTIVE CASE #######
                case 2:
                    BleMvxApplication._reader.rfid.SetPowerSequencing(0);
                    BleMvxApplication._reader.rfid.SetPowerLevel(300);
                    break;
                // ###########################
                case 4:
                    SetConfigPower();
                    break;
            }
        }

        public void SetConfigPower() 
        {
            if (BleMvxApplication._reader.rfid.GetAntennaPort() == 1) {
                if (BleMvxApplication._config.RFID_PowerSequencing_NumberofPower == 0) {
                    BleMvxApplication._reader.rfid.SetPowerSequencing(0);
                    BleMvxApplication._reader.rfid.SetPowerLevel(BleMvxApplication._config.RFID_Antenna_Power[0]);
                }
                else BleMvxApplication._reader.rfid.SetPowerSequencing(
                    BleMvxApplication._config.RFID_PowerSequencing_NumberofPower, 
                    BleMvxApplication._config.RFID_PowerSequencing_Level, 
                    BleMvxApplication._config.RFID_PowerSequencing_DWell
                );
            }
            else {
                for (uint cnt = BleMvxApplication._reader.rfid.GetAntennaPort() - 1; cnt >= 0; cnt--) {
                    BleMvxApplication._reader.rfid.SetPowerLevel(BleMvxApplication._config.RFID_Antenna_Power[cnt], cnt);
                }
            }
        }

        public double getTempF(UInt16 temp, UInt64 CalCode)
        {
            return (getTemperature(temp, CalCode) * 1.8 + 32.0);
        }

        public double getTempC(UInt16 temp, UInt64 CalCode)
        {
            return getTemperature(temp, CalCode);
        }

        public double getTemperature(UInt16 temp, UInt64 CalCode)
        { // VERIFIED FROM MAGNUS AXZON DOCUMENTATION
            int crc      = (int)(CalCode >> 48) & 0xffff;
            int calCode1 = (int)(CalCode >> 36) & 0x0fff;
            int calTemp1 = (int)(CalCode >> 25) & 0x07ff;
            int calCode2 = (int)(CalCode >> 13) & 0x0fff;
            int calTemp2 = (int)(CalCode >> 2) & 0x7FF;
            int calVer   = (int)(CalCode & 0x03);

            double fTemperature = temp;
            fTemperature  = ((double)calTemp2 - (double)calTemp1) * (fTemperature - (double)calCode1);
            fTemperature /= ((double)(calCode2) - (double)calCode1);
            fTemperature += (double)calTemp1;
            fTemperature -= 800;
            fTemperature /= 10;
            return fTemperature;
        }
        /////////////////////////////////////////////////////////

    }
}