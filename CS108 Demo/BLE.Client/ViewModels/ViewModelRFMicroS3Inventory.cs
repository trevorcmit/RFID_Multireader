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
        class Glove {
            public string Forefinger  { get; set; }
            public string Pinkie { get; set; }
            public List<string> TagList { get; set; }

            public Glove(string f, string p) {
                Forefinger = f;
                Pinkie = p;
                TagList = new List<string> { Forefinger, Pinkie };
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

        // Hefner, L   - 1, 4, 8, 9
        // Gelinas, M  - 2, 3, 11
        // Person 3, L - 5, 6, 10
        Shirt shirt1  = new Shirt("7F57", "6082", "51BE", "0551", "5D88", "1EB8", "5CA6", "89BA", "5286");
        Shirt shirt2  = new Shirt("3259", "846D", "0469", "8C94", "53C5", "3405", "36C1", "8534", "5866");
        Shirt shirt3  = new Shirt("82BD", "A892", "7A48", "4D1E", "849B", "0D83", "5C9A", "78AE", "877F");
        Shirt shirt4  = new Shirt("26CO", "3CA6", "3D5B", "1D8D", "7C8A", "4768", "843F", "2846", "4257");
        Shirt shirt5  = new Shirt("4594", "1073", "3415", "56AE", "6809", "97A8", "9B3D", "917C", "6627");
        Shirt shirt6  = new Shirt("89BE", "522F", "3D80", "3F51", "597F", "8599", "80DC", "026C", "B574");
        // Shirt shirt7  = new Shirt("7625", "5D20", "A8AB", "2BA9", "3D39", "4F7B", "B592", "90A8", "4FAB");
        Shirt shirt8  = new Shirt("30CB", "3592", "3B18", "75D4", "54D3", "5F3A", "8A4C", "73A1", "4CA2");
        Shirt shirt9  = new Shirt("859F", "A75A", "AF4F", "4946", "5AAA", "5FAF", "5C89", "A958", "B66D");
        Shirt shirt10 = new Shirt("1772", "0385", "5487", "1A30", "482E", "4FDF", "5A34", "73CD", "92A1");
        Shirt shirt11 = new Shirt("9D7A", "8913", "A587", "B894", "5988", "1C82", "0088", "AC59", "382A");
        // Shirt shirt12 = new Shirt("2124", "1C48", "8485", "9CAC", "8E70", "620E", "8133", "8571", "306B");

        // Hefner, Glove   - 1, 2, 5
        // Gelinas, Glove  - 7, 8, 9
        // Person 3, Glove - 6, 11, 12
        Glove glove1_L  = new Glove( "7251", "577F" ); Glove glove1_R  = new Glove( "8DC7", "4DA7" );
        Glove glove2_L  = new Glove( "5A93", "1342" ); Glove glove2_R  = new Glove( "69AF", "A074" );
        // Glove glove3_L  = new Glove( "92CB", "2B2C" ); Glove glove3_R  = new Glove( "4368", "55DF" );
        // Glove glove4_L  = new Glove( "5855", "B964" ); Glove glove4_R  = new Glove( "B786", "5378" );
        Glove glove5_L  = new Glove( "3FCE", "39A0" ); Glove glove5_R  = new Glove( "5E18", "6423" );
        Glove glove6_L  = new Glove( "6B69", "3BB8" ); Glove glove6_R  = new Glove( "1235", "422D" );
        Glove glove7_L  = new Glove( "087D", "3D35" ); Glove glove7_R  = new Glove( "3B9A", "8582" );
        Glove glove8_L  = new Glove( "59A9", "9742" ); Glove glove8_R  = new Glove( "5198", "49D5" );
        Glove glove9_L  = new Glove( "0A70", "2CAB" ); Glove glove9_R  = new Glove( "2D5B", "4548" );
        // Glove glove10_L = new Glove( "2361", "BA61" ); Glove glove10_R = new Glove( "7A30", "2FAB" );
        Glove glove11_L = new Glove( "0AA9", "6904" ); Glove glove11_R = new Glove( "7A40", "55D5" );
        Glove glove12_L = new Glove( "3B21", "1E46" ); Glove glove12_R = new Glove( "794B", "3ACE" );

        // Beanie beanie1 = new Beanie( "639A", "7971" );
        // Beanie beanie2 = new Beanie( "B878", "7819" );
        // Beanie beanie3 = new Beanie( "B365", "348A" );
        // Beanie beanie4 = new Beanie( "7166", "398F" );
        // Beanie beanie5 = new Beanie( "77B2", "3E64" );
        // Beanie beanie6 = new Beanie( "4C2D", "3848" );
        List<string> Hefner_Beanie  = new List<string>{ "639A", "7971", "B878", "7819" };
        List<string> Gelinas_Beanie = new List<string>{ "B365", "348A", "7166", "398F" };
        List<string> Person3_Beanie = new List<string>{ "77B2", "3E64", "4C2D", "3848" };

        // Balaclava bala1 = new Balaclava( "5410", "6D60", "19BF", "47D4" );
        // Balaclava bala2 = new Balaclava( "928A", "6221", "5EB7", "----" );
        // Balaclava bala3 = new Balaclava( "8A77", "6818", "4D9D", "A45B" );
        // Balaclava bala4 = new Balaclava( "96C0", "9B90", "9D39", "9DA6" );
        // Balaclava bala5 = new Balaclava( "2F77", "2CA4", "5FDB", "A382" );
        // Balaclava bala6 = new Balaclava( "304D", "564E", "62D2", "7E48" );

        List<string> Hefner_Bala  = new List<string>{ "5410", "6D60", "19BF", "47D4", "928A", "6221", "5EB7", "----" };
        List<string> Gelinas_Bala = new List<string>{ "8A77", "6818", "4D9D", "A45B", "96C0", "9B90", "9D39", "9DA6" };
        List<string> Person3_Bala = new List<string>{ "2F77", "2CA4", "5FDB", "A382", "304D", "564E", "62D2", "7E48" };

        List<string> Hefner_Back; List<string> Hefner_BackNeck; List<string> Hefner_Chest;
        List<string> Hefner_LeftAb; List<string> Hefner_RightAb;
        List<string> Hefner_LeftUpArm; List<string> Hefner_RightUpArm; List<string> Hefner_LeftLowArm; List<string> Hefner_RightLowArm;

        List<string> Gelinas_Back; List<string> Gelinas_BackNeck; List<string> Gelinas_Chest;
        List<string> Gelinas_LeftAb; List<string> Gelinas_RightAb;
        List<string> Gelinas_LeftUpArm; List<string> Gelinas_RightUpArm; List<string> Gelinas_LeftLowArm; List<string> Gelinas_RightLowArm;

        List<string> Person3_Back; List<string> Person3_BackNeck; List<string> Person3_Chest;
        List<string> Person3_LeftAb; List<string> Person3_RightAb;
        List<string> Person3_LeftUpArm; List<string> Person3_RightUpArm; List<string> Person3_LeftLowArm; List<string> Person3_RightLowArm;

        List<string> Hefner_Rglove; List<string> Hefner_Lglove;
        List<string> Gelinas_Rglove; List<string> Gelinas_Lglove;
        List<string> Person3_Rglove; List<string> Person3_Lglove;
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

            Hefner_Back = new List<string> { shirt1.Back, shirt4.Back, shirt8.Back, shirt9.Back };
            Hefner_BackNeck = new List<string> { shirt1.BackNeck, shirt4.BackNeck, shirt8.BackNeck, shirt9.BackNeck };
            Hefner_Chest = new List<string> { shirt1.Chest, shirt4.Chest, shirt8.Chest, shirt9.Chest };
            Hefner_LeftAb = new List<string> { shirt1.LeftAb, shirt4.LeftAb, shirt8.LeftAb, shirt9.LeftAb };
            Hefner_RightAb = new List<string> { shirt1.RightAb, shirt4.RightAb, shirt8.RightAb, shirt9.RightAb };
            Hefner_LeftUpArm = new List<string> { shirt1.LeftUpArm, shirt4.LeftUpArm, shirt8.LeftUpArm, shirt9.LeftUpArm };
            Hefner_RightUpArm = new List<string> { shirt1.RightUpArm, shirt4.RightUpArm, shirt8.RightUpArm, shirt9.RightUpArm };
            Hefner_LeftLowArm = new List<string> { shirt1.LeftLowArm, shirt4.LeftLowArm, shirt8.LeftLowArm, shirt9.LeftLowArm };
            Hefner_RightLowArm = new List<string> { shirt1.RightLowArm, shirt4.RightLowArm, shirt8.RightLowArm, shirt9.RightLowArm };

            Gelinas_Back = new List<string> { shirt2.Back, shirt3.Back, shirt11.Back };
            Gelinas_BackNeck = new List<string> { shirt2.BackNeck, shirt3.BackNeck, shirt11.BackNeck };
            Gelinas_Chest = new List<string> { shirt2.Chest, shirt3.Chest, shirt11.Chest };
            Gelinas_LeftAb = new List<string> { shirt2.LeftAb, shirt3.LeftAb, shirt11.LeftAb };
            Gelinas_RightAb = new List<string> { shirt2.RightAb, shirt3.RightAb, shirt11.RightAb };
            Gelinas_LeftUpArm = new List<string> { shirt2.LeftUpArm, shirt3.LeftUpArm, shirt11.LeftUpArm };
            Gelinas_RightUpArm = new List<string> { shirt2.RightUpArm, shirt3.RightUpArm, shirt11.RightUpArm };
            Gelinas_LeftLowArm = new List<string> { shirt2.LeftLowArm, shirt3.LeftLowArm, shirt11.LeftLowArm };
            Gelinas_RightLowArm = new List<string> { shirt2.RightLowArm, shirt3.RightLowArm, shirt11.RightLowArm };

            Person3_Back = new List<string> { shirt5.Back, shirt6.Back, shirt10.Back };
            Person3_BackNeck = new List<string> { shirt5.BackNeck, shirt6.BackNeck, shirt10.BackNeck };
            Person3_Chest = new List<string> { shirt5.Chest, shirt6.Chest, shirt10.Chest };
            Person3_LeftAb = new List<string> { shirt5.LeftAb, shirt6.LeftAb, shirt10.LeftAb };
            Person3_RightAb = new List<string> { shirt5.RightAb, shirt6.RightAb, shirt10.RightAb };
            Person3_LeftUpArm = new List<string> { shirt5.LeftUpArm, shirt6.LeftUpArm, shirt10.LeftUpArm };
            Person3_RightUpArm = new List<string> { shirt5.RightUpArm, shirt6.RightUpArm, shirt10.RightUpArm };
            Person3_LeftLowArm = new List<string> { shirt5.LeftLowArm, shirt6.LeftLowArm, shirt10.LeftLowArm };
            Person3_RightLowArm = new List<string> { shirt5.RightLowArm, shirt6.RightLowArm, shirt10.RightLowArm };

            Hefner_Lglove = new List<string> { glove1_L.Forefinger, glove1_L.Pinkie, glove2_L.Forefinger, glove2_L.Pinkie, glove5_L.Forefinger, glove5_L.Pinkie };
            Hefner_Rglove = new List<string> { glove1_R.Forefinger, glove1_R.Pinkie, glove2_R.Forefinger, glove2_R.Pinkie, glove5_R.Forefinger, glove5_R.Pinkie };
            Gelinas_Lglove = new List<string> { glove7_L.Forefinger, glove7_L.Pinkie, glove8_L.Forefinger, glove8_L.Pinkie, glove9_L.Forefinger, glove9_L.Pinkie };
            Gelinas_Rglove = new List<string> { glove7_R.Forefinger, glove7_R.Pinkie, glove8_R.Forefinger, glove8_R.Pinkie, glove9_R.Forefinger, glove9_R.Pinkie };
            Person3_Lglove = new List<string> { glove6_L.Forefinger, glove6_L.Pinkie, glove11_L.Forefinger, glove11_L.Pinkie, glove12_L.Forefinger, glove12_L.Pinkie };
            Person3_Rglove = new List<string> { glove6_R.Forefinger, glove6_R.Pinkie, glove11_R.Forefinger, glove11_R.Pinkie, glove12_R.Forefinger, glove12_R.Pinkie };

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

                                            // Glove Section
                                            // Hefner   - 1, 2, 5
                                            // Gelinas  - 7, 8, 9
                                            // Person 3 - 6, 11, 12
                                            if (Hefner_Lglove.Contains(temp_EPC)) {
                                                _LeftHand1_T = DisplaySAV;
                                                RaisePropertyChanged(() => LeftHand1_T);
                                                if ((SAV>THRESHOLD) && (_LeftHand1!="green")) {
                                                    _LeftHand1 = "green";
                                                    RaisePropertyChanged(() => LeftHand1);
                                                }
                                                else if ((SAV<=THRESHOLD) && (_LeftHand1!="red")) {
                                                    _LeftHand1 = "red";
                                                    RaisePropertyChanged(() => LeftHand1);
                                                } 
                                            }
                                            else if (Hefner_Rglove.Contains(temp_EPC)) {
                                                _RightHand1_T = DisplaySAV;
                                                RaisePropertyChanged(() => RightHand1_T);
                                                if ((SAV>THRESHOLD) && (_RightHand1!="green")) {
                                                    _RightHand1 = "green";
                                                    RaisePropertyChanged(() => RightHand1);
                                                }
                                                else if ((SAV<=THRESHOLD) && (_RightHand1!="red")) {
                                                    _RightHand1 = "red";
                                                    RaisePropertyChanged(() => RightHand1);
                                                } 
                                            }
                                            else if (Gelinas_Lglove.Contains(temp_EPC)) {
                                                _LeftHand2_T = DisplaySAV;
                                                RaisePropertyChanged(() => LeftHand2_T);
                                                if ((SAV>THRESHOLD) && (_LeftHand2!="green")) {
                                                    _LeftHand2 = "green";
                                                    RaisePropertyChanged(() => LeftHand2);
                                                }
                                                else if ((SAV<=THRESHOLD) && (_LeftHand2!="red")) {
                                                    _LeftHand2 = "red";
                                                    RaisePropertyChanged(() => LeftHand2);
                                                } 
                                            }
                                            else if (Gelinas_Rglove.Contains(temp_EPC)) {
                                                _RightHand2_T = DisplaySAV;
                                                RaisePropertyChanged(() => RightHand2_T);
                                                if ((SAV>THRESHOLD) && (_RightHand2!="green")) {
                                                    _RightHand2 = "green";
                                                    RaisePropertyChanged(() => RightHand2);
                                                }
                                                else if ((SAV<=THRESHOLD) && (_RightHand2!="red")) {
                                                    _RightHand2 = "red";
                                                    RaisePropertyChanged(() => RightHand2);
                                                } 
                                            }
                                            else if (Person3_Lglove.Contains(temp_EPC)) {
                                                _LeftHand3_T = DisplaySAV;
                                                RaisePropertyChanged(() => LeftHand3_T);
                                                if ((SAV>THRESHOLD) && (_LeftHand3!="green")) {
                                                    _LeftHand3 = "green";
                                                    RaisePropertyChanged(() => LeftHand3);
                                                }
                                                else if ((SAV<=THRESHOLD) && (_LeftHand3!="red")) {
                                                    _LeftHand3 = "red";
                                                    RaisePropertyChanged(() => LeftHand3);
                                                } 
                                            }
                                            else if (Person3_Rglove.Contains(temp_EPC)) {
                                                _RightHand3_T = DisplaySAV;
                                                RaisePropertyChanged(() => RightHand3_T);
                                                if ((SAV>THRESHOLD) && (_RightHand3!="green")) {
                                                    _RightHand3 = "green";
                                                    RaisePropertyChanged(() => RightHand3);
                                                }
                                                else if ((SAV<=THRESHOLD) && (_RightHand3!="red")) {
                                                    _RightHand3 = "red";
                                                    RaisePropertyChanged(() => RightHand3);
                                                } 
                                            }

                                            // Beanie and Balaclava Section
                                            // Hefner   - 1, 2
                                            // Gelinas  - 3, 4
                                            // Person 3 - 5, 6
                                            else if (Hefner_Beanie.Contains(temp_EPC)) {
                                                _Beanie1_T = DisplaySAV;
                                                RaisePropertyChanged(() => Beanie1_T);
                                                if ((SAV>THRESHOLD) && (_Beanie1!="green")) {
                                                    _Beanie1 = "green";
                                                    RaisePropertyChanged(() => Beanie1);
                                                }
                                                else if ((SAV<=THRESHOLD) && (_Beanie1!="red")) {
                                                    _Beanie1 = "red";
                                                    RaisePropertyChanged(() => Beanie1);
                                                } 
                                            }
                                            else if (Gelinas_Beanie.Contains(temp_EPC)) {
                                                _Beanie2_T = DisplaySAV;
                                                RaisePropertyChanged(() => Beanie2_T);
                                                if ((SAV>THRESHOLD) && (_Beanie2!="green")) {
                                                    _Beanie2 = "green";
                                                    RaisePropertyChanged(() => Beanie2);
                                                }
                                                else if ((SAV<=THRESHOLD) && (_Beanie2!="red")) {
                                                    _Beanie2 = "red";
                                                    RaisePropertyChanged(() => Beanie2);
                                                } 
                                            }
                                            else if (Person3_Beanie.Contains(temp_EPC)) {
                                                _Beanie3_T = DisplaySAV;
                                                RaisePropertyChanged(() => Beanie3_T);
                                                if ((SAV>THRESHOLD) && (_Beanie3!="green")) {
                                                    _Beanie3 = "green";
                                                    RaisePropertyChanged(() => Beanie3);
                                                }
                                                else if ((SAV<=THRESHOLD) && (_Beanie3!="red")) {
                                                    _Beanie3 = "red";
                                                    RaisePropertyChanged(() => Beanie3);
                                                } 
                                            }
                                            else if (Hefner_Bala.Contains(temp_EPC)) {
                                                _Bala1_T = DisplaySAV;
                                                RaisePropertyChanged(() => Bala1_T);
                                                if ((SAV>THRESHOLD) && (_Bala1!="green")) {
                                                    _Bala1 = "green";
                                                    RaisePropertyChanged(() => Bala1);
                                                }
                                                else if ((SAV<=THRESHOLD) && (_Bala1!="red")) {
                                                    _Bala1 = "red";
                                                    RaisePropertyChanged(() => Bala1);
                                                } 
                                            }
                                            else if (Gelinas_Bala.Contains(temp_EPC)) {
                                                _Bala2_T = DisplaySAV;
                                                RaisePropertyChanged(() => Bala2_T);
                                                if ((SAV>THRESHOLD) && (_Bala2!="green")) {
                                                    _Bala2 = "green";
                                                    RaisePropertyChanged(() => Bala2);
                                                }
                                                else if ((SAV<=THRESHOLD) && (_Bala2!="red")) {
                                                    _Bala2 = "red";
                                                    RaisePropertyChanged(() => Bala2);
                                                } 
                                            }
                                            else if (Person3_Bala.Contains(temp_EPC)) {
                                                _Bala3_T = DisplaySAV;
                                                RaisePropertyChanged(() => Bala3_T);
                                                if ((SAV>THRESHOLD) && (_Bala3!="green")) {
                                                    _Bala3 = "green";
                                                    RaisePropertyChanged(() => Bala3);
                                                }
                                                else if ((SAV<=THRESHOLD) && (_Bala3!="red")) {
                                                    _Bala3 = "red";
                                                    RaisePropertyChanged(() => Bala3);
                                                } 
                                            }

                                            // Shirt Section
                                            // Hefner, L   - 1, 4, 8, 9
                                            // Gelinas, M  - 2, 3, 11
                                            // Person 3, L - 5, 6, 10
                                            else if ( // Hefner Shirt
                                                (shirt1.TagList.Contains(temp_EPC))||
                                                (shirt4.TagList.Contains(temp_EPC))||
                                                (shirt8.TagList.Contains(temp_EPC))||
                                                (shirt9.TagList.Contains(temp_EPC))
                                            ) {
                                                if (Hefner_Back.Contains(temp_EPC)) { 
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
                                                else if (Hefner_Chest.Contains(temp_EPC)) { 
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
                                                else if (Hefner_BackNeck.Contains(temp_EPC)) {
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
                                                else if (Hefner_LeftAb.Contains(temp_EPC)) {
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
                                                else if (Hefner_RightAb.Contains(temp_EPC)) {
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
                                                else if (Hefner_LeftUpArm.Contains(temp_EPC)) { 
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
                                                else if (Hefner_RightUpArm.Contains(temp_EPC)) {
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
                                                else if (Hefner_LeftLowArm.Contains(temp_EPC)) {
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
                                                else if (Hefner_RightLowArm.Contains(temp_EPC)) {
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
                                            
                                            else if ( // Gelinas Shirt
                                                (shirt2.TagList.Contains(temp_EPC))||
                                                (shirt3.TagList.Contains(temp_EPC))||
                                                (shirt11.TagList.Contains(temp_EPC))
                                            ) {
                                                if (Gelinas_Back.Contains(temp_EPC)) {
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
                                                else if (Gelinas_Chest.Contains(temp_EPC)) {
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
                                                else if (Gelinas_BackNeck.Contains(temp_EPC)) {
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
                                                else if (Gelinas_LeftAb.Contains(temp_EPC)) {
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
                                                else if (Gelinas_RightAb.Contains(temp_EPC)) {
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
                                                else if (Gelinas_LeftUpArm.Contains(temp_EPC)) {
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
                                                else if (Gelinas_RightUpArm.Contains(temp_EPC)) {
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
                                                else if (Gelinas_LeftLowArm.Contains(temp_EPC)) {
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
                                                else if (Gelinas_RightLowArm.Contains(temp_EPC)) {
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

                                            else if ( // Person 3 Shirt
                                                (shirt5.TagList.Contains(temp_EPC))||
                                                (shirt6.TagList.Contains(temp_EPC))||
                                                (shirt10.TagList.Contains(temp_EPC))
                                            ) {
                                              if (Person3_Back.Contains(temp_EPC)) {
                                                    _Back3_T = DisplaySAV;
                                                    RaisePropertyChanged(() => Back3_T);
                                                    if ((SAV>THRESHOLD) && (_Back3!="green")) {
                                                        _Back3 = "green";
                                                        RaisePropertyChanged(() => Back3);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_Back3!="red")) {
                                                        _Back3 = "red";
                                                        RaisePropertyChanged(() => Back3);
                                                    }
                                                }
                                                else if (Person3_Chest.Contains(temp_EPC)) {
                                                    _Chest3_T = DisplaySAV;
                                                    RaisePropertyChanged(() => Chest3_T);
                                                    if ((SAV>THRESHOLD) && (_Chest3!="green")) {
                                                        _Chest3 = "green";
                                                        RaisePropertyChanged(() => Chest3);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_Chest3!="red")) {
                                                        _Chest3 = "red";
                                                        RaisePropertyChanged(() => Chest3);
                                                    }
                                                }
                                                else if (Person3_BackNeck.Contains(temp_EPC)) {
                                                    _BackNeck3_T = DisplaySAV;
                                                    RaisePropertyChanged(() => BackNeck3_T);
                                                    if ((SAV>THRESHOLD) && (_BackNeck3!="green")) {
                                                        _BackNeck3 = "green";
                                                        RaisePropertyChanged(() => BackNeck3);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_BackNeck3!="red")) {
                                                        _BackNeck3 = "red";
                                                        RaisePropertyChanged(() => BackNeck3);
                                                    }
                                                }
                                                else if (Person3_LeftAb.Contains(temp_EPC)) {
                                                    _LeftAb3_T = DisplaySAV;
                                                    RaisePropertyChanged(() => LeftAb3_T);
                                                    if ((SAV>THRESHOLD) && (_LeftAb3!="green")) {
                                                        _LeftAb3 = "green";
                                                        RaisePropertyChanged(() => LeftAb3);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftAb3!="red")) {
                                                        _LeftAb3 = "red";
                                                        RaisePropertyChanged(() => LeftAb3);
                                                    }
                                                }
                                                else if (Person3_RightAb.Contains(temp_EPC)) {
                                                    _RightAb3_T = DisplaySAV;
                                                    RaisePropertyChanged(() => RightAb3_T);
                                                    if ((SAV>THRESHOLD) && (_RightAb3!="green")) {
                                                        _RightAb3 = "green";
                                                        RaisePropertyChanged(() => RightAb3);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightAb3!="red")) {
                                                        _RightAb3 = "red";
                                                        RaisePropertyChanged(() => RightAb3);
                                                    }
                                                }
                                                else if (Person3_LeftUpArm.Contains(temp_EPC)) {
                                                    _LeftUpArm3_T = DisplaySAV;
                                                    RaisePropertyChanged(() => LeftUpArm3_T);
                                                    if ((SAV>THRESHOLD) && (_LeftUpArm3!="green")) {
                                                        _LeftUpArm3 = "green";
                                                        RaisePropertyChanged(() => LeftUpArm3);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftUpArm3!="red")) {
                                                        _LeftUpArm3 = "red";
                                                        RaisePropertyChanged(() => LeftUpArm3);
                                                    }
                                                }
                                                else if (Person3_RightUpArm.Contains(temp_EPC)) {
                                                    _RightUpArm3_T = DisplaySAV;
                                                    RaisePropertyChanged(() => RightUpArm3_T);
                                                    if ((SAV>THRESHOLD) && (_RightUpArm3!="green")) {
                                                        _RightUpArm3 = "green";
                                                        RaisePropertyChanged(() => RightUpArm3);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightUpArm3!="red")) {
                                                        _RightUpArm3 = "red";
                                                        RaisePropertyChanged(() => RightUpArm3);
                                                    }
                                                }
                                                else if (Person3_LeftLowArm.Contains(temp_EPC)) {
                                                    _LeftLowArm3_T = DisplaySAV;
                                                    RaisePropertyChanged(() => LeftLowArm3_T);
                                                    if ((SAV>THRESHOLD) && (_LeftLowArm3!="green")) {
                                                        _LeftLowArm3 = "green";
                                                        RaisePropertyChanged(() => LeftLowArm3);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftLowArm3!="red")) {
                                                        _LeftLowArm3 = "red";
                                                        RaisePropertyChanged(() => LeftLowArm3);
                                                    }
                                                }
                                                else if (Person3_RightLowArm.Contains(temp_EPC)) {
                                                    _RightLowArm3_T = DisplaySAV;
                                                    RaisePropertyChanged(() => RightLowArm3_T);
                                                    if ((SAV>THRESHOLD) && (_RightLowArm3!="green")) {
                                                        _RightLowArm3 = "green";
                                                        RaisePropertyChanged(() => RightLowArm3);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightLowArm3!="red")) {
                                                        _RightLowArm3 = "red";
                                                        RaisePropertyChanged(() => RightLowArm3);
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                            else { }
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
    
