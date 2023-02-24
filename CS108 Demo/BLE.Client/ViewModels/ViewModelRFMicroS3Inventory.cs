using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Xamarin;
using Xamarin.Forms;
using Xamarin.Essentials;

// using LiveChartsCore;
// using LiveChartsCore.Defaults;
// using LiveChartsCore.SkiaSharpView;
// using LiveChartsCore.SkiaSharpView.Painting;
// using LiveChartsCore.Drawing;
// using LiveChartsCore.Kernel;
// using LiveChartsCore.Kernel.Drawing;
// using LiveChartsCore.Kernel.Sketches;
// using LiveChartsCore.Measure;

// New Imports for Bluetooth Autoconnect
using Plugin.BLE.Abstractions.Extensions;
using System.Threading;



namespace BLE.Client.ViewModels {
    public class ViewModelRFMicroS3Inventory : BaseViewModel {
        public class RFMicroTagInfoViewModel : BindableBase {
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // CLASS UPDATES/ADDITIONS
            private string _TimeString;    // Time at which last tag was read
            public string TimeString { get { return this._TimeString; } set { this.SetProperty(ref this._TimeString, value); } }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            private string _EPC;            public string EPC { get { return this._EPC; } set { this.SetProperty(ref this._EPC, value); } }
            private string _sensorAvgValue; public string SensorAvgValue { get { return this._sensorAvgValue; } set { this.SetProperty(ref this._sensorAvgValue, value); } }
            public RFMicroTagInfoViewModel() {}    // Class constructor (constructs nothing)
        }

        private readonly IUserDialogs _userDialogs;

        #region -------------- RFID inventory -----------------

        public ICommand OnStartInventoryButtonCommand { protected set; get; }
        public ICommand OnClearButtonCommand { protected set; get; }
        public ICommand OnShareDataCommand { protected set; get; }

        private ObservableCollection<RFMicroTagInfoViewModel> _TagInfoList = new ObservableCollection<RFMicroTagInfoViewModel>();
        public ObservableCollection<RFMicroTagInfoViewModel> TagInfoList {get {return _TagInfoList;} set {SetProperty(ref _TagInfoList, value);}}

        private string _startInventoryButtonText = "Start Inventory"; public string startInventoryButtonText {get {return _startInventoryButtonText;}}

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////// For Saving Data / CSV exporting ///////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        List<string> tag_List = new List<string>();
        Dictionary<string, List<string>> tag_Time = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> tag_Data = new Dictionary<string, List<string>>();
        // private List<string> _epcs; public List<string> epcs { get => _epcs; set { _epcs = value; OnPropertyChanged("epcs"); } }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool _startInventory = true;

        ////////////////////////////////////////////////////
        ///////////// Variables for Duty Cycle /////////////
        ////////////////////////////////////////////////////
        private int _active_time; public int active_time { get => _active_time; set { _active_time = value; OnPropertyChanged("active_time"); } }
        private int _inactive_time; public int inactive_time { get => _inactive_time; set { _inactive_time = value; OnPropertyChanged("inactive_time"); } }
        public System.Timers.Timer activetimer = new System.Timers.Timer();
        public System.Timers.Timer downtimer = new System.Timers.Timer();
        ////////////////////////////////////////////////////

        // Save FilePicker.PickAsync() result for use in Autosave function
        public FileResult pick_result; 
  
        #endregion


        #region ------------- EPCs ----------------
        private string _Beanie1; public string Beanie1 { get => _Beanie1; set { _Beanie1 = value; OnPropertyChanged("Beanie1"); } }
        private string _Back1; public string Back1 { get => _Back1; set { _Back1 = value; OnPropertyChanged("Back1"); } }
        private string _BackNeck1; public string BackNeck1 { get => _BackNeck1; set { _BackNeck1 = value; OnPropertyChanged("BackNeck1"); } }
        private string _Chest1; public string Chest1 { get => _Chest1; set { _Chest1 = value; OnPropertyChanged("Chest1"); } }
        private string _LeftAb1; public string LeftAb1 { get => _LeftAb1; set { _LeftAb1 = value; OnPropertyChanged("LeftAb1"); } }
        private string _RightAb1; public string RightAb1 { get => _RightAb1; set { _RightAb1 = value; OnPropertyChanged("RightAb1"); } }
        private string _LeftUpArm1; public string LeftUpArm1 { get => _LeftUpArm1; set { _LeftUpArm1 = value; OnPropertyChanged("LeftUpArm1"); } }
        private string _RightUpArm1; public string RightUpArm1 { get => _RightUpArm1; set { _RightUpArm1 = value; OnPropertyChanged("RightUpArm1"); } }
        private string _LeftLowArm1; public string LeftLowArm1 { get => _LeftLowArm1; set { _LeftLowArm1 = value; OnPropertyChanged("LeftLowArm1"); } }
        private string _RightLowArm1; public string RightLowArm1 { get => _RightLowArm1; set { _RightLowArm1 = value; OnPropertyChanged("RightLowArm1"); } }
        private string _LeftHand1; public string LeftHand1 { get => _LeftHand1; set { _LeftHand1 = value; OnPropertyChanged("LeftHand1"); } }
        private string _RightHand1; public string RightHand1 { get => _RightHand1; set { _RightHand1 = value; OnPropertyChanged("RightHand1"); } }
        private string _Bala1; public string Bala1 { get => _Bala1; set { _Bala1 = value; OnPropertyChanged("Bala1"); } }

        private string _Beanie1_T; public string Beanie1_T { get => _Beanie1_T; set { _Beanie1_T = value; OnPropertyChanged("Beanie1_T"); } }
        private string _Back1_T; public string Back1_T { get => _Back1_T; set { _Back1_T = value; OnPropertyChanged("Back1_T"); } }
        private string _BackNeck1_T; public string BackNeck1_T { get => _BackNeck1_T; set { _BackNeck1_T = value; OnPropertyChanged("BackNeck1_T"); } }
        private string _Chest1_T; public string Chest1_T { get => _Chest1_T; set { _Chest1_T = value; OnPropertyChanged("Chest1_T"); } }
        private string _LeftAb1_T; public string LeftAb1_T { get => _LeftAb1_T; set { _LeftAb1_T = value; OnPropertyChanged("LeftAb1_T"); } }
        private string _RightAb1_T; public string RightAb1_T { get => _RightAb1_T; set { _RightAb1_T = value; OnPropertyChanged("RightAb1_T"); } }
        private string _LeftUpArm1_T; public string LeftUpArm1_T { get => _LeftUpArm1_T; set { _LeftUpArm1_T = value; OnPropertyChanged("LeftUpArm1_T"); } }
        private string _RightUpArm1_T; public string RightUpArm1_T { get => _RightUpArm1_T; set { _RightUpArm1_T = value; OnPropertyChanged("RightUpArm1_T"); } }
        private string _LeftLowArm1_T; public string LeftLowArm1_T { get => _LeftLowArm1_T; set { _LeftLowArm1_T = value; OnPropertyChanged("LeftLowArm1_T"); } }
        private string _RightLowArm1_T; public string RightLowArm1_T { get => _RightLowArm1_T; set { _RightLowArm1_T = value; OnPropertyChanged("RightLowArm1_T"); } }
        private string _LeftHand1_T; public string LeftHand1_T { get => _LeftHand1_T; set { _LeftHand1_T = value; OnPropertyChanged("LeftHand1_T"); } }
        private string _RightHand1_T; public string RightHand1_T { get => _RightHand1_T; set { _RightHand1_T = value; OnPropertyChanged("RightHand1_T"); } }
        private string _Bala1_T; public string Bala1_T { get => _Bala1_T; set { _Bala1_T = value; OnPropertyChanged("Bala1_T"); } }

        private string _Beanie2; public string Beanie2 { get => _Beanie2; set { _Beanie2 = value; OnPropertyChanged("Beanie2"); } }
        private string _Back2; public string Back2 { get => _Back2; set { _Back2 = value; OnPropertyChanged("Back2"); } }
        private string _BackNeck2; public string BackNeck2 { get => _BackNeck2; set { _BackNeck2 = value; OnPropertyChanged("BackNeck2"); } }
        private string _Chest2; public string Chest2 { get => _Chest2; set { _Chest2 = value; OnPropertyChanged("Chest2"); } }
        private string _LeftAb2; public string LeftAb2 { get => _LeftAb2; set { _LeftAb2 = value; OnPropertyChanged("LeftAb2"); } }
        private string _RightAb2; public string RightAb2 { get => _RightAb2; set { _RightAb2 = value; OnPropertyChanged("RightAb2"); } }
        private string _LeftUpArm2; public string LeftUpArm2 { get => _LeftUpArm2; set { _LeftUpArm2 = value; OnPropertyChanged("LeftUpArm2"); } }
        private string _RightUpArm2; public string RightUpArm2 { get => _RightUpArm2; set { _RightUpArm2 = value; OnPropertyChanged("RightUpArm2"); } }
        private string _LeftLowArm2; public string LeftLowArm2 { get => _LeftLowArm2; set { _LeftLowArm2 = value; OnPropertyChanged("LeftLowArm2"); } }
        private string _RightLowArm2; public string RightLowArm2 { get => _RightLowArm2; set { _RightLowArm2 = value; OnPropertyChanged("RightLowArm2"); } }
        private string _LeftHand2; public string LeftHand2 { get => _LeftHand2; set { _LeftHand2 = value; OnPropertyChanged("LeftHand2"); } }
        private string _RightHand2; public string RightHand2 { get => _RightHand2; set { _RightHand2 = value; OnPropertyChanged("RightHand2"); } }
        private string _Bala2; public string Bala2 { get => _Bala2; set { _Bala2 = value; OnPropertyChanged("Bala2"); } }

        private string _Beanie2_T; public string Beanie2_T { get => _Beanie2_T; set { _Beanie2_T = value; OnPropertyChanged("Beanie2_T"); } }
        private string _Back2_T; public string Back2_T { get => _Back2_T; set { _Back2_T = value; OnPropertyChanged("Back2_T"); } }
        private string _BackNeck2_T; public string BackNeck2_T { get => _BackNeck2_T; set { _BackNeck2_T = value; OnPropertyChanged("BackNeck2_T"); } }
        private string _Chest2_T; public string Chest2_T { get => _Chest2_T; set { _Chest2_T = value; OnPropertyChanged("Chest2_T"); } }
        private string _LeftAb2_T; public string LeftAb2_T { get => _LeftAb2_T; set { _LeftAb2_T = value; OnPropertyChanged("LeftAb2_T"); } }
        private string _RightAb2_T; public string RightAb2_T { get => _RightAb2_T; set { _RightAb2_T = value; OnPropertyChanged("RightAb2_T"); } }
        private string _LeftUpArm2_T; public string LeftUpArm2_T { get => _LeftUpArm2_T; set { _LeftUpArm2_T = value; OnPropertyChanged("LeftUpArm2_T"); } }
        private string _RightUpArm2_T; public string RightUpArm2_T { get => _RightUpArm2_T; set { _RightUpArm2_T = value; OnPropertyChanged("RightUpArm2_T"); } }
        private string _LeftLowArm2_T; public string LeftLowArm2_T { get => _LeftLowArm2_T; set { _LeftLowArm2_T = value; OnPropertyChanged("LeftLowArm2_T"); } }
        private string _RightLowArm2_T; public string RightLowArm2_T { get => _RightLowArm2_T; set { _RightLowArm2_T = value; OnPropertyChanged("RightLowArm2_T"); } }
        private string _LeftHand2_T; public string LeftHand2_T { get => _LeftHand2_T; set { _LeftHand2_T = value; OnPropertyChanged("LeftHand2_T"); } }
        private string _RightHand2_T; public string RightHand2_T { get => _RightHand2_T; set { _RightHand2_T = value; OnPropertyChanged("RightHand2_T"); } }
        private string _Bala2_T; public string Bala2_T { get => _Bala2_T; set { _Bala2_T = value; OnPropertyChanged("Bala2_T"); } }

        private string _Beanie3; public string Beanie3 { get => _Beanie3; set { _Beanie3 = value; OnPropertyChanged("Beanie3"); } }
        private string _Back3; public string Back3 { get => _Back3; set { _Back3 = value; OnPropertyChanged("Back3"); } }
        private string _BackNeck3; public string BackNeck3 { get => _BackNeck3; set { _BackNeck3 = value; OnPropertyChanged("BackNeck3"); } }
        private string _Chest3; public string Chest3 { get => _Chest3; set { _Chest3 = value; OnPropertyChanged("Chest3"); } }
        private string _LeftAb3; public string LeftAb3 { get => _LeftAb3; set { _LeftAb3 = value; OnPropertyChanged("LeftAb3"); } }
        private string _RightAb3; public string RightAb3 { get => _RightAb3; set { _RightAb3 = value; OnPropertyChanged("RightAb3"); } }
        private string _LeftUpArm3; public string LeftUpArm3 { get => _LeftUpArm3; set { _LeftUpArm3 = value; OnPropertyChanged("LeftUpArm3"); } }
        private string _RightUpArm3; public string RightUpArm3 { get => _RightUpArm3; set { _RightUpArm3 = value; OnPropertyChanged("RightUpArm3"); } }
        private string _LeftLowArm3; public string LeftLowArm3 { get => _LeftLowArm3; set { _LeftLowArm3 = value; OnPropertyChanged("LeftLowArm3"); } }
        private string _RightLowArm3; public string RightLowArm3 { get => _RightLowArm3; set { _RightLowArm3 = value; OnPropertyChanged("RightLowArm3"); } }
        private string _LeftHand3; public string LeftHand3 { get => _LeftHand3; set { _LeftHand3 = value; OnPropertyChanged("LeftHand3"); } }
        private string _RightHand3; public string RightHand3 { get => _RightHand3; set { _RightHand3 = value; OnPropertyChanged("RightHand3"); } }
        private string _Bala3; public string Bala3 { get => _Bala3; set { _Bala3 = value; OnPropertyChanged("Bala3"); } }

        private string _Beanie3_T; public string Beanie3_T { get => _Beanie3_T; set { _Beanie3_T = value; OnPropertyChanged("Beanie3_T"); } }
        private string _Back3_T; public string Back3_T { get => _Back3_T; set { _Back3_T = value; OnPropertyChanged("Back3_T"); } }
        private string _BackNeck3_T; public string BackNeck3_T { get => _BackNeck3_T; set { _BackNeck3_T = value; OnPropertyChanged("BackNeck3_T"); } }
        private string _Chest3_T; public string Chest3_T { get => _Chest3_T; set { _Chest3_T = value; OnPropertyChanged("Chest3_T"); } }
        private string _LeftAb3_T; public string LeftAb3_T { get => _LeftAb3_T; set { _LeftAb3_T = value; OnPropertyChanged("LeftAb3_T"); } }
        private string _RightAb3_T; public string RightAb3_T { get => _RightAb3_T; set { _RightAb3_T = value; OnPropertyChanged("RightAb3_T"); } }
        private string _LeftUpArm3_T; public string LeftUpArm3_T { get => _LeftUpArm3_T; set { _LeftUpArm3_T = value; OnPropertyChanged("LeftUpArm3_T"); } }
        private string _RightUpArm3_T; public string RightUpArm3_T { get => _RightUpArm3_T; set { _RightUpArm3_T = value; OnPropertyChanged("RightUpArm3_T"); } }
        private string _LeftLowArm3_T; public string LeftLowArm3_T { get => _LeftLowArm3_T; set { _LeftLowArm3_T = value; OnPropertyChanged("LeftLowArm3_T"); } }
        private string _RightLowArm3_T; public string RightLowArm3_T { get => _RightLowArm3_T; set { _RightLowArm3_T = value; OnPropertyChanged("RightLowArm3_T"); } }
        private string _LeftHand3_T; public string LeftHand3_T { get => _LeftHand3_T; set { _LeftHand3_T = value; OnPropertyChanged("LeftHand3_T"); } }
        private string _RightHand3_T; public string RightHand3_T { get => _RightHand3_T; set { _RightHand3_T = value; OnPropertyChanged("RightHand3_T"); } }
        private string _Bala3_T; public string Bala3_T { get => _Bala3_T; set { _Bala3_T = value; OnPropertyChanged("Bala3_T"); } }

        public int THRESHOLD = 15;
        
        // class Shirt {
        //     public string Back          { get; set; }
        //     public string BackNeck      { get; set; }
        //     public string Chest         { get; set; }
        //     public string LeftAb        { get; set; }
        //     public string RightAb       { get; set; }
        //     public string LeftUpArm     { get; set; }
        //     public string RightUpArm    { get; set; }
        //     public string LeftLowArm    { get; set; }
        //     public string RightLowArm   { get; set; }
        //     public List<string> TagList { get; set; }

        //     public Shirt(
        //         string backneck, string back, string chest, string leftab, string rightab, 
        //         string rightuparm, string rightlowarm, string leftuparm, string leftlowarm
        //     ) {
        //         // Shirt Locations
        //         BackNeck    = backneck;
        //         Back        = back;
        //         Chest       = chest;
        //         LeftAb      = leftab;
        //         RightAb     = rightab;
        //         LeftUpArm   = leftuparm;
        //         RightUpArm  = rightuparm;
        //         LeftLowArm  = leftlowarm;
        //         RightLowArm = rightlowarm;
        //         TagList = new List<string> { Back, BackNeck, Chest, LeftAb, RightAb, LeftUpArm, RightUpArm, LeftLowArm, RightLowArm };
        //     }
        // }

        class Person {
            public KeyValuePair< string, double? > BackInner { get; set; }
            public KeyValuePair< string, double? > BackOuter { get; set; }
            public KeyValuePair< string, double? > BackNeckInner { get; set; }
            public KeyValuePair< string, double? > BackNeckOuter { get; set; }
            public KeyValuePair< string, double? > ChestInner { get; set; }
            public KeyValuePair< string, double? > ChestOuter { get; set; }
            public KeyValuePair< string, double? > LeftAbInner { get; set; }
            public KeyValuePair< string, double? > LeftAbOuter { get; set; }
            public KeyValuePair< string, double? > RightAbInner { get; set; }
            public KeyValuePair< string, double? > RightAbOuter { get; set; }
            public KeyValuePair< string, double? > LeftUpArmInner { get; set; }
            public KeyValuePair< string, double? > LeftUpArmOuter { get; set; }
            public KeyValuePair< string, double? > RightUpArmInner { get; set; }
            public KeyValuePair< string, double? > RightUpArmOuter { get; set; }
            public KeyValuePair< string, double? > LeftLowArmInner { get; set; }
            public KeyValuePair< string, double? > LeftLowArmOuter { get; set; }
            public KeyValuePair< string, double? > RightLowArmInner { get; set; }
            public KeyValuePair< string, double? > RightLowArmOuter { get; set; }

            public Person (
                string bn1, string b1, string c1, string lab1, string rab1,
                string rua1, string rla1, string lua1, string lla1,
                string bn2, string b2, string c2, string lab2, string rab2,
                string rua2, string rla2, string lua2, string lla2
            ) {
                
                BackInner = new KeyValuePair<string, double?>(b1, null);
                BackOuter = new KeyValuePair<string, double?>(b2, null);
                BackNeckInner = new KeyValuePair<string, double?>(bn1, null);
                BackNeckOuter = new KeyValuePair<string, double?>(bn2, null);
                ChestInner = new KeyValuePair<string, double?>(c1, null);
                ChestOuter = new KeyValuePair<string, double?>(c2, null);
                LeftAbInner = new KeyValuePair<string, double?>(lab1, null);
                LeftAbOuter = new KeyValuePair<string, double?>(lab2, null);
                RightAbInner = new KeyValuePair<string, double?>(rab1, null);
                RightAbOuter = new KeyValuePair<string, double?>(rab2, null);
                LeftUpArmInner = new KeyValuePair<string, double?>(lua1, null);
                LeftUpArmOuter = new KeyValuePair<string, double?>(lua2, null);
                RightUpArmInner = new KeyValuePair<string, double?>(rua1, null);
                RightUpArmOuter = new KeyValuePair<string, double?>(rua2, null);
                LeftLowArmInner = new KeyValuePair<string, double?>(lla1, null);
                LeftLowArmOuter = new KeyValuePair<string, double?>(lla2, null);
                RightLowArmInner = new KeyValuePair<string, double?>(rla1, null);
                RightLowArmOuter = new KeyValuePair<string, double?>(rla2, null);

            }
        }

        Person p = new Person(
            "777F", "67DB", "184A", "885D", "71CF", "BA4C", "8FA9", "B6A1", "2C97",
            "9854", "A3B0", "9EC6", "9A91", "343B", "87D4", "81D4", "8A53", "1397"
        );

        List<string> epcs = new List<string>{
            "777F", "67DB", "184A", "885D", "71CF", "BA4C", "8FA9", "B6A1", "2C97",
            "9854", "A3B0", "9EC6", "9A91", "343B", "87D4", "81D4", "8A53", "1397"
        };

        #endregion



        public ViewModelRFMicroS3Inventory(IAdapter adapter, IUserDialogs userDialogs) : base(adapter) {
            _userDialogs = userDialogs;

            Back1        = "gray"; Back2        = "gray"; Back3        = "gray"; Back1_T        = "--"; Back2_T        = "--"; Back3_T        = "--";
            BackNeck1    = "gray"; BackNeck2    = "gray"; BackNeck3    = "gray"; BackNeck1_T    = "--"; BackNeck2_T    = "--"; BackNeck3_T    = "--";
            Chest1       = "gray"; Chest2       = "gray"; Chest3       = "gray"; Chest1_T       = "--"; Chest2_T       = "--"; Chest3_T       = "--";
            LeftAb1      = "gray"; LeftAb2      = "gray"; LeftAb3      = "gray"; LeftAb1_T      = "--"; LeftAb2_T      = "--"; LeftAb3_T      = "--";
            RightAb1     = "gray"; RightAb2     = "gray"; RightAb3     = "gray"; RightAb1_T     = "--"; RightAb2_T     = "--"; RightAb3_T     = "--";
            LeftUpArm1   = "gray"; LeftUpArm2   = "gray"; LeftUpArm3   = "gray"; LeftUpArm1_T   = "--"; LeftUpArm2_T   = "--"; LeftUpArm3_T   = "--";
            RightUpArm1  = "gray"; RightUpArm2  = "gray"; RightUpArm3  = "gray"; RightUpArm1_T  = "--"; RightUpArm2_T  = "--"; RightUpArm3_T  = "--";
            LeftLowArm1  = "gray"; LeftLowArm2  = "gray"; LeftLowArm3  = "gray"; LeftLowArm1_T  = "--"; LeftLowArm2_T  = "--"; LeftLowArm3_T  = "--";
            RightLowArm1 = "gray"; RightLowArm2 = "gray"; RightLowArm3 = "gray"; RightLowArm1_T = "--"; RightLowArm2_T = "--"; RightLowArm3_T = "--";
            Beanie1      = "gray"; Beanie2      = "gray"; Beanie3      = "gray"; Beanie1_T      = "--"; Beanie2_T      = "--"; Beanie3_T      = "--";
            LeftHand1    = "gray"; LeftHand2    = "gray"; LeftHand3    = "gray"; LeftHand1_T    = "--"; LeftHand2_T    = "--"; LeftHand3_T    = "--";
            RightHand1   = "gray"; RightHand2   = "gray"; RightHand3   = "gray"; RightHand1_T   = "--"; RightHand2_T   = "--"; RightHand3_T   = "--";
            Bala1        = "gray"; Bala2        = "gray"; Bala3        = "gray"; Bala1_T        = "--"; Bala2_T        = "--"; Bala3_T        = "--";

            // Set disconnection event for reconnection
            Adapter.DeviceDisconnected += OnDeviceDisconnected;
            Adapter.DeviceConnectionLost += OnDeviceDisconnected; // connection or discconnect?

            GetTimes();      // Get Duty Cycle Times

            OnStartInventoryButtonCommand = new Command(StartInventoryClick);
            OnClearButtonCommand = new Command(ClearClick);
            OnShareDataCommand = new Command(ShareDataButtonClick);
        }

        // Event for Device Disconnection
        private async void OnDeviceDisconnected(object sender, DeviceEventArgs e) {
            Devices.FirstOrDefault(d => d.Id == e.Device.Id)?.Update();
            _userDialogs.HideLoading();
            _userDialogs.Toast($"Disconnected {e.Device.Name}");

            // ATTEMPTING TO SWITCH TO DISCONNECT CASE
            await BleMvxApplication._reader.DisconnectAsync();

            ////////////////////////////////////////////////////////
            ///////// ConnectToPreviousDeviceAsync Section /////////

            IDevice device;
            try {
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                ConnectParameters connectParameters = new ConnectParameters(true, false);

                var config = new ProgressDialogConfig() {
                    Title = $"Searching for '{PreviousGuid}'",
                    CancelText = "Cancel",
                    IsDeterministic = false,
                    OnCancel = tokenSource.Cancel
                };

                using (var progress = _userDialogs.Progress(config)) {
                    progress.Show();
                    device = await Adapter.ConnectToKnownDeviceAsync(PreviousGuid, connectParameters, tokenSource.Token);
                }

                // var deviceItem = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
            }

            catch (Exception ex) {
                _userDialogs.ShowError(ex.Message, 5000);
                return;
            }

            ////////////////////////////////////////////////////////

        }

        ~ViewModelRFMicroS3Inventory() {}

        public override void Resume() {
            base.Resume();

            // RFID event handler
            BleMvxApplication._reader.rfid.OnAsyncCallback += new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);

            // Key Button event handler
            BleMvxApplication._reader.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);
            BleMvxApplication._reader.notification.OnVoltageEvent += new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent);

            InventorySetting();
        }

        public override void Suspend() {
            BleMvxApplication._reader.rfid.CancelAllSelectCriteria(); // Confirm cancel all filter
            BleMvxApplication._reader.rfid.StopOperation();
            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
            BleMvxApplication._reader.barcode.Stop();

            // Cancel RFID event handler
            BleMvxApplication._reader.rfid.OnAsyncCallback -= new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
            BleMvxApplication._reader.rfid.OnStateChanged += new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent);

            // Key Button event handler
            BleMvxApplication._reader.notification.OnKeyEvent -= new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);
            BleMvxApplication._reader.notification.OnVoltageEvent -= new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent);

            base.Suspend();
        }

        protected override void InitFromBundle(IMvxBundle parameters) { base.InitFromBundle(parameters); }

        private void ClearClick() {
            InvokeOnMainThread(() => {
                lock (TagInfoList) { TagInfoList.Clear(); }
                tag_Data.Clear();
                tag_Time.Clear();
                tag_List.Clear();
            });
        }

        public RFMicroTagInfoViewModel objItemSelected { get; set; }

        void StartInventory() {
            if (_startInventory == false) return;

            SetPower(BleMvxApplication._rfMicro_Power);
            {
                _startInventory = false;
                _startInventoryButtonText = "Refresh Inventory";
            }

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_EXERANGING);
            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.INVENTORY);

            RaisePropertyChanged(() => startInventoryButtonText);
        }

        void StopInventory() {
            _startInventory = true;
            _startInventoryButtonText = "Start Inventory";

            BleMvxApplication._reader.rfid.StopOperation();
            RaisePropertyChanged(() => startInventoryButtonText);
        }

        void StartInventoryClick() {
            if (_startInventory) {
                activetimer.Enabled = true; 
                StartInventory(); 
            }
            else {
                StopInventory();
                activetimer.Enabled = false;
                downtimer.Enabled = false; 
            }
        }



        //////////////////////////////////////////////////////////////////
        //////////////// Timer Function and Event Section ////////////////
        //////////////////////////////////////////////////////////////////

        async void GetTimes() {
            // Necessary part for picking autosave location
            pick_result = await FilePicker.PickAsync();

            // Save every second and we cycle by half seconds
            _active_time   = 1000;
            _inactive_time = 1000;

            RaisePropertyChanged(() => active_time);
            RaisePropertyChanged(() => inactive_time);

            ActiveTimer();
            DownTimer();
        }

        private void ActiveTimer() {  
            activetimer.Interval = inactive_time;       // READER IS OFF FOR THIS DURATION
            activetimer.Elapsed += ActiveEvent;  
            activetimer.Enabled = false;
        }

        private void DownTimer() {
            downtimer.Interval = active_time;           // READER IS ACTIVE FOR THIS LONG
            downtimer.Elapsed += DownEvent;
            downtimer.Enabled = false;
        }

        private void ActiveEvent(object sender, System.Timers.ElapsedEventArgs e) {  
            activetimer.Enabled = false;
            downtimer.Enabled = true;
            // StartInventory();
        }

        private void DownEvent(object sender, System.Timers.ElapsedEventArgs e) {
            // StopInventory();
            AutoSaveData();    // Autosave while Down is occurring
            activetimer.Enabled = true;
            downtimer.Enabled = false;
        }

        //////////////////////////////////////////////////////////////////



        void TagInventoryEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e) {
            if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING) return;
            if (e.info.Bank1Data == null || e.info.Bank2Data == null)   return;
            InvokeOnMainThread(() => { AddOrUpdateTagData(e.info); });
        }

        void StateChangedEvent(object sender, CSLibrary.Events.OnStateChangedEventArgs e) {
            switch (e.state) {
                case CSLibrary.Constants.RFState.IDLE:
                    ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
                    switch (BleMvxApplication._reader.rfid.LastMacErrorCode) {
                        case 0x00: // Normal End
                            break;
                        case 0x0309:
                            _userDialogs.Alert("Too near to metal, please move CS108 away from metal and start inventory again.");
                            break;
                        default:
                            _userDialogs.Alert("Mac error : 0x" + BleMvxApplication._reader.rfid.LastMacErrorCode.ToString("X4"));
                            break;
                    }
                    break;
            }
        }

        private void AddOrUpdateTagData(CSLibrary.Structures.TagCallbackInfo info) {
            InvokeOnMainThread(() => {
                bool found = false;
                int cnt;

                lock (TagInfoList) {
                    UInt16 sensorCode = (UInt16)(info.Bank1Data[0] & 0x1ff);   // Address c
                    UInt16 ocRSSI     = info.Bank1Data[1];                     // Address d
                    UInt16 temp       = info.Bank1Data[2];                     // Address e

                    for (cnt=0; cnt<TagInfoList.Count; cnt++) {

                        if (TagInfoList[cnt].EPC==info.epc.ToString()) {
                        // if (epcs.Contains(info.epc.ToString()) && (TagInfoList[cnt].EPC == info.epc.ToString())) {

                            if (ocRSSI >= BleMvxApplication._rfMicro_minOCRSSI && ocRSSI <= BleMvxApplication._rfMicro_maxOCRSSI) {
                                if (temp >= 1300 && temp <= 3500) {
                                    UInt64 caldata = (UInt64)(((UInt64)info.Bank2Data[0]<<48) | ((UInt64)info.Bank2Data[1]<<32) | ((UInt64)info.Bank2Data[2]<<16) | ((UInt64)info.Bank2Data[3]));

                                    if (caldata == 0) { TagInfoList[cnt].SensorAvgValue = "NoCalData"; }
                                    else {
                                        double SAV = Math.Round(getTempC(temp, caldata), 2);   
                                        string DisplaySAV = Math.Round(SAV, 1).ToString();
                                        TagInfoList[cnt].SensorAvgValue = SAV.ToString();
                                        TagInfoList[cnt].TimeString = DateTime.Now.ToString("HH:mm:ss");

                                        try {
                                            if (!tag_List.Contains(TagInfoList[cnt].EPC)) {      // Check Tag_List contains tags, add new data
                                                tag_List.Add(TagInfoList[cnt].EPC);
                                            }

                                            if (!tag_Time.ContainsKey(TagInfoList[cnt].EPC)) {   // Check Tag_Time contains tags, add new data
                                                List<string> t_time = new List<string>{TagInfoList[cnt].TimeString};
                                                tag_Time.Add(TagInfoList[cnt].EPC, t_time);
                                            }
                                            else {
                                                tag_Time[TagInfoList[cnt].EPC].Add(TagInfoList[cnt].TimeString);
                                            }

                                            if (!tag_Data.ContainsKey(TagInfoList[cnt].EPC)) {   // Check Tag_Data contains tags, add new data
                                                List<string> t_data = new List<string>{TagInfoList[cnt].SensorAvgValue};
                                                tag_Data.Add(TagInfoList[cnt].EPC, t_data);
                                            }
                                            else {
                                                tag_Data[TagInfoList[cnt].EPC].Add(TagInfoList[cnt].SensorAvgValue);
                                            }
                                        }

                                        finally {
                                            // Get Last Four Characters of EPC
                                            string tEPC = TagInfoList[cnt].EPC.Substring(TagInfoList[cnt].EPC.Length - 4);

                                            KeyValuePair<string, double?> newkvp = new KeyValuePair<string, double?>(tEPC, SAV);

                                            if (tEPC==p.BackInner.Key) {
                                                p.BackInner = newkvp;
                                                if (!(p.BackInner.Value is null) && !(p.BackOuter.Value is null)) {
                                                    _Back1_T = (p.BackInner.Value - p.BackOuter.Value).ToString();
                                                    RaisePropertyChanged(() => Back1_T);
                                                }
                                            }
                                            else if (tEPC==p.BackOuter.Key) {
                                                p.BackOuter = newkvp;
                                                if (!(p.BackInner.Value is null) && !(p.BackOuter.Value is null)) {
                                                    _Back1_T = (p.BackInner.Value - p.BackOuter.Value).ToString();
                                                    RaisePropertyChanged(() => Back1_T);
                                                }
                                            }
                                            else if (tEPC==p.BackNeckInner.Key) {
                                                p.BackNeckInner = newkvp;
                                                if (!(p.BackNeckInner.Value is null) && !(p.BackNeckOuter.Value is null)) {
                                                    _BackNeck1_T = (p.BackNeckInner.Value - p.BackNeckOuter.Value).ToString();
                                                    RaisePropertyChanged(() => BackNeck1_T);
                                                }
                                            }
                                            else if (tEPC==p.BackNeckOuter.Key) {
                                                p.BackNeckOuter = newkvp;
                                                if (!(p.BackNeckInner.Value is null) && !(p.BackNeckOuter.Value is null)) {
                                                    _BackNeck1_T = (p.BackNeckInner.Value - p.BackNeckOuter.Value).ToString();
                                                    RaisePropertyChanged(() => BackNeck1_T);
                                                }
                                            }
                                            else if (tEPC==p.ChestInner.Key) {
                                                p.ChestInner = newkvp;
                                                if (!(p.ChestInner.Value is null) && !(p.ChestOuter.Value is null)) {
                                                    _Chest1_T = (p.ChestInner.Value - p.ChestOuter.Value).ToString();
                                                    RaisePropertyChanged(() => Chest1_T);
                                                }
                                            }
                                            else if (tEPC==p.ChestOuter.Key) {
                                                p.ChestOuter = newkvp;
                                                if (!(p.ChestInner.Value is null) && !(p.ChestOuter.Value is null)) {
                                                    _Chest1_T = (p.ChestInner.Value - p.ChestOuter.Value).ToString();
                                                    RaisePropertyChanged(() => Chest1_T);
                                                }
                                            }
                                            else if (tEPC==p.LeftAbInner.Key) {
                                                p.LeftAbInner = newkvp;
                                                if (!(p.LeftAbInner.Value is null) && !(p.LeftAbOuter.Value is null)) {
                                                    _LeftAb1_T = (p.LeftAbInner.Value - p.LeftAbOuter.Value).ToString();
                                                    RaisePropertyChanged(() => LeftAb1_T);
                                                }
                                            }
                                            else if (tEPC==p.LeftAbOuter.Key) {
                                                p.LeftAbOuter = newkvp;
                                                if (!(p.LeftAbInner.Value is null) && !(p.LeftAbOuter.Value is null)) {
                                                    _LeftAb1_T = (p.LeftAbInner.Value - p.LeftAbOuter.Value).ToString();
                                                    RaisePropertyChanged(() => LeftAb1_T);
                                                }
                                            }
                                            else if (tEPC==p.RightAbInner.Key) {
                                                p.RightAbInner = newkvp;
                                                if (!(p.RightAbInner.Value is null) && !(p.RightAbOuter.Value is null)) {
                                                    _RightAb1_T = (p.RightAbInner.Value - p.RightAbOuter.Value).ToString();
                                                    RaisePropertyChanged(() => RightAb1_T);
                                                }
                                            }
                                            else if (tEPC==p.RightAbOuter.Key) {
                                                p.RightAbOuter = newkvp;
                                                if (!(p.RightAbInner.Value is null) && !(p.RightAbOuter.Value is null)) {
                                                    _RightAb1_T = (p.RightAbInner.Value - p.RightAbOuter.Value).ToString();
                                                    RaisePropertyChanged(() => RightAb1_T);
                                                }
                                            }
                                            else if (tEPC==p.LeftUpArmInner.Key) {
                                                p.LeftUpArmInner = newkvp;
                                                if (!(p.LeftUpArmInner.Value is null) && !(p.LeftUpArmOuter.Value is null)) {
                                                    _LeftUpArm1_T = (p.LeftUpArmInner.Value - p.LeftUpArmOuter.Value).ToString();
                                                    RaisePropertyChanged(() => LeftUpArm1_T);
                                                }
                                            }
                                            else if (tEPC==p.LeftUpArmOuter.Key) {
                                                p.LeftUpArmOuter = newkvp;
                                                if (!(p.LeftUpArmInner.Value is null) && !(p.LeftUpArmOuter.Value is null)) {
                                                    _LeftUpArm1_T = (p.LeftUpArmInner.Value - p.LeftUpArmOuter.Value).ToString();
                                                    RaisePropertyChanged(() => LeftUpArm1_T);
                                                }
                                            }
                                            else if (tEPC==p.RightUpArmInner.Key) {
                                                p.RightUpArmInner = newkvp;
                                                if (!(p.RightUpArmInner.Value is null) && !(p.RightUpArmOuter.Value is null)) {
                                                    _RightUpArm1_T = (p.RightUpArmInner.Value - p.RightUpArmOuter.Value).ToString();
                                                    RaisePropertyChanged(() => RightUpArm1_T);
                                                }
                                            }
                                            else if (tEPC==p.RightUpArmOuter.Key) {
                                                p.RightUpArmOuter = newkvp;
                                                if (!(p.RightUpArmInner.Value is null) && !(p.RightUpArmOuter.Value is null)) {
                                                    _RightUpArm1_T = (p.RightUpArmInner.Value - p.RightUpArmOuter.Value).ToString();
                                                    RaisePropertyChanged(() => RightUpArm1_T);
                                                }
                                            }
                                            else if (tEPC==p.LeftLowArmInner.Key) {
                                                p.LeftLowArmInner = newkvp;
                                                if (!(p.LeftLowArmInner.Value is null) && !(p.LeftLowArmOuter.Value is null)) {
                                                    _LeftLowArm1_T = (p.LeftLowArmInner.Value - p.LeftLowArmOuter.Value).ToString();
                                                    RaisePropertyChanged(() => LeftLowArm1_T);
                                                }
                                            }
                                            else if (tEPC==p.LeftLowArmOuter.Key) {
                                                p.LeftLowArmOuter = newkvp;
                                                if (!(p.LeftLowArmInner.Value is null) && !(p.LeftLowArmOuter.Value is null)) {
                                                    _LeftLowArm1_T = (p.LeftLowArmInner.Value - p.LeftLowArmOuter.Value).ToString();
                                                    RaisePropertyChanged(() => LeftLowArm1_T);
                                                }
                                            }
                                            else if (tEPC==p.RightLowArmInner.Key) {
                                                p.RightLowArmInner = newkvp;
                                                if (!(p.RightLowArmInner.Value is null) && !(p.RightLowArmOuter.Value is null)) {
                                                    _RightLowArm1_T = (p.RightLowArmInner.Value - p.RightLowArmOuter.Value).ToString();
                                                    RaisePropertyChanged(() => RightLowArm1_T);
                                                }
                                            }
                                            else if (tEPC==p.RightLowArmOuter.Key) {
                                                p.RightLowArmOuter = newkvp;
                                                if (!(p.RightLowArmInner.Value is null) && !(p.RightLowArmOuter.Value is null)) {
                                                    _RightLowArm1_T = (p.RightLowArmInner.Value - p.RightLowArmOuter.Value).ToString();
                                                    RaisePropertyChanged(() => RightLowArm1_T);
                                                }
                                            }
                                        }

                                    } // if caldata is nonzero
                                }     // if temp within range
                            }
                            else {}
                            found = true;
                            break;
                        }
                    }

                    if (!found) {

                        // if (epcs.Contains(info.epc.ToString())) {

                        RFMicroTagInfoViewModel item = new RFMicroTagInfoViewModel();
                        item.EPC = info.epc.ToString();
                        item.SensorAvgValue = "";

                        if (ocRSSI >= BleMvxApplication._rfMicro_minOCRSSI && ocRSSI <= BleMvxApplication._rfMicro_maxOCRSSI) {
                            if (temp>=1300 && temp<=3500) {
                                UInt64 caldata = (UInt64)(((UInt64)info.Bank2Data[0] << 48) | ((UInt64)info.Bank2Data[1] << 32) | ((UInt64)info.Bank2Data[2] << 16) | ((UInt64)info.Bank2Data[3]));

                                if (caldata==0) { item.SensorAvgValue = "NoCalData"; }
                                else {
                                    double SAV = Math.Round(getTempC(temp, caldata), 1);   
                                    item.SensorAvgValue = SAV.ToString();
                                    item.TimeString = DateTime.Now.ToString("HH:mm:ss");

                                    List<string> t_time = new List<string>{ item.TimeString };
                                    List<string> t_data = new List<string>{ item.SensorAvgValue };

                                    try {
                                        tag_Time.Add(item.EPC, t_time);
                                        tag_Data.Add(item.EPC, t_data);
                                        tag_List.Add(item.EPC);
                                    }
                                    finally {}
                                }
                            }
                        }
                        else { }
                        TagInfoList.Insert(0, item);

                        //} // added to filter EPCs to savedata

                    }
                }
            });
        }

        void VoltageEvent(object sender, CSLibrary.Notification.VoltageEventArgs e) {}

        private void AutoSaveData() {    // Function for Sharing time series data from tags
            InvokeOnMainThread(()=> {
                string fileName = pick_result.FullPath;    // Get file name from picker

                File.WriteAllText(fileName, String.Empty); // Empty text file to rewrite database
                using (StreamWriter writer = new StreamWriter(fileName, true)) {
                    foreach (string name in tag_List) {
                        writer.WriteLine(name + "\n" + "[");
                        foreach (var i in tag_Time[name]) { writer.WriteLine(i); }
                        writer.WriteLine("]\n[");
                        foreach (var j in tag_Data[name]) { writer.WriteLine(j); }
                        writer.WriteLine("]\n ");
                    }
                    writer.Close();
                }
            });
        }

        private async void ShareDataButtonClick()
        {
            string fileName = pick_result.FullPath;

            await Share.RequestAsync(new ShareFileRequest {
                Title = "Share Tags",
                File = new ShareFile(fileName)
            });
        }

        #region Key_event
        void HotKeys_OnKeyEvent(object sender, CSLibrary.Notification.HotKeyEventArgs e)
        {
            if (e.KeyCode == CSLibrary.Notification.Key.BUTTON) {
                if (e.KeyDown) { StartInventory(); }
                else           { StopInventory(); }
            }
        }
        #endregion

    }
}
    
