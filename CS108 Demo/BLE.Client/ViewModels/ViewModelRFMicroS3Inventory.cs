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
            private string _sensorAvgValue; public string SensorAvgValue {get { return this._sensorAvgValue; } set { this.SetProperty(ref this._sensorAvgValue, value); } }
            public RFMicroTagInfoViewModel() {}    // Class constructor (constructs nothing)
        }

        private readonly IUserDialogs _userDialogs;

        #region -------------- RFID inventory -----------------

        public ICommand OnStartInventoryButtonCommand {protected set; get; }
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


        #region ------------- Shirt Selection ----------------
        private List<string> _pickerList1; public List<string> pickerList1 { get => _pickerList1; set { _pickerList1 = value; OnPropertyChanged("pickerList1"); } }
        private List<string> _pickerList2; public List<string> pickerList2 { get => _pickerList2; set { _pickerList2 = value; OnPropertyChanged("pickerList2"); } }

        private int _Selected1; 
        public int Selected1 {
            get => _Selected1;
            set { 
                _Selected1 = value; 
                OnPropertyChanged("Selected1");
                _Back1        = "gray"; RaisePropertyChanged(() => Back1);
                _BackNeck1    = "gray"; RaisePropertyChanged(() => BackNeck1);
                _Chest1       = "gray"; RaisePropertyChanged(() => Chest1);
                _LeftAb1      = "gray"; RaisePropertyChanged(() => LeftAb1);
                _RightAb1     = "gray"; RaisePropertyChanged(() => RightAb1);
                _LeftUpArm1   = "gray"; RaisePropertyChanged(() => LeftUpArm1);
                _RightUpArm1  = "gray"; RaisePropertyChanged(() => RightUpArm1);
                _LeftLowArm1  = "gray"; RaisePropertyChanged(() => LeftLowArm1);
                _RightLowArm1 = "gray"; RaisePropertyChanged(() => RightLowArm1);
                _Beanie1      = "gray"; RaisePropertyChanged(() => Beanie1);
                _Back1_T        = "--"; RaisePropertyChanged(() => Back1_T);
                _BackNeck1_T    = "--"; RaisePropertyChanged(() => BackNeck1_T);
                _Chest1_T       = "--"; RaisePropertyChanged(() => Chest1_T);
                _LeftAb1_T      = "--"; RaisePropertyChanged(() => LeftAb1_T);
                _RightAb1_T     = "--"; RaisePropertyChanged(() => RightAb1_T);
                _LeftUpArm1_T   = "--"; RaisePropertyChanged(() => LeftUpArm1_T);
                _RightUpArm1_T  = "--"; RaisePropertyChanged(() => RightUpArm1_T);
                _LeftLowArm1_T  = "--"; RaisePropertyChanged(() => LeftLowArm1_T);
                _RightLowArm1_T = "--"; RaisePropertyChanged(() => RightLowArm1_T);
                _Beanie1_T      = "--"; RaisePropertyChanged(() => Beanie1_T);
            }
        }

        private int _Selected2; 
        public int Selected2 {
            get => _Selected2;
            set { 
                _Selected2 = value; 
                OnPropertyChanged("Selected2");
                _Back2        = "gray"; RaisePropertyChanged(() => Back2);
                _BackNeck2    = "gray"; RaisePropertyChanged(() => BackNeck2);
                _Chest2       = "gray"; RaisePropertyChanged(() => Chest2);
                _LeftAb2      = "gray"; RaisePropertyChanged(() => LeftAb2);
                _RightAb2     = "gray"; RaisePropertyChanged(() => RightAb2);
                _LeftUpArm2   = "gray"; RaisePropertyChanged(() => LeftUpArm2);
                _RightUpArm2  = "gray"; RaisePropertyChanged(() => RightUpArm2);
                _LeftLowArm2  = "gray"; RaisePropertyChanged(() => LeftLowArm2);
                _RightLowArm2 = "gray"; RaisePropertyChanged(() => RightLowArm2);
                _Beanie2      = "gray"; RaisePropertyChanged(() => Beanie2);
                _Back2_T        = "--"; RaisePropertyChanged(() => Back2_T);
                _BackNeck2_T    = "--"; RaisePropertyChanged(() => BackNeck2_T);
                _Chest2_T       = "--"; RaisePropertyChanged(() => Chest2_T);
                _LeftAb2_T      = "--"; RaisePropertyChanged(() => LeftAb2_T);
                _RightAb2_T     = "--"; RaisePropertyChanged(() => RightAb2_T);
                _LeftUpArm2_T   = "--"; RaisePropertyChanged(() => LeftUpArm2_T);
                _RightUpArm2_T  = "--"; RaisePropertyChanged(() => RightUpArm2_T);
                _LeftLowArm2_T  = "--"; RaisePropertyChanged(() => LeftLowArm2_T);
                _RightLowArm2_T = "--"; RaisePropertyChanged(() => RightLowArm2_T);
                _Beanie2_T      = "--"; RaisePropertyChanged(() => Beanie2_T);
            }
        }

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

        public int THRESHOLD = 15;
        class Glove {
            public string LeftHand  { get; set; }
            public string RightHand { get; set; }
            public List<string> TagList { get; set; }

            public Glove(string l_EPC, string r_EPC) {
                LeftHand  = l_EPC;
                RightHand = r_EPC;
                TagList = new List<string> { LeftHand, RightHand };
            }
        }

        class Shirt {
            public string Back          { get; set; }
            public string BackNeck      { get; set; }
            public string Chest         { get; set; }
            public string LeftAb        { get; set; }
            public string RightAb       { get; set; }
            public string LeftUpArm     { get; set; }
            public string RightUpArm    { get; set; }
            public string LeftLowArm    { get; set; }
            public string RightLowArm   { get; set; }
            public List<string> TagList { get; set; }

            public Shirt(
                string backneck, string back, string chest, string leftab, string rightab, 
                string rightuparm, string rightlowarm, string leftuparm, string leftlowarm
            ) {
                // Shirt Locations
                BackNeck    = backneck;
                Back        = back;
                Chest       = chest;
                LeftAb      = leftab;
                RightAb     = rightab;
                LeftUpArm   = leftuparm;
                RightUpArm  = rightuparm;
                LeftLowArm  = leftlowarm;
                RightLowArm = rightlowarm;
                TagList = new List<string> { Back, BackNeck, Chest, LeftAb, RightAb, LeftUpArm, RightUpArm, LeftLowArm, RightLowArm };
            }
        }

        Shirt shirt1  = new Shirt("7F57", "6082", "51BE", "0551", "5D88", "1EB8", "5CA6", "89BA", "5286");
        Shirt shirt2  = new Shirt("3259", "846D", "0469", "8C94", "53C5", "3405", "36C1", "8534", "5866");
        Shirt shirt3  = new Shirt("82BD", "A892", "7A48", "4D1E", "849B", "0D83", "5C9A", "78AE", "877F");
        Shirt shirt4  = new Shirt("26CO", "3CA6", "3D5B", "1D8D", "7C8A", "4768", "843F", "2846", "4257");
        Shirt shirt5  = new Shirt("4594", "1073", "3415", "56AE", "6809", "97A8", "9B3D", "917C", "6627");
        Shirt shirt6  = new Shirt("89BE", "522F", "3D80", "3F51", "597F", "8599", "80DC", "026C", "B574");
        Shirt shirt7  = new Shirt("7625", "5D20", "A8AB", "2BA9", "3D39", "4F7B", "B592", "90A8", "4FAB");
        Shirt shirt8  = new Shirt("30CB", "3592", "3B18", "75D4", "54D3", "5F3A", "8A4C", "73A1", "4CA2");
        Shirt shirt9  = new Shirt("859F", "A75A", "AF4F", "4946", "5AAA", "5FAF", "5C89", "A958", "B66D");
        Shirt shirt10 = new Shirt("1772", "0385", "5487", "1A30", "482E", "4FDF", "5A34", "73CD", "92A1");
        Shirt shirt11 = new Shirt("9D7A", "8913", "A587", "B894", "5988", "1C82", "0088", "AC59", "382A");
        Shirt shirt12 = new Shirt("2124", "1C48", "8485", "9CAC", "8E70", "620E", "8133", "8571", "306B");

        Glove glove1_L  = new Glove("7251", "577F"); Glove glove1_R  = new Glove("8DC7", "4DA7");
        Glove glove2_L  = new Glove("5A93", "1342"); Glove glvoe2_R  = new Glove("69AF", "A074");
        Glove glove3_L  = new Glove("92CB", "2B2C"); Glove glove3_R  = new Glove("4368", "55DF");
        Glove glove4_L  = new Glove("5855", "B964"); Glove glove4_R  = new Glove("B786", "5378");
        Glove glove5_L  = new Glove("3FCE", "39A0"); Glove glove5_R  = new Glove("5E18", "6423");
        Glove glove6_L  = new Glove("6B69", "3BB8"); Glove glove6_R  = new Glove("1235", "422D");
        Glove glove7_L  = new Glove("087D", "3D35"); Glove glove7_R  = new Glove("3B9A", "8582");
        Glove glove8_L  = new Glove("59A9", "9742"); Glove glove8_R  = new Glove("5198", "49D5");
        Glove glove9_L  = new Glove("0A70", "2CAB"); Glove glove9_R  = new Glove("2D5B", "4548");
        Glove glove10_L = new Glove("2361", "BA61"); Glove glove10_R = new Glove("7A30", "2FAB");
        Glove glove11_L = new Glove("0AA9", "6904"); Glove glove11_R = new Glove("7A40", "55D5");
        Glove glove12_L = new Glove("3B21", "1E46"); Glove glove12_R = new Glove("794B", "3ACE");

        Dictionary<int, Shirt> people = new Dictionary<int, Shirt>();
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

            people = new Dictionary<int, Shirt> {
                {0, shirt1},  {1, shirt2},  {2, shirt3}, {3, shirt4},  {4, shirt5},   {5, shirt6},
                {6, shirt7},  {7, shirt8},  {8, shirt9}, {9, shirt10}, {10, shirt11}, {11, shirt12}
            };

            // Set disconnection event for reconnection
            Adapter.DeviceDisconnected += OnDeviceDisconnected;
            Adapter.DeviceConnectionLost += OnDeviceDisconnected; // connection or discconnect?


            // Setup Picker Lists on Initialization
            _pickerList1 = new List<string>{
                "Shirt 1", "Shirt 2", "Shirt 3", "Shirt 4",  "Shirt 5",  "Shirt 6",
                "Shirt 7", "Shirt 8", "Shirt 9", "Shirt 10", "Shirt 11", "Shirt 12"
            };
            RaisePropertyChanged(() => pickerList1);


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
                // _Chest1 = "red";
                // RaisePropertyChanged(() => Chest1);
                // _Back1 = "red";
                // RaisePropertyChanged(() => Back1);
                // _Back2 = "red";
                // RaisePropertyChanged(() => Back2);

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
                _startInventoryButtonText = "Stop Inventory";
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
            downtimer.Interval = active_time;          // READER IS ACTIVE FOR THIS LONG
            downtimer.Elapsed += DownEvent;
            downtimer.Enabled = false;
        }

        private void ActiveEvent(object sender, System.Timers.ElapsedEventArgs e) {  
            activetimer.Enabled = false;
            downtimer.Enabled = true;
        }

        private void DownEvent(object sender, System.Timers.ElapsedEventArgs e) {
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
                        // if (epcs.Contains(info.epc.ToString()) && (TagInfoList[cnt].EPC == info.epc.ToString())) {

                        if (TagInfoList[cnt].EPC==info.epc.ToString()) {
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
                                            string temp_EPC = TagInfoList[cnt].EPC.Substring(TagInfoList[cnt].EPC.Length - 4);

                                            Shirt p1 = people[Selected1];
                                            Shirt p2 = people[Selected2];

                                            if (p1.TagList.Contains(temp_EPC)) {
                                                if (temp_EPC==p1.Back) { 
                                                    _Back1_T = DisplaySAV;
                                                    RaisePropertyChanged(() => Back1_T);
                                                    if ((SAV>THRESHOLD) && (_Back1!="green")) {
                                                        _Back1 = "green";
                                                        RaisePropertyChanged(() => Back1);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_Back1!="red")) {
                                                        _Back1 = "red";
                                                        RaisePropertyChanged(() => Back1);
                                                    } 
                                                }
                                                else if (temp_EPC==p1.Chest) { 
                                                    _Chest1_T = DisplaySAV;
                                                    RaisePropertyChanged(() => Chest1_T);
                                                    if ((SAV>THRESHOLD) && (_Chest1!="green")) {
                                                        _Chest1 = "green";
                                                        RaisePropertyChanged(() => Chest1);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_Chest1!="red")) {
                                                        _Chest1 = "red";
                                                        RaisePropertyChanged(() => Chest1);
                                                    }
                                                }
                                                else if (temp_EPC==p1.BackNeck) {
                                                    _BackNeck1_T = DisplaySAV;
                                                    RaisePropertyChanged(() => BackNeck1_T);
                                                    if ((SAV>THRESHOLD) && (_BackNeck1!="green")) {
                                                        _BackNeck1 = "green";
                                                        RaisePropertyChanged(() => BackNeck1);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_BackNeck1!="red")) {
                                                        _BackNeck1 = "red";
                                                        RaisePropertyChanged(() => BackNeck1);
                                                    }
                                                }
                                                else if (temp_EPC==p1.LeftAb) {
                                                    _LeftAb1_T = DisplaySAV;
                                                    RaisePropertyChanged(() => LeftAb1_T);
                                                    if ((SAV>THRESHOLD) && (_LeftAb1!="green")) {
                                                        _LeftAb1 = "green";
                                                        RaisePropertyChanged(() => LeftAb1);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftAb1!="red")) {
                                                        _LeftAb1 = "red";
                                                        RaisePropertyChanged(() => LeftAb1);
                                                    }
                                                }
                                                else if (temp_EPC==p1.RightAb) {
                                                    _RightAb1_T = DisplaySAV;
                                                    RaisePropertyChanged(() => RightAb1_T);
                                                    if ((SAV>THRESHOLD) && (_RightAb1!="green")) {
                                                        _RightAb1 = "green";
                                                        RaisePropertyChanged(() => RightAb1);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightAb1!="red")) {
                                                        _RightAb1 = "red";
                                                        RaisePropertyChanged(() => RightAb1);
                                                    }
                                                }
                                                else if (temp_EPC==p1.LeftUpArm) {
                                                    _LeftUpArm1_T = DisplaySAV;
                                                    RaisePropertyChanged(() => LeftUpArm1_T);
                                                    if ((SAV>THRESHOLD) && (_LeftUpArm1!="green")) {
                                                        _LeftUpArm1 = "green";
                                                        RaisePropertyChanged(() => LeftUpArm1);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftUpArm1!="red")) {
                                                        _LeftUpArm1 = "red";
                                                        RaisePropertyChanged(() => LeftUpArm1);
                                                    }
                                                }
                                                else if (temp_EPC==p1.RightUpArm) {
                                                    _RightUpArm1_T = DisplaySAV;
                                                    RaisePropertyChanged(() => RightUpArm1_T);
                                                    if ((SAV>THRESHOLD) && (_RightUpArm1!="green")) {
                                                        _RightUpArm1 = "green";
                                                        RaisePropertyChanged(() => RightUpArm1);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightUpArm1!="red")) {
                                                        _RightUpArm1 = "red";
                                                        RaisePropertyChanged(() => RightUpArm1);
                                                    }
                                                }
                                                else if (temp_EPC==p1.LeftLowArm) {
                                                    _LeftLowArm1_T = DisplaySAV;
                                                    RaisePropertyChanged(() => LeftLowArm1_T);
                                                    if ((SAV>THRESHOLD) && (_LeftLowArm1!="green")) {
                                                        _LeftLowArm1 = "green";
                                                        RaisePropertyChanged(() => LeftLowArm1);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftLowArm1!="red")) {
                                                        _LeftLowArm1 = "red";
                                                        RaisePropertyChanged(() => LeftLowArm1);
                                                    }
                                                }
                                                else if (temp_EPC==p1.RightLowArm) {
                                                    _RightLowArm1_T = DisplaySAV;
                                                    RaisePropertyChanged(() => RightLowArm1_T);
                                                    if ((SAV>THRESHOLD) && (_RightLowArm1!="green")) {
                                                        _RightLowArm1 = "green";
                                                        RaisePropertyChanged(() => RightLowArm1);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightLowArm1!="red")) {
                                                        _RightLowArm1 = "red";
                                                        RaisePropertyChanged(() => RightLowArm1);
                                                    }
                                                }
                                            }

                                            if (p2.TagList.Contains(temp_EPC)) {
                                                if (temp_EPC==p2.Back) {
                                                    _Back2_T = DisplaySAV;
                                                    RaisePropertyChanged(() => Back2_T);
                                                    if ((SAV>THRESHOLD) && (_Back2!="green")) {
                                                        _Back2 = "green";
                                                        RaisePropertyChanged(() => Back2);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_Back2!="red")) {
                                                        _Back2 = "red";
                                                        RaisePropertyChanged(() => Back2);
                                                    }
                                                }
                                                else if (temp_EPC==p2.Chest) {
                                                    _Chest2_T = DisplaySAV;
                                                    RaisePropertyChanged(() => Chest2_T);
                                                    if ((SAV>THRESHOLD) && (_Chest2!="green")) {
                                                        _Chest2 = "green";
                                                        RaisePropertyChanged(() => Chest2);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_Chest2!="red")) {
                                                        _Chest2 = "red";
                                                        RaisePropertyChanged(() => Chest2);
                                                    }
                                                }
                                                else if (temp_EPC==p2.BackNeck) {
                                                    _BackNeck2_T = DisplaySAV;
                                                    RaisePropertyChanged(() => BackNeck2_T);
                                                    if ((SAV>THRESHOLD) && (_BackNeck2!="green")) {
                                                        _BackNeck2 = "green";
                                                        RaisePropertyChanged(() => BackNeck2);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_BackNeck2!="red")) {
                                                        _BackNeck2 = "red";
                                                        RaisePropertyChanged(() => BackNeck2);
                                                    }
                                                }
                                                else if (temp_EPC==p2.LeftAb) {
                                                    _LeftAb2_T = DisplaySAV;
                                                    RaisePropertyChanged(() => LeftAb2_T);
                                                    if ((SAV>THRESHOLD) && (_LeftAb2!="green")) {
                                                        _LeftAb2 = "green";
                                                        RaisePropertyChanged(() => LeftAb2);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftAb2!="red")) {
                                                        _LeftAb2 = "red";
                                                        RaisePropertyChanged(() => LeftAb2);
                                                    }
                                                }
                                                else if (temp_EPC==p2.RightAb) {
                                                    _RightAb2_T = DisplaySAV;
                                                    RaisePropertyChanged(() => RightAb2_T);
                                                    if ((SAV>THRESHOLD) && (_RightAb2!="green")) {
                                                        _RightAb2 = "green";
                                                        RaisePropertyChanged(() => RightAb2);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightAb2!="red")) {
                                                        _RightAb2 = "red";
                                                        RaisePropertyChanged(() => RightAb2);
                                                    }
                                                }
                                                else if (temp_EPC==p2.LeftUpArm) {
                                                    _LeftUpArm2_T = DisplaySAV;
                                                    RaisePropertyChanged(() => LeftUpArm2_T);
                                                    if ((SAV>THRESHOLD) && (_LeftUpArm2!="green")) {
                                                        _LeftUpArm2 = "green";
                                                        RaisePropertyChanged(() => LeftUpArm2);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftUpArm2!="red")) {
                                                        _LeftUpArm2 = "red";
                                                        RaisePropertyChanged(() => LeftUpArm2);
                                                    }
                                                }
                                                else if (temp_EPC==p2.RightUpArm) {
                                                    _RightUpArm2_T = DisplaySAV;
                                                    RaisePropertyChanged(() => RightUpArm2_T);
                                                    if ((SAV>THRESHOLD) && (_RightUpArm2!="green")) {
                                                        _RightUpArm2 = "green";
                                                        RaisePropertyChanged(() => RightUpArm2);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightUpArm2!="red")) {
                                                        _RightUpArm2 = "red";
                                                        RaisePropertyChanged(() => RightUpArm2);
                                                    }
                                                }
                                                else if (temp_EPC==p2.LeftLowArm) {
                                                    _LeftLowArm2_T = DisplaySAV;
                                                    RaisePropertyChanged(() => LeftLowArm2_T);
                                                    if ((SAV>THRESHOLD) && (_LeftLowArm2!="green")) {
                                                        _LeftLowArm2 = "green";
                                                        RaisePropertyChanged(() => LeftLowArm2);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftLowArm2!="red")) {
                                                        _LeftLowArm2 = "red";
                                                        RaisePropertyChanged(() => LeftLowArm2);
                                                    }
                                                }
                                                else if (temp_EPC==p2.RightLowArm) {
                                                    _RightLowArm2_T = DisplaySAV;
                                                    RaisePropertyChanged(() => RightLowArm2_T);
                                                    if ((SAV>THRESHOLD) && (_RightLowArm2!="green")) {
                                                        _RightLowArm2 = "green";
                                                        RaisePropertyChanged(() => RightLowArm2);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightLowArm2!="red")) {
                                                        _RightLowArm2 = "red";
                                                        RaisePropertyChanged(() => RightLowArm2);
                                                    }
                                                }
                                            }

                                            // else if (p2.Beanie.Contains(temp_EPC)) {
                                            //     _Beanie2_T = DisplaySAV;
                                            //     RaisePropertyChanged(() => Beanie2_T);
                                            //     if ((SAV>THRESHOLD) && (_Beanie2!="green"))
                                            //     {
                                            //         _Beanie2 = "green"; RaisePropertyChanged(() => Beanie2);
                                            //     }
                                            //     else if ((SAV<=THRESHOLD) && (_Beanie2!="red"))
                                            //     {
                                            //         _Beanie2 = "red"; RaisePropertyChanged(() => Beanie2);
                                            //     }
                                            // }

                                        }
                                    }
                                }
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
                            if (temp >= 1300 && temp <= 3500) {
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
                        // }
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
    
