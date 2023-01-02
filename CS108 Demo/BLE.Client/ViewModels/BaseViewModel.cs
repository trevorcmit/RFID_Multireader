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





        // Moved from DeviceListViewModel to the BaseViewModel for all ViewModels to inherit
        public ObservableCollection<DeviceListItemViewModel> Devices {get; set;} = new ObservableCollection<DeviceListItemViewModel>();

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




    }
}