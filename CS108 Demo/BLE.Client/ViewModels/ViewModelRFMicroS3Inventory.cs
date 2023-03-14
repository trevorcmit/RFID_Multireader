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

// For Live Plotting
// using LiveChartsCore;
// using LiveChartsCore.SkiaSharpView.XamarinForms;
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
            private uint _sucessCount;      public uint SucessCount { get { return this._sucessCount; } set { this.SetProperty(ref this._sucessCount, value); } }
            private string _DisplayName;    public string DisplayName { get { return this._DisplayName; } set { this.SetProperty(ref this._DisplayName, value); } }
            public RFMicroTagInfoViewModel() {}    // Class constructor (constructs nothing)
        }

        private readonly IUserDialogs _userDialogs;

        #region -------------- RFID inventory -----------------

        public ICommand OnStartInventoryButtonCommand { protected set; get; }
        public ICommand OnClearButtonCommand          { protected set; get; }
        public ICommand OnShareDataCommand            { protected set; get; }

        private ObservableCollection<RFMicroTagInfoViewModel> _TagInfoList = new ObservableCollection<RFMicroTagInfoViewModel>();
        public ObservableCollection<RFMicroTagInfoViewModel> TagInfoList { get { return _TagInfoList; } set { SetProperty(ref _TagInfoList, value); } }

        private string _startInventoryButtonText = "Start Inventory"; public string startInventoryButtonText { get { return _startInventoryButtonText; } }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////// For Saving Data / CSV exporting ///////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        List<string> tag_List = new List<string>();
        Dictionary<string, List<string>> tag_Time = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> tag_Data = new Dictionary<string, List<string>>();
        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
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

        public FileResult pick_result;  // Save FilePicker.PickAsync() result for use in Autosave function
  
        #endregion


        #region ------------- EPCs ----------------
        
        // Display Variables for the Beanie
        private string _Beanie_In; public string Beanie_In { get => _Beanie_In; set { _Beanie_In = value; OnPropertyChanged("Beanie_In"); } }
        private string _Beanie_In_T; public string Beanie_In_T { get => _Beanie_In_T; set { _Beanie_In_T = value; OnPropertyChanged("Beanie_In_T"); } }
        private string _Beanie_Out_T; public string Beanie_Out_T { get => _Beanie_Out_T; set { _Beanie_Out_T = value; OnPropertyChanged("Beanie_Out_T"); } }

        // Display Variables for Gloves
        private string _GloveL; public string GloveL { get => _GloveL; set { _GloveL = value; OnPropertyChanged("GloveL"); } }
        private string _GloveR; public string GloveR { get => _GloveR; set { _GloveR = value; OnPropertyChanged("GloveR"); } }
        private string _GloveInL_T; public string GloveInL_T { get => _GloveInL_T; set { _GloveInL_T = value; OnPropertyChanged("GloveInL_T"); } }
        private string _GloveInR_T; public string GloveInR_T { get => _GloveInR_T; set { _GloveInR_T = value; OnPropertyChanged("GloveInR_T"); } }
        private string _GloveOutL_T; public string GloveOutL_T { get => _GloveOutL_T; set { _GloveOutL_T = value; OnPropertyChanged("GloveOutL_T"); } }
        private string _GloveOutR_T; public string GloveOutR_T { get => _GloveOutR_T; set { _GloveOutR_T = value; OnPropertyChanged("GloveOutR_T"); } }

        // Display Variables for Pants
        private string _ThighL; public string ThighL { get => _ThighL; set { _ThighL = value; OnPropertyChanged("ThighL"); } }
        private string _ThighR; public string ThighR { get => _ThighR; set { _ThighR = value; OnPropertyChanged("ThighR"); } }
        private string _ThighLIn_T; public string ThighLIn_T { get => _ThighLIn_T; set { _ThighLIn_T = value; OnPropertyChanged("ThighLIn_T"); } }
        private string _ThighRIn_T; public string ThighRIn_T { get => _ThighRIn_T; set { _ThighRIn_T = value; OnPropertyChanged("ThighRIn_T"); } }
        private string _ThighLOut_T; public string ThighLOut_T { get => _ThighLOut_T; set { _ThighLOut_T = value; OnPropertyChanged("ThighLOut_T"); } }
        private string _ThighROut_T; public string ThighROut_T { get => _ThighROut_T; set { _ThighROut_T = value; OnPropertyChanged("ThighROut_T"); } }
        private string _CalfL; public string CalfL { get => _CalfL; set { _CalfL = value; OnPropertyChanged("CalfL"); } }
        private string _CalfR; public string CalfR { get => _CalfR; set { _CalfR = value; OnPropertyChanged("CalfR"); } }
        private string _CalfLIn_T; public string CalfLIn_T { get => _CalfLIn_T; set { _CalfLIn_T = value; OnPropertyChanged("CalfLIn_T"); } }
        private string _CalfRIn_T; public string CalfRIn_T { get => _CalfRIn_T; set { _CalfRIn_T = value; OnPropertyChanged("CalfRIn_T"); } }
        private string _CalfLOut_T; public string CalfLOut_T { get => _CalfLOut_T; set { _CalfLOut_T = value; OnPropertyChanged("CalfLOut_T"); } }
        private string _CalfROut_T; public string CalfROut_T { get => _CalfROut_T; set { _CalfROut_T = value; OnPropertyChanged("CalfROut_T"); } }

        // Display Variables for Shirt
        private string _Chest; public string Chest { get => _Chest; set { _Chest = value; OnPropertyChanged("Chest"); } }
        private string _ChestIn_T; public string ChestIn_T { get => _ChestIn_T; set { _ChestIn_T = value; OnPropertyChanged("ChestIn_T"); } }
        private string _ChestOut_T; public string ChestOut_T { get => _ChestOut_T; set { _ChestOut_T = value; OnPropertyChanged("ChestOut_T"); } }
        private string _Abs; public string Abs { get => _Abs; set { _Abs = value; OnPropertyChanged("Abs"); } }
        private string _AbIn_T; public string AbIn_T { get => _AbIn_T; set { _AbIn_T = value; OnPropertyChanged("AbIn_T"); } }
        private string _AbOut_T; public string AbOut_T { get => _AbOut_T; set { _AbOut_T = value; OnPropertyChanged("AbOut_T"); } }
        private string _RightLow; public string RightLow { get => _RightLow; set { _RightLow = value; OnPropertyChanged("RightLow"); } }
        private string _RightLowIn_T; public string RightLowIn_T { get => _RightLowIn_T; set { _RightLowIn_T = value; OnPropertyChanged("RightLowIn_T"); } }
        private string _RightLowOut_T; public string RightLowOut_T { get => _RightLowOut_T; set { _RightLowOut_T = value; OnPropertyChanged("RightLowOut_T"); } }
        private string _LeftLow; public string LeftLow { get => _LeftLow; set { _LeftLow = value; OnPropertyChanged("LeftLow"); } }
        private string _LeftLowIn_T; public string LeftLowIn_T { get => _LeftLowIn_T; set { _LeftLowIn_T = value; OnPropertyChanged("LeftLowIn_T"); } }
        private string _LeftLowOut_T; public string LeftLowOut_T { get => _LeftLowOut_T; set { _LeftLowOut_T = value; OnPropertyChanged("LeftLowOut_T"); } }
        private string _RightUp; public string RightUp { get => _RightUp; set { _RightUp = value; OnPropertyChanged("RightUp"); } }
        private string _RightUpIn_T; public string RightUpIn_T { get => _RightUpIn_T; set { _RightUpIn_T = value; OnPropertyChanged("RightUpIn_T"); } }
        private string _RightUpOut_T; public string RightUpOut_T { get => _RightUpOut_T; set { _RightUpOut_T = value; OnPropertyChanged("RightUpOut_T"); } }
        private string _LeftUp; public string LeftUp { get => _LeftUp; set { _LeftUp = value; OnPropertyChanged("LeftUp"); } }
        private string _LeftUpIn_T; public string LeftUpIn_T { get => _LeftUpIn_T; set { _LeftUpIn_T = value; OnPropertyChanged("LeftUpIn_T"); } }
        private string _LeftUpOut_T; public string LeftUpOut_T { get => _LeftUpOut_T; set { _LeftUpOut_T = value; OnPropertyChanged("LeftUpOut_T"); } }

        // Display Variables for Socks
        private string _SockL; public string SockL { get => _SockL; set { _SockL = value; OnPropertyChanged("SockL"); } }
        private string _SockR; public string SockR { get => _SockR; set { _SockR = value; OnPropertyChanged("SockR"); } }
        private string _SockInR_T; public string SockInR_T { get => _SockInR_T; set { _SockInR_T = value; OnPropertyChanged("SockInR_T"); } }
        private string _SockOutR_T; public string SockOutR_T { get => _SockOutR_T; set { _SockOutR_T = value; OnPropertyChanged("SockOutR_T"); } }
        private string _SockInL_T; public string SockInL_T { get => _SockInL_T; set { _SockInL_T = value; OnPropertyChanged("SockInL_T"); } }
        private string _SockOutL_T; public string SockOutL_T { get => _SockOutL_T; set { _SockOutL_T = value; OnPropertyChanged("SockOutL_T"); } }

        // Display Variables for Scapula
        private string _ScapLeft; public string ScapLeft { get => _ScapLeft; set { _ScapLeft = value; OnPropertyChanged("ScapLeft"); } }
        private string _ScapRight; public string ScapRight { get => _ScapRight; set { _ScapRight = value; OnPropertyChanged("ScapRight"); } }
        private string _ScapLeftIn_T; public string ScapLeftIn_T { get => _ScapLeftIn_T; set { _ScapLeftIn_T = value; OnPropertyChanged("ScapLeftIn_T"); } }
        private string _ScapLeftOut_T; public string ScapLeftOut_T { get => _ScapLeftOut_T; set { _ScapLeftOut_T = value; OnPropertyChanged("ScapLeftOut_T"); } }
        private string _ScapRightIn_T; public string ScapRightIn_T { get => _ScapRightIn_T; set { _ScapRightIn_T = value; OnPropertyChanged("ScapRightIn_T"); } }
        private string _ScapRightOut_T; public string ScapRightOut_T { get => _ScapRightOut_T; set { _ScapRightOut_T = value; OnPropertyChanged("ScapRightOut_T"); } }

        public int THRESHOLD = 15;

        class Glove {
            public string DorsalLeftIn { get; set; }
            public string DorsalLeftOut { get; set; }
            public string DorsalRightIn { get; set; }
            public string DorsalRightOut { get; set; }
            public string FourthLeftIn { get; set; }
            public string FourthLeftOut { get; set; }
            public string FourthRightIn { get; set; }
            public string FourthRightOut { get; set; }
            public List<string> TagList { get; set; }
            public Glove(
                string dorsal_left_in, string dorsal_left_out, string dorsal_right_in, string dorsal_right_out, 
                string fourth_left_in, string fourth_left_out, string fourth_right_in, string fourth_right_out
            ) {
                DorsalLeftIn = dorsal_left_in;
                DorsalLeftOut = dorsal_left_out;
                DorsalRightIn = dorsal_right_in;
                DorsalRightOut = dorsal_right_out;
                FourthLeftIn = fourth_left_in;
                FourthLeftOut = fourth_left_out;
                FourthRightIn = fourth_right_in;
                FourthRightOut = fourth_right_out;
                TagList = new List<string> { dorsal_left_in, dorsal_left_out, dorsal_right_in, dorsal_right_out, fourth_left_in, fourth_left_out, fourth_right_in, fourth_right_out };
            }
        }

        Glove glove1 = new Glove("6091", "147F", "2AC0", "637B", "43C3", "83B0", "63D4", "3B5C");
        Glove glove2 = new Glove("----", "2C4A", "91AA", "2693", "----", "----", "----", "----");
        Glove glove3 = new Glove("----", "6344", "----", "62A5", "----", "----", "----", "----");
        Glove glove4 = new Glove("----", "1776", "----", "9AB2", "----", "----", "----", "----");
        Glove glove5 = new Glove("----", "9722", "----", "9A48", "----", "----", "----", "----");
        Glove glove6 = new Glove("----", "3E5D", "----", "2E3B", "----", "----", "----", "----");
        Glove glove7 = new Glove("----", "A959", "----", "4202", "----", "----", "----", "----");
        Glove glove8 = new Glove("----", "91D2", "----", "4B37", "----", "----", "----", "----");

        class Shirt {
            public string Chest_In { get; set; }
            public string RightUp_In { get; set; }
            public string LeftUp_In { get; set; }
            public string RightLow_In { get; set; }
            public string LeftLow_In { get; set; }
            public string Abdomen_In { get; set; }
            public string Chest_Out { get; set; }
            public string RightUp_Out { get; set; }
            public string LeftUp_Out { get; set; }
            public string RightLow_Out { get; set; }
            public string LeftLow_Out { get; set; }
            public string Abdomen_Out { get; set; }
            public string SubscapLeft_In { get; set; }
            public string SubscapLeft_Out { get; set; }
            public string SubscapRight_In { get; set; }
            public string SubscapRight_Out { get; set; }
            public List<string> TagList { get; set; }
            public Shirt(
                string ss_l_out, string ss_l_in, string ss_r_out, string ss_r_in,
                string chest_out, string chest_in, string ab_out, string ab_in, string ru_out, string ru_in,
                string rl_out, string rl_in, string lu_out, string lu_in, string ll_out, string ll_in
            ) {
                SubscapLeft_Out = ss_l_out;
                SubscapLeft_In = ss_l_in;
                SubscapRight_Out = ss_r_out;
                SubscapRight_In = ss_r_in;
                Chest_In = chest_in;
                RightUp_In = ru_in;
                LeftUp_In = lu_in;
                RightLow_In = rl_in;
                LeftLow_In = ll_in;
                Abdomen_In = ab_in;
                Chest_Out = chest_out;
                RightUp_Out = ru_out;
                LeftUp_Out = lu_out;
                RightLow_Out = rl_out;
                LeftLow_Out = ll_out;
                Abdomen_Out = ab_out;
                TagList = new List<string> {
                    SubscapLeft_Out, SubscapLeft_In, SubscapRight_Out, SubscapRight_In,
                    Chest_In, RightUp_In, LeftUp_In, RightLow_In, LeftLow_In, Abdomen_In, 
                    Chest_Out, RightUp_Out, LeftUp_Out, RightLow_Out, LeftLow_Out, Abdomen_Out
                };
            }
        }

        Shirt shirt1 = new Shirt("61D8", "2BC6", "427A", "0F7D", "759A", "8099", "7198", "9A80", "94B4", "896D", "80D2", "79D3", "8265", "98CC", "7852", "4E7A");
        Shirt shirt2 = new Shirt("B57A", "088C", "8853", "5471", "0790", "3DBA", "9E3B", "8F77", "6065", "8FA4", "077C", "A035", "A831", "5782", "9B7D", "0A4A");
        Shirt shirt3 = new Shirt("1591", "8C56", "66C9", "1364", "6D2E", "0B61", "1E72", "1B7E", "5C34", "265C", "5E8F", "3DCF", "92C1", "2371", "7181", "2774");
        Shirt shirt4 = new Shirt("6FD7", "62CA", "----", "056D", "6ACF", "4743", "1650", "37BB", "2CC8", "424E", "7EB3", "2C98", "3750", "5713", "4FDC", "B0A2");
        Shirt shirt5 = new Shirt("1E9E", "3644", "0086", "3325", "099B", "5577", "2D1C", "5352", "855B", "3C4F", "8E8D", "7CD4", "2A7A", "3B75", "1CBB", "9941");
        Shirt shirt6 = new Shirt("B4A6", "7D23", "41C1", "4780", "AB82", "98C0", "067B", "734B", "41AC", "BFA4", "8938", "B142", "A3BC", "1C2E", "7BDA", "8917");
        Shirt shirt7 = new Shirt("9353", "7E20", "2E4F", "60D1", "7B06", "4EE3", "3318", "6415", "OD7C", "1EB3", "8A72", "2D69", "1F90", "77DE", "2850", "9F6B");
        Shirt shirt8 = new Shirt("277D", "77B5", "0859", "431C", "3869", "833E", "774F", "2B45", "483D", "64A5", "0E92", "AC75", "737F", "675C", "2567", "B76E");

        class Pants {
            public string Thigh_In_L { get; set; }
            public string Thigh_Out_L { get; set; }
            public string Thigh_Out_R { get; set; }
            public string Thigh_In_R { get; set; }
            public string Calf_In_L { get; set; }
            public string Calf_Out_L { get; set; }
            public string Calf_Out_R { get; set; }
            public string Calf_In_R { get; set; }
            public List<string> TagList { get; set; }
            public Pants(string til, string tol, string tir, string tor, string cil, string col, string cir, string cor) {
                Thigh_In_L = til;
                Thigh_Out_L = tol;
                Thigh_In_R = tir;
                Thigh_Out_R = tor;
                Calf_In_L = cil;
                Calf_Out_L = col;
                Calf_In_R = cir;
                Calf_Out_R = cor;
                TagList = new List<string> {
                    Thigh_In_L, Thigh_Out_L, Thigh_In_R, Thigh_Out_R, Calf_In_L, Calf_Out_L, Calf_In_R, Calf_Out_R
                };
            }
        }

        Pants pants1 = new Pants("409D", "1734", "A389", "AC37", "5439", "508F", "67A1", "701B");
        Pants pants2 = new Pants("8992", "AB37", "5E5E", "9873", "846F", "592A", "8220", "6227");
        Pants pants3 = new Pants("9D63", "4465", "6BA1", "2445", "1D74", "3A8A", "726C", "8C46");
        Pants pants4 = new Pants("----", "----", "----", "----", "----", "----", "----", "----");
        Pants pants5 = new Pants("----", "----", "----", "----", "----", "----", "----", "----");
        Pants pants6 = new Pants("----", "----", "----", "----", "----", "----", "----", "----");
        Pants pants7 = new Pants("----", "----", "----", "----", "----", "----", "----", "----");
        Pants pants8 = new Pants("----", "----", "----", "----", "----", "----", "----", "----");

        class Sock {
            public string Above_In { get; set; }
            public string Above_Out { get; set; }
            public string Toes { get; set; }
            public List<string> TagList { get; set; }
            public Sock(string ai, string ao, string t) {
                Above_In = ai;
                Above_Out = ao;
                Toes = t;
                TagList = new List<string> { Above_In, Above_Out, Toes };
            }
        }

        Sock sock1 = new Sock("----", "----", "7D30");
        Sock sock2 = new Sock("----", "----", "B27A");
        Sock sock3 = new Sock("----", "----", "----");
        Sock sock4 = new Sock("----", "----", "----");
        Sock sock5 = new Sock("----", "----", "----");
        Sock sock6 = new Sock("----", "----", "----");
        Sock sock7 = new Sock("----", "----", "----");
        Sock sock8 = new Sock("----", "----", "----");

        class Beanie {
            public string Forehead_In { get; set; }
            public string Forehead_Out { get; set; }
            public List<string> TagList { get; set; }
            public Beanie(string fi, string fo) {
                Forehead_In = fi;
                Forehead_Out = fo;
                TagList = new List<string> { Forehead_In, Forehead_Out };
            }
        }
        
        Beanie beanie1 = new Beanie("624F", "3F15");
        Beanie beanie2 = new Beanie("3B3C", "----");
        Beanie beanie3 = new Beanie("----", "----");
        Beanie beanie4 = new Beanie("----", "----");
        Beanie beanie5 = new Beanie("----", "----");
        Beanie beanie6 = new Beanie("----", "----");
        Beanie beanie7 = new Beanie("----", "----");
        Beanie beanie8 = new Beanie("----", "----");

        private List<string> _BeaniePicker; public List<string> BeaniePicker { get => _BeaniePicker; set { _BeaniePicker = value; OnPropertyChanged("BeaniePicker"); } }
        private List<string> _ShirtPicker; public List<string> ShirtPicker { get => _ShirtPicker; set { _ShirtPicker = value; OnPropertyChanged("ShirtPicker"); } }
        private List<string> _PantsPicker; public List<string> PantsPicker { get => _PantsPicker; set { _PantsPicker = value; OnPropertyChanged("PantsPicker"); } }
        private List<string> _SockPicker; public List<string> SockPicker { get => _SockPicker; set { _SockPicker = value; OnPropertyChanged("SockPicker"); } }
        private List<string> _GlovePicker; public List<string> GlovePicker { get => _GlovePicker; set { _GlovePicker = value; OnPropertyChanged("GlovePicker"); } }

        private int _SelectBeanie; 
        public int SelectBeanie {
            get => _SelectBeanie;
            set { 
                _SelectBeanie = value; 
                OnPropertyChanged("SelectBeanie");
                _Beanie_In_T  = "--"; RaisePropertyChanged(() => Beanie_In_T);
                _Beanie_Out_T = "--"; RaisePropertyChanged(() => Beanie_Out_T);
                _Beanie_In    = "gray"; RaisePropertyChanged(() => Beanie_In);
            }
        }
        private int _SelectShirt;
        public int SelectShirt {
            get => _SelectShirt;
            set {
                _SelectShirt = value;
                OnPropertyChanged("SelectShirt");
                _Chest = "gray";     RaisePropertyChanged(() => Chest);
                _RightUp = "gray";   RaisePropertyChanged(() => RightUp);
                _LeftUp = "gray";    RaisePropertyChanged(() => LeftUp);
                _RightLow = "gray";  RaisePropertyChanged(() => RightLow);
                _LeftLow = "gray";   RaisePropertyChanged(() => LeftLow);
                _Abs = "gray";       RaisePropertyChanged(() => Abs);
                _ScapLeft = "gray";  RaisePropertyChanged(() => ScapLeft);
                _ScapRight = "gray"; RaisePropertyChanged(() => ScapRight);
                _ChestIn_T = "--";     RaisePropertyChanged(() => ChestIn_T);
                _RightUpIn_T = "--";   RaisePropertyChanged(() => RightUpIn_T);
                _LeftUpIn_T = "--";    RaisePropertyChanged(() => LeftUpIn_T);
                _RightLowIn_T = "--";  RaisePropertyChanged(() => RightLowIn_T);
                _LeftLowIn_T = "--";   RaisePropertyChanged(() => LeftLowIn_T);
                _AbIn_T = "--";        RaisePropertyChanged(() => AbIn_T);
                _ChestOut_T = "--";    RaisePropertyChanged(() => ChestOut_T);
                _RightUpOut_T = "--";  RaisePropertyChanged(() => RightUpOut_T);
                _LeftUpOut_T = "--";   RaisePropertyChanged(() => LeftUpOut_T);
                _RightLowOut_T = "--"; RaisePropertyChanged(() => RightLowOut_T);
                _LeftLowOut_T = "--";  RaisePropertyChanged(() => LeftLowOut_T);
                _AbOut_T = "--";       RaisePropertyChanged(() => AbOut_T);
                _ScapLeftIn_T = "--";  RaisePropertyChanged(() => ScapLeftIn_T);
                _ScapRightIn_T = "--"; RaisePropertyChanged(() => ScapRightIn_T);
                _ScapLeftOut_T = "--"; RaisePropertyChanged(() => ScapLeftOut_T);
                _ScapRightOut_T = "--";RaisePropertyChanged(() => ScapRightOut_T);
            }
        }
        private int _SelectPants;
        public int SelectPants {
            get => _SelectPants;
            set {
                _SelectPants = value;
                OnPropertyChanged("SelectPants");
                _ThighL = "gray"; RaisePropertyChanged(() => ThighL);
                _ThighR = "gray"; RaisePropertyChanged(() => ThighR);
                _CalfL  = "gray"; RaisePropertyChanged(() => CalfL);
                _CalfR  = "gray"; RaisePropertyChanged(() => CalfR);
                _ThighLIn_T = "--"; RaisePropertyChanged(() => ThighLIn_T);
                _ThighRIn_T = "--"; RaisePropertyChanged(() => ThighRIn_T);
                _CalfLIn_T  = "--"; RaisePropertyChanged(() => CalfLIn_T);
                _CalfRIn_T  = "--"; RaisePropertyChanged(() => CalfRIn_T);
                _ThighLOut_T = "--"; RaisePropertyChanged(() => ThighLOut_T);
                _ThighROut_T = "--"; RaisePropertyChanged(() => ThighROut_T);
                _CalfLOut_T  = "--"; RaisePropertyChanged(() => CalfLOut_T);
                _CalfROut_T  = "--"; RaisePropertyChanged(() => CalfROut_T);
            }
        }
        private int _SelectSock;
        public int SelectSock {
            get => _SelectSock;
            set {
                _SelectSock = value;
                OnPropertyChanged("SelectSock");
                _SockL = "gray"; RaisePropertyChanged(() => SockL);
                _SockR = "gray"; RaisePropertyChanged(() => SockR);
                _SockInL_T  = "--"; RaisePropertyChanged(() => SockInL_T);
                _SockOutL_T = "--"; RaisePropertyChanged(() => SockOutL_T);
                _SockInR_T  = "--"; RaisePropertyChanged(() => SockInR_T);
                _SockOutR_T = "--"; RaisePropertyChanged(() => SockOutR_T);
            }
        }
        private int _SelectGlove;
        public int SelectGlove {
            get => _SelectGlove;
            set {
                _SelectGlove = value;
                OnPropertyChanged("SelectGlove");
                _GloveL = "gray"; RaisePropertyChanged(() => GloveL);
                _GloveR = "gray"; RaisePropertyChanged(() => GloveR);
                _GloveInL_T  = "--"; RaisePropertyChanged(() => GloveInL_T);
                _GloveOutL_T = "--"; RaisePropertyChanged(() => GloveOutL_T);
                _GloveInR_T  = "--"; RaisePropertyChanged(() => GloveInR_T);
                _GloveOutR_T = "--"; RaisePropertyChanged(() => GloveOutR_T);
            }
        }

        private string _DebugVar; public string DebugVar { get => _DebugVar; set { _DebugVar = value; OnPropertyChanged("DebugVar"); } }

        Dictionary<int, Shirt> shirts = new Dictionary<int, Shirt>();
        Dictionary<int, Beanie> beanies = new Dictionary<int, Beanie>();
        Dictionary<int, Pants> pants = new Dictionary<int, Pants>();
        Dictionary<int, Sock> socks = new Dictionary<int, Sock>();
        Dictionary<int, Glove> gloves = new Dictionary<int, Glove>();

        public Random rnd = new Random();
        public int r;

        #endregion



        public ViewModelRFMicroS3Inventory(IAdapter adapter, IUserDialogs userDialogs) : base(adapter) {
            _userDialogs = userDialogs;
            r = rnd.Next(10000, 99999);

            _SelectBeanie = 0; RaisePropertyChanged(() => SelectBeanie);
            _SelectShirt = 0; RaisePropertyChanged(() => SelectShirt);
            _SelectPants = 0; RaisePropertyChanged(() => SelectPants);
            _SelectSock = 0; RaisePropertyChanged(() => SelectSock);
            _SelectGlove = 0; RaisePropertyChanged(() => SelectGlove);

            Beanie_In = "gray"; Beanie_In_T = "--"; Beanie_Out_T = "--";

            LeftLow  = "gray"; LeftLowIn_T  = "--"; LeftLowOut_T  = "--";
            RightLow = "gray"; RightLowIn_T = "--"; RightLowOut_T = "--";
            RightUp  = "gray"; RightUpIn_T  = "--"; RightUpOut_T  = "--";
            LeftUp   = "gray"; LeftUpIn_T   = "--"; LeftUpOut_T   = "--";
            Chest    = "gray"; ChestIn_T    = "--"; ChestOut_T    = "--";
            Abs      = "gray"; AbIn_T       = "--"; AbOut_T       = "--";

            ThighL = "gray"; ThighLIn_T = "--"; ThighLOut_T = "--";
            ThighR = "gray"; ThighRIn_T = "--"; ThighROut_T = "--";
            CalfL = "gray";  CalfLIn_T = "--";  CalfLOut_T = "--";
            CalfR = "gray";  CalfRIn_T = "--";  CalfROut_T = "--";

            SockL = "gray"; SockInL_T  = "--"; SockOutL_T = "--";
            SockR = "gray"; SockInR_T  = "--"; SockOutR_T = "--";

            GloveL = "gray";
            GloveR = "gray";
            GloveInL_T  = "--";
            GloveOutL_T = "--";
            GloveInR_T  = "--";
            GloveOutR_T = "--";

            ScapLeft = "gray"; ScapLeftIn_T = "--"; ScapLeftOut_T = "--";
            ScapRight = "gray";
            ScapRightIn_T = "--";
            ScapRightOut_T = "--";

            shirts = new Dictionary<int, Shirt> {
                {0, shirt1}, {1, shirt2}, {2, shirt3}, {3, shirt4}, 
                {4, shirt5}, {5, shirt6}, {6, shirt7}, {7, shirt8}   
            };

            beanies = new Dictionary<int, Beanie> {
                {0, beanie1}, {1, beanie2}, {2, beanie3}, {3, beanie4}, 
                {4, beanie5}, {5, beanie6}, {6, beanie7}, {7, beanie8}   
            };

            gloves = new Dictionary<int, Glove> {
                {0, glove1}, {1, glove2}, {2, glove3}, {3, glove4}, 
                {4, glove5}, {5, glove6}, {6, glove7}, {7, glove8}   
            };

            socks = new Dictionary<int, Sock> {
                {0, sock1}, {1, sock2}, {2, sock3}, {3, sock4}, 
                {4, sock5}, {5, sock6}, {6, sock7}, {7, sock8}   
            };

            pants = new Dictionary<int, Pants> {
                {0, pants1}, {1, pants2}, {2, pants3}, {3, pants4}, 
                {4, pants5}, {5, pants6}, {6, pants7}, {7, pants8}   
            };

            // Set disconnection event for reconnection
            Adapter.DeviceDisconnected += OnDeviceDisconnected;
            Adapter.DeviceConnectionLost += OnDeviceDisconnected; // connection or disconnect?

            GetTimes();  // Get Duty Cycle Times

            _BeaniePicker = new List<string> { "Cap 1", "Cap 2", "Cap 3", "Cap 4", "Cap 5", "Cap 6", "Cap 7", "Cap 8" };
            _ShirtPicker  = new List<string> { "Shirt 1", "Shirt 2", "Shirt 3", "Shirt 4", "Shirt 5", "Shirt 6", "Shirt 7", "Shirt 8" };
            _PantsPicker  = new List<string> { "Pants 1", "Pants 2", "Pants 3", "Pants 4", "Pants 5", "Pants 6", "Pants 7", "Pants 8" };
            _SockPicker   = new List<string> { "Sock 1", "Sock 2", "Sock 3", "Sock 4", "Sock 5", "Sock 6", "Sock 7", "Sock 8" };
            _GlovePicker  = new List<string> { "Glove 1", "Glove 2", "Glove 3", "Glove 4", "Glove 5", "Glove 6", "Glove 7", "Glove 8" };

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
            string x = await BleMvxApplication._reader.DisconnectAsync();

            _DebugVar = x;
            RaisePropertyChanged(() => DebugVar);

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
            // pick_result = await FilePicker.PickAsync();

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
                            _userDialogs.Alert("Too close to metal! Please move CS108 away from metal and try again.");
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
                                        ///////////////////////////////
                                        TagInfoList[cnt].SucessCount++;
                                        ///////////////////////////////

                                        double SAV = Math.Round(getTempC(temp, caldata), 2);   
                                        string DisplaySAV = Math.Round(SAV, 2).ToString();
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

                                            Shirt s1 = shirts[SelectShirt];
                                            Beanie b1 = beanies[SelectBeanie];
                                            Glove g1 = gloves[SelectGlove];
                                            Sock sk1 = socks[SelectSock];

                                            if (s1.TagList.Contains(tEPC)) {
                                                if (tEPC==s1.Chest_In) {
                                                    _ChestIn_T = DisplaySAV; RaisePropertyChanged(() => ChestIn_T);
                                                    if ((SAV>THRESHOLD) && (_Chest!="green")) {
                                                        _Chest = "green"; RaisePropertyChanged(() => Chest);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_Chest!="red")) {
                                                        _Chest = "red"; RaisePropertyChanged(() => Chest);
                                                    }
                                                }
                                                else if (tEPC==s1.Chest_Out) {
                                                    _ChestOut_T = DisplaySAV; RaisePropertyChanged(() => ChestOut_T);
                                                }

                                                else if (tEPC==s1.Abdomen_In) {
                                                    _AbIn_T = DisplaySAV; RaisePropertyChanged(() => AbIn_T);
                                                    if ((SAV>THRESHOLD) && (_Abs!="green")) {
                                                        _Abs = "green"; RaisePropertyChanged(() => Abs);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_Abs!="red")) {
                                                        _Abs = "red"; RaisePropertyChanged(() => Abs);
                                                    }
                                                }
                                                else if (tEPC==s1.Abdomen_Out) {
                                                    _AbOut_T = DisplaySAV; RaisePropertyChanged(() => AbOut_T);
                                                }

                                                else if (tEPC==s1.LeftUp_In) {
                                                    _LeftUpIn_T = DisplaySAV; RaisePropertyChanged(() => LeftUpIn_T);
                                                    if ((SAV>THRESHOLD) && (_LeftUp!="green")) {
                                                        _LeftUp = "green"; RaisePropertyChanged(() => LeftUp);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftUp!="red")) {
                                                        _LeftUp = "red"; RaisePropertyChanged(() => LeftUp);
                                                    }
                                                }
                                                else if (tEPC==s1.LeftUp_Out) {
                                                    _LeftUpOut_T = DisplaySAV; RaisePropertyChanged(() => LeftUpOut_T);
                                                }

                                                else if (tEPC==s1.LeftLow_In) {
                                                    _LeftLowIn_T = DisplaySAV; RaisePropertyChanged(() => LeftLowIn_T);
                                                    if ((SAV>THRESHOLD) && (_LeftLow!="green")) {
                                                        _LeftLow = "green"; RaisePropertyChanged(() => LeftLow);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftLow!="red")) {
                                                        _LeftLow = "red"; RaisePropertyChanged(() => LeftLow);
                                                    }
                                                }
                                                else if (tEPC==s1.LeftLow_Out) {
                                                    _LeftLowOut_T = DisplaySAV; RaisePropertyChanged(() => LeftLowOut_T);
                                                }

                                                else if (tEPC==s1.RightUp_In) {
                                                    _RightUpIn_T = DisplaySAV; RaisePropertyChanged(() => RightUpIn_T);
                                                    if ((SAV>THRESHOLD) && (_RightUp!="green")) {
                                                        _RightUp = "green"; RaisePropertyChanged(() => RightUp);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightUp!="red")) {
                                                        _RightUp = "red"; RaisePropertyChanged(() => RightUp);
                                                    }
                                                }
                                                else if (tEPC==s1.RightUp_Out) {
                                                    _RightUpOut_T = DisplaySAV; RaisePropertyChanged(() => RightUpOut_T);
                                                }

                                                else if (tEPC==s1.RightLow_In) {
                                                    _RightLowIn_T = DisplaySAV; RaisePropertyChanged(() => RightLowIn_T);
                                                    if ((SAV>THRESHOLD) && (_RightLow!="green")) {
                                                        _RightLow = "green"; RaisePropertyChanged(() => RightLow);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightLow!="red")) {
                                                        _RightLow = "red"; RaisePropertyChanged(() => RightLow);
                                                    }
                                                }
                                                else if (tEPC==s1.RightLow_Out) {
                                                    _RightLowOut_T = DisplaySAV; RaisePropertyChanged(() => RightLowOut_T);
                                                }

                                                else if (tEPC==s1.SubscapLeft_In) {
                                                    _ScapLeftIn_T = DisplaySAV; RaisePropertyChanged(() => ScapLeftIn_T);
                                                    if ((SAV>THRESHOLD) && (_ScapLeft!="green")) {
                                                        _ScapLeft = "green"; RaisePropertyChanged(() => ScapLeft);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_ScapLeft!="red")) {
                                                        _ScapLeft = "red"; RaisePropertyChanged(() => ScapLeft);
                                                    }
                                                }
                                                else if (tEPC==s1.SubscapLeft_Out) {
                                                    _ScapLeftOut_T = DisplaySAV; RaisePropertyChanged(() => ScapLeftOut_T);
                                                }

                                                else if (tEPC==s1.SubscapRight_In) {
                                                    _ScapRightIn_T = DisplaySAV; RaisePropertyChanged(() => ScapRightIn_T);
                                                    if ((SAV>THRESHOLD) && (_ScapRight!="green")) {
                                                        _ScapRight = "green"; RaisePropertyChanged(() => ScapRight);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_ScapRight!="red")) {
                                                        _ScapRight = "red"; RaisePropertyChanged(() => ScapRight);
                                                    }
                                                }
                                                else if (tEPC==s1.SubscapRight_Out) {
                                                    _ScapRightOut_T = DisplaySAV; RaisePropertyChanged(() => ScapRightOut_T);
                                                }
                                            }

                                            else if (g1.TagList.Contains(tEPC)) {
                                                if (tEPC==g1.DorsalLeftIn) {
                                                    _GloveInL_T = DisplaySAV; RaisePropertyChanged(() => GloveInL_T);
                                                    if ((SAV>THRESHOLD) && (_GloveL!="green")) {
                                                        _GloveL = "green"; RaisePropertyChanged(() => GloveL);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_GloveL!="red")) {
                                                        _GloveL = "red"; RaisePropertyChanged(() => GloveL);
                                                    }
                                                }
                                                else if (tEPC==g1.DorsalLeftOut) {
                                                    _GloveOutL_T = DisplaySAV; RaisePropertyChanged(() => GloveOutL_T);
                                                }
                                                else if (tEPC==g1.DorsalRightIn) {
                                                    _GloveInR_T = DisplaySAV; RaisePropertyChanged(() => GloveInR_T);
                                                    if ((SAV>THRESHOLD) && (_GloveR!="green")) {
                                                        _GloveR = "green"; RaisePropertyChanged(() => GloveR);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_GloveR!="red")) {
                                                        _GloveR = "red"; RaisePropertyChanged(() => GloveR);
                                                    }
                                                }
                                                else if (tEPC==g1.DorsalRightOut) {
                                                    _GloveOutR_T = DisplaySAV; RaisePropertyChanged(() => GloveOutR_T);
                                                }
                                            }

                                            else if (b1.TagList.Contains(tEPC)) {
                                                if (tEPC==b1.Forehead_In) {
                                                    _Beanie_In_T = DisplaySAV; RaisePropertyChanged(() => Beanie_In_T);
                                                    if ((SAV>THRESHOLD) && (_Beanie_In!="green")) {
                                                        _Beanie_In = "green"; RaisePropertyChanged(() => Beanie_In);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_Beanie_In!="red")) {
                                                        _Beanie_In = "red"; RaisePropertyChanged(() => Beanie_In);
                                                    }
                                                }
                                                else if (tEPC==b1.Forehead_Out) {
                                                    _Beanie_Out_T = DisplaySAV; RaisePropertyChanged(() => Beanie_Out_T);
                                                }
                                            }


                                        } // end of Try/Finally block

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
                        item.SucessCount = 0;
                        item.DisplayName = item.EPC;

                        if (ocRSSI >= BleMvxApplication._rfMicro_minOCRSSI && ocRSSI <= BleMvxApplication._rfMicro_maxOCRSSI) {
                            if (temp>=1300 && temp<=3500) {
                                UInt64 caldata = (UInt64)(((UInt64)info.Bank2Data[0] << 48) | ((UInt64)info.Bank2Data[1] << 32) | ((UInt64)info.Bank2Data[2] << 16) | ((UInt64)info.Bank2Data[3]));

                                if (caldata==0) { item.SensorAvgValue = "NoCalData"; }
                                else {
                                    ///////////////////
                                    item.SucessCount++;
                                    ///////////////////

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
                                    finally { }
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

        public string fpath;

        private void AutoSaveData() {    // Function for Sharing time series data from tags
            InvokeOnMainThread(()=> {
                string fpath = "tags_" + r.ToString() + ".csv";

                // string fileName = pick_result.FullPath;    // Get file name from picker
                string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fpath);
                // for UWP cannot use filepicker, use local folder instead

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
    
