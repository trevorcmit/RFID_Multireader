﻿using Acr.UserDialogs;
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


        public int THRESHOLD = 15;

        class Glove {
            public string DorsalLeft { get; set; }
            public string DorsalRight { get; set; }
            public string FourthLeft { get; set; }
            public string FourthRight { get; set; }
            public List<string> TagList { get; set; }
            public Glove(string dl, string dr, string fl, string fr) {
                DorsalLeft = dl;
                DorsalRight = dr;
                FourthLeft = fl;
                FourthRight = fr;
                TagList = new List<string> { DorsalLeft, DorsalRight, FourthLeft, FourthRight };
            }
        }

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
            public List<string> TagList { get; set; }
            public Shirt(
                string ci, string rui, string lui, string rli, string lli, string ai,
                string co, string ruo, string luo, string rlo, string llo, string ao
            ) {
                Chest_In = ci;
                RightUp_In = rui;
                LeftUp_In = lui;
                RightLow_In = rli;
                LeftLow_In = lli;
                Abdomen_In = ai;
                Chest_Out = co;
                RightUp_Out = ruo;
                LeftUp_Out = luo;
                RightLow_Out = rlo;
                LeftLow_Out = llo;
                Abdomen_Out = ao;
                TagList = new List<string> {
                    Chest_In, RightUp_In, LeftUp_In, RightLow_In, LeftLow_In, Abdomen_In, 
                    Chest_Out, RightUp_Out, LeftUp_Out, RightLow_Out, LeftLow_Out, Abdomen_Out
                };
            }
        }
        
        class Pants {
            public string Thigh_In { get; set; }
            public string Thigh_Out { get; set; }
            public string Calf_In { get; set; }
            public string Calf_Out { get; set; }
            public List<string> TagList { get; set; }
            public Pants(string ti, string to, string ci, string co) {
                Thigh_In = ti;
                Thigh_Out = to;
                Calf_In = ci;
                Calf_Out = co;
                TagList = new List<string> { Thigh_In, Thigh_Out, Calf_In, Calf_Out };
            }
        }

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
                _Chest = "gray";    RaisePropertyChanged(() => Chest);
                _RightUp = "gray";  RaisePropertyChanged(() => RightUp);
                _LeftUp = "gray";   RaisePropertyChanged(() => LeftUp);
                _RightLow = "gray"; RaisePropertyChanged(() => RightLow);
                _LeftLow = "gray";  RaisePropertyChanged(() => LeftLow);
                _Abs = "gray";      RaisePropertyChanged(() => Abs);
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

        #endregion



        public ViewModelRFMicroS3Inventory(IAdapter adapter, IUserDialogs userDialogs) : base(adapter) {
            _userDialogs = userDialogs;

            Beanie_In = "gray";

            Chest = "gray";
            Abs = "gray";
            RightUp = "gray";
            LeftUp = "gray";
            LeftLow = "gray";
            RightLow = "gray";

            ThighL = "gray";
            ThighR = "gray";
            CalfL = "gray";
            CalfR = "gray";

            SockL = "gray";
            SockR = "gray";
            SockInL_T  = "--";
            SockOutL_T = "--";
            SockInR_T  = "--";
            SockOutR_T = "--";

            GloveL = "gray";
            GloveR = "gray";
            GloveInL_T  = "--";
            GloveOutL_T = "--";
            GloveInR_T  = "--";
            GloveOutR_T = "--";

            // Set disconnection event for reconnection
            Adapter.DeviceDisconnected += OnDeviceDisconnected;
            Adapter.DeviceConnectionLost += OnDeviceDisconnected; // connection or discconnect?

            GetTimes();      // Get Duty Cycle Times

            _BeaniePicker = new List<string> { "Cap 1", "Cap 2", "Cap 3", "Cap 4", "Cap 5", "Cap 6", "Cap 7", "Cap 8" };
            _ShirtPicker = new List<string> { "Shirt 1", "Shirt 2", "Shirt 3", "Shirt 4", "Shirt 5", "Shirt 6", "Shirt 7", "Shirt 8" };
            _PantsPicker = new List<string> { "Pants 1", "Pants 2", "Pants 3", "Pants 4", "Pants 5", "Pants 6", "Pants 7", "Pants 8" };
            _SockPicker = new List<string> { "Sock 1", "Sock 2", "Sock 3", "Sock 4", "Sock 5", "Sock 6", "Sock 7", "Sock 8" };
            _GlovePicker = new List<string> { "Glove 1", "Glove 2", "Glove 3", "Glove 4", "Glove 5", "Glove 6", "Glove 7", "Glove 8" };

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

                // string fileName = pick_result.FullPath;    // Get file name from picker
                string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tags.csv");
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
    
