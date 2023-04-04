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

// New Imports for Bluetooth Autoconnect
using Plugin.BLE.Abstractions.Extensions;
using System.Threading;



namespace BLE.Client.ViewModels {
    public class ViewModelRFMicroS3Inventory : BaseViewModel {
        public class RFMicroTagInfoViewModel : BindableBase
        {
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // CLASS UPDATES/ADDITIONS
            private string _TimeString;    // Time at which last tag was read
            public string TimeString { get { return this._TimeString; } set { this.SetProperty(ref this._TimeString, value); } }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            private string _EPC;             public string EPC { get { return this._EPC; } set { this.SetProperty(ref this._EPC, value); } }
            private string _sensorAvgValue;  public string SensorAvgValue { get { return this._sensorAvgValue; } set { this.SetProperty(ref this._sensorAvgValue, value); } }
            private uint _sucessCount;       public uint SucessCount { get { return this._sucessCount; } set { this.SetProperty(ref this._sucessCount, value); } }
            private string _DisplayName;     public string DisplayName { get { return this._DisplayName; } set { this.SetProperty(ref this._DisplayName, value); } }
            private uint _OCRSSI;            public uint OCRSSI { get { return this._OCRSSI; } set { this.SetProperty(ref this._OCRSSI, value); } }
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
        Dictionary<string, List<string>> tag_RSSI = new Dictionary<string, List<string>>();
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

        // public FileResult pick_result;  // Save FilePicker.PickAsync() result for use in Autosave function
  
        #endregion


        #region ------------- EPCs ----------------

        // Display Variables for Shirt
        private string _Chest; public string Chest { get => _Chest; set { _Chest = value; OnPropertyChanged("Chest"); } }
        private string _ChestIn_T; public string ChestIn_T { get => _ChestIn_T; set { _ChestIn_T = value; OnPropertyChanged("ChestIn_T"); } }
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
        private string _LeftAb; public string LeftAb { get => _LeftAb; set { _LeftAb = value; OnPropertyChanged("LeftAb"); } }
        private string _LeftAbIn_T; public string LeftAbIn_T { get => _LeftAbIn_T; set { _LeftAbIn_T = value; OnPropertyChanged("LeftAbIn_T"); } }
        private string _LeftAbOut_T; public string LeftAbOut_T { get => _LeftAbOut_T; set { _LeftAbOut_T = value; OnPropertyChanged("LeftAbOut_T"); } }
        private string _RightAb; public string RightAb { get => _RightAb; set { _RightAb = value; OnPropertyChanged("RightAb"); } }
        private string _RightAbIn_T; public string RightAbIn_T { get => _RightAbIn_T; set { _RightAbIn_T = value; OnPropertyChanged("RightAbIn_T"); } }
        private string _RightAbOut_T; public string RightAbOut_T { get => _RightAbOut_T; set { _RightAbOut_T = value; OnPropertyChanged("RightAbOut_T"); } }

        public int THRESHOLD = 15;

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

        // 18 on Inside, 20 on Outside

        Shirt shirt18 = new Shirt( "B43E", "19B1", "AEA6", "9152", "59D6", "3060", "7491", "893F", "38C3" );
        Shirt shirt20 = new Shirt( "84B5", "A02C", "0A80", "787B", "83D5", "77DB", "9FA0", "6EC4", "AF3F" ); 

        private string _DebugVar; public string DebugVar { get => _DebugVar; set { _DebugVar = value; OnPropertyChanged("DebugVar"); } }

        public Random rnd = new Random();
        public int r;


        ////////////////////////////////////////////////////////////////////////////
        /////////////////// Added for UMich demo, on MWTC shirts ///////////////////
        ////////////////////////////////////////////////////////////////////////////
        private string _Back; public string Back { get => _Back; set { _Back = value; OnPropertyChanged("Back"); } }
        private string _BackIn_T; public string BackIn_T { get => _BackIn_T; set { _BackIn_T = value; OnPropertyChanged("BackIn_T"); } }
        private string _BackOut_T; public string BackOut_T { get => _BackOut_T; set { _BackOut_T = value; OnPropertyChanged("BackOut_T"); } }

        private string _BackNeck; public string BackNeck { get => _BackNeck; set { _BackNeck = value; OnPropertyChanged("BackNeck"); } }
        private string _BackNeckIn_T; public string BackNeckIn_T { get => _BackNeckIn_T; set { _BackNeckIn_T = value; OnPropertyChanged("BackNeckIn_T"); } }
        private string _BackNeckOut_T; public string BackNeckOut_T { get => _BackNeckOut_T; set { _BackNeckOut_T = value; OnPropertyChanged("BackNeckOut_T"); } }

        public double Chest1out = 0.0; public double Chest1in = 0.0;
        ////////////////////////////////////////////////////////////////////////////

        #endregion


        public ViewModelRFMicroS3Inventory(IAdapter adapter, IUserDialogs userDialogs) : base(adapter) {
            _userDialogs = userDialogs;
            r = rnd.Next(10000, 99999);

            LeftLow   = "gray"; LeftLowIn_T   = "--"; LeftLowOut_T   = "--";
            RightLow  = "gray"; RightLowIn_T  = "--"; RightLowOut_T  = "--";
            RightUp   = "gray"; RightUpIn_T   = "--"; RightUpOut_T   = "--";
            LeftUp    = "gray"; LeftUpIn_T    = "--"; LeftUpOut_T    = "--";
            Chest     = "gray"; ChestIn_T     = "--";
            Back      = "gray"; BackIn_T      = "--"; BackOut_T      = "--";
            BackNeck  = "gray"; BackNeckIn_T  = "--"; BackNeckOut_T  = "--";
            LeftAb    = "gray"; LeftAbIn_T    = "--"; LeftAbOut_T    = "--";
            RightAb   = "gray"; RightAbIn_T   = "--"; RightAbOut_T   = "--";

            // Set disconnection event for reconnection
            Adapter.DeviceDisconnected += OnDeviceDisconnected;
            Adapter.DeviceConnectionLost += OnDeviceDisconnected; // connection or disconnect?

            GetTimes();  // Get Duty Cycle Times

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
            // try {
            //     CancellationTokenSource tokenSource = new CancellationTokenSource();
            //     ConnectParameters connectParameters = new ConnectParameters(true, false);

            //     var config = new ProgressDialogConfig() {
            //         Title = $"Searching for '{PreviousGuid}'",
            //         CancelText = "Cancel",
            //         IsDeterministic = false,
            //         OnCancel = tokenSource.Cancel
            //     };

            //     using (var progress = _userDialogs.Progress(config)) {
            //         progress.Show();
            //         device = await Adapter.ConnectToKnownDeviceAsync(PreviousGuid, connectParameters, tokenSource.Token);
            //     }

            //     // var deviceItem = Devices.FirstOrDefault(d => d.Device.Id == device.Id);
            // }

            // catch (Exception ex) {
            //     _userDialogs.ShowError(ex.Message, 5000);
            //     return;
            // }

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
            _active_time   = 2000;
            _inactive_time = 2000;

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
                                        TagInfoList[cnt].OCRSSI = ocRSSI;

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

                                            if (!tag_RSSI.ContainsKey(TagInfoList[cnt].EPC)) {   // Check Tag_Data contains tags, add new data
                                                List<string> t_RSSI = new List<string>{TagInfoList[cnt].OCRSSI.ToString()};
                                                tag_RSSI.Add(TagInfoList[cnt].EPC, t_RSSI);
                                            }
                                            else {
                                                tag_RSSI[TagInfoList[cnt].EPC].Add(TagInfoList[cnt].OCRSSI.ToString());
                                            }
                                        }

                                        finally {
                                            // Get Last Four Characters of EPC
                                            string tEPC = TagInfoList[cnt].EPC.Substring(TagInfoList[cnt].EPC.Length - 4);

                                            if (shirt18.TagList.Contains(tEPC)) {
                                                if (tEPC==shirt18.Chest) {
                                                    Chest1in = SAV;
                                                    if ((Chest1out!=0.0) && (Chest1in!=0.0)) {
                                                        double flux = -2.39f * (Chest1in - Chest1out);
                                                        _ChestIn_T = flux.ToString("0.00");
                                                        RaisePropertyChanged(() => ChestIn_T);

                                                        if ((flux > 0.0) && (_Chest!="green")) {
                                                            _Chest = "green";
                                                            RaisePropertyChanged(() => Chest);
                                                        }
                                                        else if ((flux< 0.0) && (_Chest!="red")) {
                                                            _Chest = "red";
                                                            RaisePropertyChanged(() => Chest);
                                                        }
                                                    }
                                                }

                                                else if (tEPC==shirt18.LeftUpArm) {
                                                    _LeftUpIn_T = DisplaySAV; RaisePropertyChanged(() => LeftUpIn_T);
                                                    if ((SAV>THRESHOLD) && (_LeftUp!="green")) {
                                                        _LeftUp = "green"; RaisePropertyChanged(() => LeftUp);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftUp!="red")) {
                                                        _LeftUp = "red"; RaisePropertyChanged(() => LeftUp);
                                                    }
                                                }

                                                else if (tEPC==shirt18.LeftLowArm) {
                                                    _LeftLowIn_T = DisplaySAV; RaisePropertyChanged(() => LeftLowIn_T);
                                                    if ((SAV>THRESHOLD) && (_LeftLow!="green")) {
                                                        _LeftLow = "green"; RaisePropertyChanged(() => LeftLow);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftLow!="red")) {
                                                        _LeftLow = "red"; RaisePropertyChanged(() => LeftLow);
                                                    }
                                                }

                                                else if (tEPC==shirt18.RightUpArm) {
                                                    _RightUpIn_T = DisplaySAV; RaisePropertyChanged(() => RightUpIn_T);
                                                    if ((SAV>THRESHOLD) && (_RightUp!="green")) {
                                                        _RightUp = "green"; RaisePropertyChanged(() => RightUp);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightUp!="red")) {
                                                        _RightUp = "red"; RaisePropertyChanged(() => RightUp);
                                                    }
                                                }

                                                else if (tEPC==shirt18.RightLowArm) {
                                                    _RightLowIn_T = DisplaySAV; RaisePropertyChanged(() => RightLowIn_T);
                                                    if ((SAV>THRESHOLD) && (_RightLow!="green")) {
                                                        _RightLow = "green"; RaisePropertyChanged(() => RightLow);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightLow!="red")) {
                                                        _RightLow = "red"; RaisePropertyChanged(() => RightLow);
                                                    }
                                                }

                                                else if (tEPC==shirt18.Back) {
                                                    _BackIn_T = DisplaySAV; RaisePropertyChanged(() => BackIn_T);
                                                    if ((SAV>THRESHOLD) && (_Back!="green")) {
                                                        _Back = "green"; RaisePropertyChanged(() => Back);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_Back!="red")) {
                                                        _Back = "red"; RaisePropertyChanged(() => Back);
                                                    }
                                                }

                                                else if (tEPC==shirt18.BackNeck) {
                                                    _BackNeckIn_T = DisplaySAV; RaisePropertyChanged(() => BackNeckIn_T);
                                                    if ((SAV>THRESHOLD) && (_BackNeck!="green")) {
                                                        _BackNeck = "green"; RaisePropertyChanged(() => BackNeck);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_BackNeck!="red")) {
                                                        _BackNeck = "red"; RaisePropertyChanged(() => BackNeck);
                                                    }
                                                }

                                                else if (tEPC==shirt18.LeftAb) {
                                                    _LeftAbIn_T = DisplaySAV; RaisePropertyChanged(() => LeftAbIn_T);
                                                    if ((SAV>THRESHOLD) && (_LeftAb!="green")) {
                                                        _LeftAb = "green"; RaisePropertyChanged(() => LeftAb);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftAb!="red")) {
                                                        _LeftAb = "red"; RaisePropertyChanged(() => LeftAb);
                                                    }
                                                }

                                                else if (tEPC==shirt18.RightAb) {
                                                    _RightAbIn_T = DisplaySAV; RaisePropertyChanged(() => RightAbIn_T);
                                                    if ((SAV>THRESHOLD) && (_RightAb!="green")) {
                                                        _RightAb = "green"; RaisePropertyChanged(() => RightAb);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightAb!="red")) {
                                                        _RightAb = "red"; RaisePropertyChanged(() => RightAb);
                                                    }
                                                }

                                            }

                                            // Shirt
                                            else if (shirt20.TagList.Contains(tEPC)) {

                                                if (tEPC==shirt20.Chest) {
                                                    Chest1out = SAV;
                                                    if ((Chest1out!=0.0) && (Chest1in!=0.0)) {
                                                        double flux = -2.39f * (Chest1in - Chest1out);
                                                        _ChestIn_T = flux.ToString("0.00");
                                                        RaisePropertyChanged(() => ChestIn_T);
                                                        if ((flux > 0.0) && (_Chest!="green")) {
                                                            _Chest = "green";
                                                            RaisePropertyChanged(() => Chest);
                                                        }
                                                        else if ((flux< 0.0) && (_Chest!="red")) {
                                                            _Chest = "red";
                                                            RaisePropertyChanged(() => Chest);
                                                        }
                                                    }
                                                }
                                                
                                                else if (tEPC==shirt20.LeftUpArm) {
                                                    _LeftUpOut_T = DisplaySAV; RaisePropertyChanged(() => LeftUpOut_T);
                                                    if ((SAV>THRESHOLD) && (_LeftUp!="green")) {
                                                        _LeftUp = "green"; RaisePropertyChanged(() => LeftUp);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftUp!="red")) {
                                                        _LeftUp = "red"; RaisePropertyChanged(() => LeftUp);
                                                    }
                                                }

                                                else if (tEPC==shirt20.LeftLowArm) {
                                                    _LeftLowOut_T = DisplaySAV; RaisePropertyChanged(() => LeftLowOut_T);
                                                    if ((SAV>THRESHOLD) && (_LeftLow!="green")) {
                                                        _LeftLow = "green"; RaisePropertyChanged(() => LeftLow);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftLow!="red")) {
                                                        _LeftLow = "red"; RaisePropertyChanged(() => LeftLow);
                                                    }
                                                }

                                                else if (tEPC==shirt20.RightUpArm) {
                                                    _RightUpOut_T = DisplaySAV; RaisePropertyChanged(() => RightUpOut_T);
                                                    if ((SAV>THRESHOLD) && (_RightUp!="green")) {
                                                        _RightUp = "green"; RaisePropertyChanged(() => RightUp);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightUp!="red")) {
                                                        _RightUp = "red"; RaisePropertyChanged(() => RightUp);
                                                    }
                                                }

                                                else if (tEPC==shirt20.RightLowArm) {
                                                    _RightLowOut_T = DisplaySAV; RaisePropertyChanged(() => RightLowOut_T);
                                                    if ((SAV>THRESHOLD) && (_RightLow!="green")) {
                                                        _RightLow = "green"; RaisePropertyChanged(() => RightLow);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightLow!="red")) {
                                                        _RightLow = "red"; RaisePropertyChanged(() => RightLow);
                                                    }
                                                }

                                                else if (tEPC==shirt20.Back) {
                                                    _BackOut_T = DisplaySAV; RaisePropertyChanged(() => BackOut_T);
                                                    if ((SAV>THRESHOLD) && (_Back!="green")) {
                                                        _Back = "green"; RaisePropertyChanged(() => Back);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_Back!="red")) {
                                                        _Back = "red"; RaisePropertyChanged(() => Back);
                                                    }
                                                }

                                                else if (tEPC==shirt20.BackNeck) {
                                                    _BackNeckOut_T = DisplaySAV; RaisePropertyChanged(() => BackNeckOut_T);
                                                    if ((SAV>THRESHOLD) && (_BackNeck!="green")) {
                                                        _BackNeck = "green"; RaisePropertyChanged(() => BackNeck);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_BackNeck!="red")) {
                                                        _BackNeck = "red"; RaisePropertyChanged(() => BackNeck);
                                                    }
                                                }

                                                else if (tEPC==shirt20.LeftAb) {
                                                    _LeftAbOut_T = DisplaySAV; RaisePropertyChanged(() => LeftAbOut_T);
                                                    if ((SAV>THRESHOLD) && (_LeftAb!="green")) {
                                                        _LeftAb = "green"; RaisePropertyChanged(() => LeftAb);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_LeftAb!="red")) {
                                                        _LeftAb = "red"; RaisePropertyChanged(() => LeftAb);
                                                    }
                                                }

                                                else if (tEPC==shirt20.RightAb) {
                                                    _RightAbOut_T = DisplaySAV; RaisePropertyChanged(() => RightAbOut_T);
                                                    if ((SAV>THRESHOLD) && (_RightAb!="green")) {
                                                        _RightAb = "green"; RaisePropertyChanged(() => RightAb);
                                                    }
                                                    else if ((SAV<=THRESHOLD) && (_RightAb!="red")) {
                                                        _RightAb = "red"; RaisePropertyChanged(() => RightAb);
                                                    }
                                                }

                                            }

                                        }   // End of Try/Finally block

                                    }     // If caldata is nonzero...
                                }         // If temp within range...
                            }
                            else {}
                            found = true;
                            break;
                        }
                    }

                    if (!found) {
                        RFMicroTagInfoViewModel item = new RFMicroTagInfoViewModel();
                        item.EPC = info.epc.ToString();
                        item.SensorAvgValue = "";
                        item.SucessCount = 0;
                        item.DisplayName = item.EPC;
                        item.OCRSSI = ocRSSI;

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
                                    List<string> t_RSSI = new List<string>{ item.OCRSSI.ToString() };

                                    try {
                                        tag_Time.Add(item.EPC, t_time);
                                        tag_Data.Add(item.EPC, t_data);
                                        tag_RSSI.Add(item.EPC, t_RSSI);
                                        tag_List.Add(item.EPC);
                                    }
                                    finally {}
                                }
                            }
                        }
                        else {}
                        TagInfoList.Insert(0, item);
                    }
                }
            });
        }

        void VoltageEvent(object sender, CSLibrary.Notification.VoltageEventArgs e) {}

        public string fpath;

        private void AutoSaveData() {    // Function for Sharing time series data from tags
            InvokeOnMainThread(()=> {
                string fpath = "tags_" + r.ToString() + ".csv";
                string rssipath = "RSSI_" + r.ToString() + ".csv";

                // string fileName = pick_result.FullPath;    // Get file name from picker
                string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fpath);
                string rssiName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), rssipath);
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

                File.WriteAllText(rssiName, String.Empty); // Empty text file to rewrite database
                using (StreamWriter writer = new StreamWriter(rssiName, true)) {
                    foreach (string name in tag_List) {
                        writer.WriteLine(name + "\n" + "[");
                        foreach (var i in tag_Time[name]) { writer.WriteLine(i); }
                        writer.WriteLine("]\n[");
                        foreach (var j in tag_RSSI[name]) { writer.WriteLine(j); }
                        writer.WriteLine("]\n ");
                    }
                    writer.Close();
                }
            });
        }

        private async void ShareDataButtonClick()
        {
            // string fileName = pick_result.FullPath;
            // await Share.RequestAsync(new ShareFileRequest {
            //     Title = "Share Tags",
            //     File = new ShareFile(fileName)
            // });
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
    
