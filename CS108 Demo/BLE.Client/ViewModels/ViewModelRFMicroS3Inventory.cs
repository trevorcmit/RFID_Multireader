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
using System.Threading.Tasks;
using System.Windows.Input;
// using Xamarin;
using Xamarin.Forms;
using Xamarin.Essentials;

// New Imports for Bluetooth Autoconnect
using Plugin.BLE.Abstractions.Extensions;
using System.Threading;


namespace BLE.Client.ViewModels
{
    public class ViewModelRFMicroS3Inventory : BaseViewModel
    {
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
        public ICommand OnSwitchCommand               { protected set; get; }

        private ObservableCollection<RFMicroTagInfoViewModel> _TagInfoList = new ObservableCollection<RFMicroTagInfoViewModel>();
        public ObservableCollection<RFMicroTagInfoViewModel> TagInfoList { get { return _TagInfoList; } set { SetProperty(ref _TagInfoList, value); } }

        private string _startInventoryButtonText = "Start Inventory"; public string startInventoryButtonText { get { return _startInventoryButtonText; } }
        private string _SwitchButtonText = "Switch to Gradient"; public string SwitchButtonText { get { return _SwitchButtonText; } }
        bool IS_GRAD = false;

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

        public FileResult pick_result;  // Save FilePicker.PickAsync() result for use in Autosave function
        public FileResult rssi_result;  
  
        #endregion


        #region ------------- EPCs ----------------

        // Display Variables for Shirt
        private string _ChestIn;       public string ChestIn {       get => _ChestIn; set { _ChestIn = value; OnPropertyChanged("ChestIn"); } }
        private string _ChestOut;      public string ChestOut {      get => _ChestOut; set { _ChestOut = value; OnPropertyChanged("ChestOut"); } }
        private string _Chest;         public string Chest {         get => _Chest; set { _Chest = value; OnPropertyChanged("Chest"); } }
        private string _RightLow;      public string RightLow {      get => _RightLow; set { _RightLow = value; OnPropertyChanged("RightLow"); } }
        private string _RightLowIn_T;  public string RightLowIn_T {  get => _RightLowIn_T; set { _RightLowIn_T = value; OnPropertyChanged("RightLowIn_T"); } }
        private string _RightLowOut_T; public string RightLowOut_T { get => _RightLowOut_T; set { _RightLowOut_T = value; OnPropertyChanged("RightLowOut_T"); } }
        private string _LeftLow;       public string LeftLow {       get => _LeftLow; set { _LeftLow = value; OnPropertyChanged("LeftLow"); } }
        private string _LeftLowIn_T;   public string LeftLowIn_T {   get => _LeftLowIn_T; set { _LeftLowIn_T = value; OnPropertyChanged("LeftLowIn_T"); } }
        private string _LeftLowOut_T;  public string LeftLowOut_T {  get => _LeftLowOut_T; set { _LeftLowOut_T = value; OnPropertyChanged("LeftLowOut_T"); } }
        private string _RightUp;       public string RightUp {       get => _RightUp; set { _RightUp = value; OnPropertyChanged("RightUp"); } }
        private string _RightUpIn_T;   public string RightUpIn_T {   get => _RightUpIn_T; set { _RightUpIn_T = value; OnPropertyChanged("RightUpIn_T"); } }
        private string _RightUpOut_T;  public string RightUpOut_T {  get => _RightUpOut_T; set { _RightUpOut_T = value; OnPropertyChanged("RightUpOut_T"); } }
        private string _LeftUp;        public string LeftUp {        get => _LeftUp; set { _LeftUp = value; OnPropertyChanged("LeftUp"); } }
        private string _LeftUpIn_T;    public string LeftUpIn_T {    get => _LeftUpIn_T; set { _LeftUpIn_T = value; OnPropertyChanged("LeftUpIn_T"); } }
        private string _LeftUpOut_T;   public string LeftUpOut_T {   get => _LeftUpOut_T; set { _LeftUpOut_T = value; OnPropertyChanged("LeftUpOut_T"); } }
        private string _LeftAb;        public string LeftAb {        get => _LeftAb; set { _LeftAb = value; OnPropertyChanged("LeftAb"); } }
        private string _LeftAbIn_T;    public string LeftAbIn_T {    get => _LeftAbIn_T; set { _LeftAbIn_T = value; OnPropertyChanged("LeftAbIn_T"); } }
        private string _LeftAbOut_T;   public string LeftAbOut_T {   get => _LeftAbOut_T; set { _LeftAbOut_T = value; OnPropertyChanged("LeftAbOut_T"); } }
        private string _RightAb;       public string RightAb {       get => _RightAb; set { _RightAb = value; OnPropertyChanged("RightAb"); } }
        private string _RightAbIn_T;   public string RightAbIn_T {   get => _RightAbIn_T; set { _RightAbIn_T = value; OnPropertyChanged("RightAbIn_T"); } }
        private string _RightAbOut_T;  public string RightAbOut_T {  get => _RightAbOut_T; set { _RightAbOut_T = value; OnPropertyChanged("RightAbOut_T"); } }

        public int THRESHOLD = 15;
        public double WET = -7.0f;

        class Shirt
        {
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

            public Shirt
            (
                string backneck, string back, string chest, string leftab, string rightab, 
                string rightuparm, string rightlowarm, string leftuparm, string leftlowarm
            )
            {
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

        // 25 on INSIDE, 26 on OUTSIDE (MWTC EPCs)
        Shirt shirt25 = new Shirt( "205E", "9A88", "9F3D", "6F32", "1F74", "51B0", "AD80", "3277", "1E53" );
        Shirt shirt26 = new Shirt( "4813", "2791", "1F7A", "6F4A", "2383", "2F66", "2E5C", "926A", "2E56" );

        private string _DebugVar; public string DebugVar { get => _DebugVar; set { _DebugVar = value; OnPropertyChanged("DebugVar"); } }

        public Random rnd = new Random();
        public int r;

        public Dictionary<string, double> CORRECTION = new Dictionary<string, double>
        {
            // { "460E", 0.572158 },
            // { "B642", 0.026506 },
            // { "8320", -0.13917 },
            // { "9D1F", -0.73628 },
            // { "152D", 0.291637 },
            // { "88A5", -0.21336 },
            // { "2B1C", -0.42449 },
            // { "B0A5", -0.36315 },
            // { "87B4", 0.231665 },
            // { "0F4B", -1.30015 },
        };

        ////////////////////////////////////////////////////////////////////////////
        private string _Back; public string Back { get => _Back; set { _Back = value; OnPropertyChanged("Back"); } }
        private string _BackIn_T; public string BackIn_T { get => _BackIn_T; set { _BackIn_T = value; OnPropertyChanged("BackIn_T"); } }
        private string _BackOut_T; public string BackOut_T { get => _BackOut_T; set { _BackOut_T = value; OnPropertyChanged("BackOut_T"); } }

        private string _BackNeck; public string BackNeck { get => _BackNeck; set { _BackNeck = value; OnPropertyChanged("BackNeck"); } }
        private string _BackNeckIn_T; public string BackNeckIn_T { get => _BackNeckIn_T; set { _BackNeckIn_T = value; OnPropertyChanged("BackNeckIn_T"); } }
        private string _BackNeckOut_T; public string BackNeckOut_T { get => _BackNeckOut_T; set { _BackNeckOut_T = value; OnPropertyChanged("BackNeckOut_T"); } }

        public double Chest1out = 0.0; public double Chest1in = 0.0;
        public double Backout = 0.0; public double Backin = 0.0;
        public double BackNeckout = 0.0; public double BackNeckin = 0.0;
        public double LeftAbout = 0.0; public double LeftAbin = 0.0;
        public double RightAbout = 0.0; public double RightAbin = 0.0;
        public double LeftUpout = 0.0; public double LeftUpin = 0.0;
        public double RightUpout = 0.0; public double RightUpin = 0.0;
        public double LeftLowout = 0.0; public double LeftLowin = 0.0;
        public double RightLowout = 0.0; public double RightLowin = 0.0;

        private string _BackWet;        public string BackWet { get => _BackWet; set { _BackWet = value; OnPropertyChanged("BackWet"); } }
        private string _BackNeckWet;    public string BackNeckWet { get => _BackNeckWet; set { _BackNeckWet = value; OnPropertyChanged("BackNeckWet"); } }
        private string _ChestWet;       public string ChestWet { get => _ChestWet; set { _ChestWet = value; OnPropertyChanged("ChestWet"); } }
        private string _LeftAbWet;      public string LeftAbWet { get => _LeftAbWet; set { _LeftAbWet = value; OnPropertyChanged("LeftAbWet"); } }
        private string _RightAbWet;     public string RightAbWet { get => _RightAbWet; set { _RightAbWet = value; OnPropertyChanged("RightAbWet"); } }
        private string _LeftUpWet;      public string LeftUpWet { get => _LeftUpWet; set { _LeftUpWet = value; OnPropertyChanged("LeftUpWet"); } }
        private string _RightUpWet;     public string RightUpWet { get => _RightUpWet; set { _RightUpWet = value; OnPropertyChanged("RightUpWet"); } }
        private string _LeftLowWet;     public string LeftLowWet { get => _LeftLowWet; set { _LeftLowWet = value; OnPropertyChanged("LeftLowWet"); } }
        private string _RightLowWet;    public string RightLowWet { get => _RightLowWet; set { _RightLowWet = value; OnPropertyChanged("RightLowWet"); } }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        /// Temperature Gradient Section ///////////////////////////////////////////

        class Recent 
        {
            public List<double> values = new List<double>{};
            public List<DateTime> times = new List<DateTime>{};
            private int Length = 10;

            public Recent() {}

            public void Add(DateTime t, double d)
            {
                // Check last time is more than one second ago
                if (times.Count==0)
                {
                    values.Add(d); times.Add(t);
                }
                else
                {
                    if (times[times.Count-1] <= t.AddSeconds(-1))
                    {
                        values.Add(d);
                        times.Add(t);
                        if (values.Count > Length) { values.RemoveAt(0); times.RemoveAt(0); }
                    }
                }
            }

            public string Slope()
            {
                if (values.Count==Length)
                {
                    double sumX = 0.0;
                    double sumY = 0.0;
                    double sumXY = 0.0;
                    double sumXX = 0.0;

                    for (int i=0; i<Length; i++)
                    {
                        double x = (times[i] - times[0]).TotalSeconds;
                        double y = values[i];
                        sumX += x;
                        sumY += y;
                        sumXX += x * x;
                        sumXY += x * y;
                    }

                    // Calculate gradient
                    double m = (10.0f * sumXY - sumX * sumY) / (10.0f * sumXX - sumX * sumX);
                    string output = Math.Round(m, 2).ToString() + "°/s";
                    return output;
                }
                else
                {
                    return "0°/s";
                }

            }

        }

        Recent In_BackRecent = new Recent();
        Recent In_BackNeckRecent = new Recent();
        Recent In_ChestRecent = new Recent();
        Recent In_LeftAbRecent = new Recent();
        Recent In_RightAbRecent = new Recent();
        Recent In_LeftUpRecent = new Recent();
        Recent In_RightUpRecent = new Recent();
        Recent In_LeftLowRecent = new Recent();
        Recent In_RightLowRecent = new Recent();

        Recent Out_BackRecent = new Recent();
        Recent Out_BackNeckRecent = new Recent();
        Recent Out_ChestRecent = new Recent();
        Recent Out_LeftAbRecent = new Recent();
        Recent Out_RightAbRecent = new Recent();
        Recent Out_LeftUpRecent = new Recent();
        Recent Out_RightUpRecent = new Recent();
        Recent Out_LeftLowRecent = new Recent();
        Recent Out_RightLowRecent = new Recent();

        private string _SwitchButtonColor; public string SwitchButtonColor { get => _SwitchButtonColor; set { _SwitchButtonColor = value; OnPropertyChanged("SwitchButtonColor"); } }

        #endregion


        public ViewModelRFMicroS3Inventory(IAdapter adapter, IUserDialogs userDialogs) : base(adapter) {
            _userDialogs = userDialogs;
            r = rnd.Next(10000, 99999);

            LeftLow   = "gray"; LeftLowIn_T   = "--"; LeftLowOut_T   = "--"; LeftLowWet  = "Dry";
            RightLow  = "gray"; RightLowIn_T  = "--"; RightLowOut_T  = "--"; RightLowWet = "Dry";
            RightUp   = "gray"; RightUpIn_T   = "--"; RightUpOut_T   = "--"; RightUpWet  = "Dry";
            LeftUp    = "gray"; LeftUpIn_T    = "--"; LeftUpOut_T    = "--"; LeftUpWet   = "Dry";
            Chest     = "gray"; ChestIn       = "--"; ChestOut       = "--"; ChestWet    = "Dry";
            Back      = "gray"; BackIn_T      = "--"; BackOut_T      = "--"; BackWet     = "Dry";
            BackNeck  = "gray"; BackNeckIn_T  = "--"; BackNeckOut_T  = "--"; BackNeckWet = "Dry";
            LeftAb    = "gray"; LeftAbIn_T    = "--"; LeftAbOut_T    = "--"; LeftAbWet   = "Dry";
            RightAb   = "gray"; RightAbIn_T   = "--"; RightAbOut_T   = "--"; RightAbWet  = "Dry";

            // Set disconnection event for reconnection
            Adapter.DeviceDisconnected += OnDeviceDisconnected;
            Adapter.DeviceConnectionLost += OnDeviceDisconnected; // connection or disconnect?

            GetTimes();  // Get Duty Cycle Times

            OnStartInventoryButtonCommand = new Command(StartInventoryClick);
            OnClearButtonCommand = new Command(ClearClick);
            OnShareDataCommand = new Command(ShareDataButtonClick);
            OnSwitchCommand = new Command(SwitchClick);
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

        public override void Resume()
        {
            base.Resume();

            // RFID event handler
            BleMvxApplication._reader.rfid.OnAsyncCallback += new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);

            // Key Button event handler
            BleMvxApplication._reader.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);
            BleMvxApplication._reader.notification.OnVoltageEvent += new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent);

            InventorySetting();
        }

        public void InventorySetting() {
            switch (BleMvxApplication._config.RFID_FrequenceSwitch) {
                case 0:
                    BleMvxApplication._reader.rfid.SetHoppingChannels(BleMvxApplication._config.RFID_Region);
                    _SwitchButtonColor = "#FF3D3E"; RaisePropertyChanged(() => SwitchButtonColor);
                    break;
                case 1:
                    BleMvxApplication._reader.rfid.SetFixedChannel(BleMvxApplication._config.RFID_Region, BleMvxApplication._config.RFID_FixedChannel);
                    _SwitchButtonColor = "#E4F004"; RaisePropertyChanged(() => SwitchButtonColor);
                    break;
                case 2:
                    BleMvxApplication._reader.rfid.SetAgileChannels(BleMvxApplication._config.RFID_Region);
                    _SwitchButtonColor = "#19AFFE"; RaisePropertyChanged(() => SwitchButtonColor);
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

        public override void Suspend()
        {
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

        private void ClearClick()
        {
            InvokeOnMainThread(() =>
            {
                lock (TagInfoList) { TagInfoList.Clear(); }
                tag_Data.Clear();
                tag_Time.Clear();
                tag_List.Clear();
            });
        }

        public RFMicroTagInfoViewModel objItemSelected { get; set; }

        void StartInventory()
        {
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

        void SwitchClick()
        {
            if (IS_GRAD==false)
            {
                IS_GRAD = true;
                _SwitchButtonText = "Switch to Temperature"; RaisePropertyChanged(() => SwitchButtonText);

                _BackIn_T = In_BackRecent.Slope();            RaisePropertyChanged(() => BackIn_T);
                _BackOut_T = Out_BackRecent.Slope();          RaisePropertyChanged(() => BackOut_T);
                _BackNeckIn_T = In_BackNeckRecent.Slope();    RaisePropertyChanged(() => BackNeckIn_T);
                _BackNeckOut_T = Out_BackNeckRecent.Slope();  RaisePropertyChanged(() => BackNeckOut_T);
                _ChestOut = Out_ChestRecent.Slope();          RaisePropertyChanged(() => ChestOut);
                _ChestIn = In_ChestRecent.Slope();            RaisePropertyChanged(() => ChestIn);
                _LeftAbIn_T = In_LeftAbRecent.Slope();        RaisePropertyChanged(() => LeftAbIn_T);
                _LeftAbOut_T = Out_LeftAbRecent.Slope();      RaisePropertyChanged(() => LeftAbOut_T);
                _RightAbIn_T = In_RightAbRecent.Slope();      RaisePropertyChanged(() => RightAbIn_T);
                _RightAbOut_T = Out_RightAbRecent.Slope();    RaisePropertyChanged(() => RightAbOut_T);
                _LeftUpIn_T = In_LeftUpRecent.Slope();        RaisePropertyChanged(() => LeftUpIn_T);
                _LeftUpOut_T = Out_LeftUpRecent.Slope();      RaisePropertyChanged(() => LeftUpOut_T);
                _RightUpIn_T = In_RightUpRecent.Slope();      RaisePropertyChanged(() => RightUpIn_T);
                _RightUpOut_T = Out_RightUpRecent.Slope();    RaisePropertyChanged(() => RightUpOut_T);
                _LeftLowIn_T = In_LeftLowRecent.Slope();      RaisePropertyChanged(() => LeftLowIn_T);
                _LeftLowOut_T = Out_LeftLowRecent.Slope();    RaisePropertyChanged(() => LeftLowOut_T);
                _RightLowIn_T = In_RightLowRecent.Slope();    RaisePropertyChanged(() => RightLowIn_T);
                _RightLowOut_T = Out_RightLowRecent.Slope();  RaisePropertyChanged(() => RightLowOut_T);
            }
            else if (IS_GRAD==true)
            {
                IS_GRAD = false;
                _SwitchButtonText = "Switch to Gradient"; RaisePropertyChanged(() => SwitchButtonText);

                _BackIn_T = Backin.ToString();            RaisePropertyChanged(() => BackIn_T);
                _BackOut_T = Backout.ToString();          RaisePropertyChanged(() => BackOut_T);
                _BackNeckIn_T = BackNeckin.ToString();    RaisePropertyChanged(() => BackNeckIn_T);
                _BackNeckOut_T = BackNeckout.ToString();  RaisePropertyChanged(() => BackNeckOut_T);
                _ChestOut = Chest1out.ToString();         RaisePropertyChanged(() => ChestOut);
                _ChestIn = Chest1in.ToString();           RaisePropertyChanged(() => ChestIn);
                _LeftAbIn_T = LeftAbin.ToString();        RaisePropertyChanged(() => LeftAbIn_T);
                _LeftAbOut_T = LeftAbout.ToString();      RaisePropertyChanged(() => LeftAbOut_T);
                _RightAbIn_T = RightAbin.ToString();      RaisePropertyChanged(() => RightAbIn_T);
                _RightAbOut_T = RightAbout.ToString();    RaisePropertyChanged(() => RightAbOut_T);
                _LeftUpIn_T = LeftUpin.ToString();        RaisePropertyChanged(() => LeftUpIn_T);
                _LeftUpOut_T = LeftUpout.ToString();      RaisePropertyChanged(() => LeftUpOut_T);
                _RightUpIn_T = RightUpin.ToString();      RaisePropertyChanged(() => RightUpIn_T);
                _RightUpOut_T = RightUpout.ToString();    RaisePropertyChanged(() => RightUpOut_T);
                _LeftLowIn_T = LeftLowin.ToString();      RaisePropertyChanged(() => LeftLowIn_T);
                _LeftLowOut_T = LeftLowout.ToString();    RaisePropertyChanged(() => LeftLowOut_T);
                _RightLowIn_T = RightLowin.ToString();    RaisePropertyChanged(() => RightLowIn_T);
                _RightLowOut_T = RightLowout.ToString();  RaisePropertyChanged(() => RightLowOut_T);
            }
        }


        //////////////////////////////////////////////////////////////////
        //////////////// Timer Function and Event Section ////////////////
        //////////////////////////////////////////////////////////////////

        async void GetTimes()
        {
            // Necessary part for picking autosave location
            pick_result = await FilePicker.PickAsync();
            rssi_result = await FilePicker.PickAsync();

            // Save every second and we cycle by half seconds
            _active_time   = 3000;
            _inactive_time = 3000;

            RaisePropertyChanged(() => active_time);
            RaisePropertyChanged(() => inactive_time);

            ActiveTimer();
            DownTimer();
        }

        private void ActiveTimer()
        {  
            activetimer.Interval = inactive_time;       // READER IS OFF FOR THIS DURATION
            activetimer.Elapsed += ActiveEvent;  
            activetimer.Enabled = false;
        }

        private void DownTimer()
        {
            downtimer.Interval = active_time;           // READER IS ACTIVE FOR THIS LONG
            downtimer.Elapsed += DownEvent;
            downtimer.Enabled = false;
        }

        private void ActiveEvent(object sender, System.Timers.ElapsedEventArgs e)
        {  
            activetimer.Enabled = false;
            downtimer.Enabled = true;
            // StartInventory();   // Turn on for Duty Cycle
        }

        private void DownEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            // StopInventory();    // Turn on for Duty Cycle
            AutoSaveData();    // Autosave while Down is occurring
            activetimer.Enabled = true;
            downtimer.Enabled = false;
        }

        //////////////////////////////////////////////////////////////////



        void TagInventoryEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e)
        {
            if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING) return;
            if (e.info.Bank1Data == null || e.info.Bank2Data == null)   return;
            InvokeOnMainThread(() => { AddOrUpdateTagData(e.info); });
        }

        void StateChangedEvent(object sender, CSLibrary.Events.OnStateChangedEventArgs e)
        {
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

        private async void AddOrUpdateTagData(CSLibrary.Structures.TagCallbackInfo info)
        {
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

                                    if (caldata==0) { TagInfoList[cnt].SensorAvgValue = "NoCalData"; }
                                    else
                                    {
                                        string tEPC = TagInfoList[cnt].EPC.Substring(TagInfoList[cnt].EPC.Length - 4);
                                        double SAV = Math.Round(getTempC(temp, caldata), 6);

                                        // if (CORRECTION.ContainsKey(tEPC))
                                        // {
                                        //     SAV = SAV - CORRECTION[tEPC];
                                        // }

                                        string DisplaySAV = Math.Round(SAV, 2).ToString() + "°";
                                        DateTime dt = DateTime.Now;
                                        TagInfoList[cnt].SensorAvgValue = SAV.ToString();
                                        TagInfoList[cnt].TimeString = DateTime.Now.ToString("HH:mm:ss");
                                        TagInfoList[cnt].OCRSSI = ocRSSI;
                                        TagInfoList[cnt].SucessCount++;

                                        try
                                        {
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

                                        finally
                                        {
                                            if (shirt25.TagList.Contains(tEPC))
                                            {
                                                if (tEPC==shirt25.Chest)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        In_ChestRecent.Add(dt, SAV);
                                                        _ChestIn = In_ChestRecent.Slope();
                                                        RaisePropertyChanged(() => ChestIn);
                                                    }
                                                    else
                                                    {
                                                        _ChestIn = DisplaySAV;
                                                        RaisePropertyChanged(() => ChestIn);
                                                    }
                                                    Chest1in = SAV;
                                                    if ((Chest1out!=0.0)&&(Chest1in!=0.0)) {
                                                        if ((Math.Abs(Chest1out - Chest1in) < WET)||(Chest1in < Chest1out)) {
                                                            _Chest = "blue"; RaisePropertyChanged(() => Chest);
                                                            _ChestWet = "Wet"; RaisePropertyChanged(() => ChestWet);
                                                        }
                                                        else {
                                                            _Chest = "green"; RaisePropertyChanged(() => Chest);
                                                            _ChestWet = "Dry"; RaisePropertyChanged(() => ChestWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt25.Back)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        In_BackRecent.Add(dt, SAV);
                                                        _BackIn_T = In_BackRecent.Slope();
                                                        RaisePropertyChanged(() => BackIn_T);
                                                    }
                                                    else
                                                    {
                                                        _BackIn_T = DisplaySAV;
                                                        RaisePropertyChanged(() => BackIn_T);
                                                    }
                                                    Backin = SAV;
                                                    if ((Backout!=0.0)&&(Backin!=0.0)) {
                                                        if ((Math.Abs(Backout - Backin) < WET)||(Backin < Backout)) {
                                                            _Back = "blue"; RaisePropertyChanged(() => Back);
                                                            _BackWet = "Wet"; RaisePropertyChanged(() => BackWet);
                                                        }
                                                        else {
                                                            _Back = "green"; RaisePropertyChanged(() => Back);
                                                            _BackWet = "Dry"; RaisePropertyChanged(() => BackWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt25.BackNeck)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        In_BackNeckRecent.Add(dt, SAV);
                                                        _BackNeckIn_T = In_BackNeckRecent.Slope();
                                                        RaisePropertyChanged(() => BackNeckIn_T);
                                                    }
                                                    else
                                                    {
                                                        _BackNeckIn_T = DisplaySAV;
                                                        RaisePropertyChanged(() => BackNeckIn_T);
                                                    }
                                                    BackNeckin = SAV;
                                                    if ((BackNeckout!=0.0)&&(BackNeckin!=0.0)) {
                                                        if ((Math.Abs(BackNeckout - BackNeckin) < WET)||(BackNeckin < BackNeckout)) {
                                                            _BackNeck = "blue"; RaisePropertyChanged(() => BackNeck);
                                                            _BackNeckWet = "Wet"; RaisePropertyChanged(() => BackNeckWet);
                                                        }
                                                        else {
                                                            _BackNeck = "green"; RaisePropertyChanged(() => BackNeck);
                                                            _BackNeckWet = "Dry"; RaisePropertyChanged(() => BackNeckWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt25.LeftUpArm)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        In_LeftUpRecent.Add(dt, SAV);
                                                        _LeftUpIn_T = In_LeftUpRecent.Slope();
                                                        RaisePropertyChanged(() => LeftUpIn_T);
                                                    }
                                                    else
                                                    {
                                                        _LeftUpIn_T = DisplaySAV;
                                                        RaisePropertyChanged(() => LeftUpIn_T);
                                                    }
                                                    LeftUpin = SAV;
                                                    if ((LeftUpout!=0.0)&&(LeftUpin!=0.0)) {
                                                        if ((Math.Abs(LeftUpout - LeftUpin) < WET)||(LeftUpin < LeftUpout)) {
                                                            _LeftUp = "blue"; RaisePropertyChanged(() => LeftUp);
                                                            _LeftUpWet = "Wet"; RaisePropertyChanged(() => LeftUpWet);
                                                        }
                                                        else {
                                                            _LeftUp = "green"; RaisePropertyChanged(() => LeftUp);
                                                            _LeftUpWet = "Dry"; RaisePropertyChanged(() => LeftUpWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt25.LeftLowArm)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        In_LeftLowRecent.Add(dt, SAV);
                                                        _LeftLowIn_T = In_LeftLowRecent.Slope();
                                                        RaisePropertyChanged(() => LeftLowIn_T);
                                                    }
                                                    else
                                                    {
                                                        _LeftLowIn_T = DisplaySAV;
                                                        RaisePropertyChanged(() => LeftLowIn_T);
                                                    }
                                                    LeftLowin = SAV;
                                                    if ((LeftLowout!=0.0)&&(LeftLowin!=0.0)) {
                                                        if ((Math.Abs(LeftLowout - LeftLowin) < WET)||(LeftLowin < LeftLowout)) {
                                                            _LeftLow = "blue"; RaisePropertyChanged(() => LeftLow);
                                                            _LeftLowWet = "Wet"; RaisePropertyChanged(() => LeftLowWet);
                                                        }
                                                        else {
                                                            _LeftLow = "green"; RaisePropertyChanged(() => LeftLow);
                                                            _LeftLowWet = "Dry"; RaisePropertyChanged(() => LeftLowWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt25.RightUpArm)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        In_RightUpRecent.Add(dt, SAV);
                                                        _RightUpIn_T = In_RightUpRecent.Slope();
                                                        RaisePropertyChanged(() => RightUpIn_T);
                                                    }
                                                    else
                                                    {
                                                        _RightUpIn_T = DisplaySAV;
                                                        RaisePropertyChanged(() => RightUpIn_T);
                                                    }
                                                    RightUpin = SAV;
                                                    if ((RightUpout!=0.0)&&(RightUpin!=0.0)) {
                                                        if ((Math.Abs(RightUpout - RightUpin) < WET)||(RightUpin < RightUpout)) {
                                                            _RightUp = "blue"; RaisePropertyChanged(() => RightUp);
                                                            _RightUpWet = "Wet"; RaisePropertyChanged(() => RightUpWet);
                                                        }
                                                        else {
                                                            _RightUp = "green"; RaisePropertyChanged(() => RightUp);
                                                            _RightUpWet = "Dry"; RaisePropertyChanged(() => RightUpWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt25.RightLowArm)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        In_RightLowRecent.Add(dt, SAV);
                                                        _RightLowIn_T = In_RightLowRecent.Slope();
                                                        RaisePropertyChanged(() => RightLowIn_T);
                                                    }
                                                    else
                                                    {
                                                        _RightLowIn_T = DisplaySAV;
                                                        RaisePropertyChanged(() => RightLowIn_T);
                                                    }
                                                    RightLowin = SAV;
                                                    if ((RightLowout!=0.0)&&(RightLowin!=0.0)) {
                                                        if ((Math.Abs(RightLowout - RightLowin) < WET)||(RightLowin < RightLowout)) {
                                                            _RightLow = "blue"; RaisePropertyChanged(() => RightLow);
                                                            _RightLowWet = "Wet"; RaisePropertyChanged(() => RightLowWet);
                                                        }
                                                        else {
                                                            _RightLow = "green"; RaisePropertyChanged(() => RightLow);
                                                            _RightLowWet = "Dry"; RaisePropertyChanged(() => RightLowWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt25.LeftAb)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        In_LeftAbRecent.Add(dt, SAV);
                                                        _LeftAbIn_T = In_LeftAbRecent.Slope();
                                                        RaisePropertyChanged(() => LeftAbIn_T);
                                                    }
                                                    else
                                                    {
                                                        _LeftAbIn_T = DisplaySAV;
                                                        RaisePropertyChanged(() => LeftAbIn_T);
                                                    }
                                                    LeftAbin = SAV;
                                                    if ((LeftAbout!=0.0)&&(LeftAbin!=0.0)) {
                                                        if ((Math.Abs(LeftAbout - LeftAbin) < WET)||(LeftAbin < LeftAbout)) {
                                                            _LeftAb = "blue"; RaisePropertyChanged(() => LeftAb);
                                                            _LeftAbWet = "Wet"; RaisePropertyChanged(() => LeftAbWet);
                                                        }
                                                        else {
                                                            _LeftAb = "green"; RaisePropertyChanged(() => LeftAb);
                                                            _LeftAbWet = "Dry"; RaisePropertyChanged(() => LeftAbWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt25.RightAb)
                                                {
                                                    if (IS_GRAD)
                                                    {
                                                        In_RightAbRecent.Add(dt, SAV);
                                                        _RightAbIn_T = In_RightAbRecent.Slope();
                                                        RaisePropertyChanged(() => RightAbIn_T);
                                                    }
                                                    else
                                                    {
                                                        _RightAbIn_T = DisplaySAV;
                                                        RaisePropertyChanged(() => RightAbIn_T);
                                                    }
                                                    RightAbin = SAV;
                                                    if ((RightAbout!=0.0)&&(RightAbin!=0.0)) {
                                                        if ((Math.Abs(RightAbout - RightAbin) < WET)||(RightAbin < RightAbout)) {
                                                            _RightAb = "blue"; RaisePropertyChanged(() => RightAb);
                                                            _RightAbWet = "Wet"; RaisePropertyChanged(() => RightAbWet);
                                                        }
                                                        else {
                                                            _RightAb = "green"; RaisePropertyChanged(() => RightAb);
                                                            _RightAbWet = "Dry"; RaisePropertyChanged(() => RightAbWet);
                                                        }
                                                    }
                                                }
                                            }
                                            else if (shirt26.TagList.Contains(tEPC))
                                            {
                                                if (tEPC==shirt26.Chest)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        Out_ChestRecent.Add(dt, SAV);
                                                        _ChestOut = Out_ChestRecent.Slope();
                                                        RaisePropertyChanged(() => ChestOut);
                                                    }
                                                    else
                                                    {
                                                        _ChestOut = DisplaySAV;
                                                        RaisePropertyChanged(() => ChestOut);
                                                    }
                                                    Chest1out = SAV;
                                                    if ((Chest1out!=0.0)&&(Chest1in!=0.0)) {
                                                        if ((Math.Abs(Chest1out - Chest1in) < WET)||(Chest1in < Chest1out)) {
                                                            _Chest = "blue"; RaisePropertyChanged(() => Chest);
                                                            _ChestWet = "Wet"; RaisePropertyChanged(() => ChestWet);
                                                        }
                                                        else {
                                                            _Chest = "green"; RaisePropertyChanged(() => Chest);
                                                            _ChestWet = "Dry"; RaisePropertyChanged(() => ChestWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt26.Back)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        Out_BackRecent.Add(dt, SAV);
                                                        _BackOut_T = Out_BackRecent.Slope();
                                                        RaisePropertyChanged(() => BackOut_T);
                                                    }
                                                    else
                                                    {
                                                        _BackOut_T = DisplaySAV;
                                                        RaisePropertyChanged(() => BackOut_T);
                                                    }
                                                    Backout = SAV;
                                                    if ((Backout!=0.0)&&(Backin!=0.0)) {
                                                        if ((Math.Abs(Backout - Backin) < WET)||(Backin < Backout)) {
                                                            _Back = "blue"; RaisePropertyChanged(() => Back);
                                                            _BackWet = "Wet"; RaisePropertyChanged(() => BackWet);
                                                        }
                                                        else {
                                                            _Back = "green"; RaisePropertyChanged(() => Back);
                                                            _BackWet = "Dry"; RaisePropertyChanged(() => BackWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt26.BackNeck)
                                                {
                                                    if (IS_GRAD)
                                                    {
                                                        Out_BackNeckRecent.Add(dt, SAV);
                                                        _BackNeckOut_T = Out_BackNeckRecent.Slope();
                                                        RaisePropertyChanged(() => BackNeckOut_T);
                                                    }
                                                    else
                                                    {
                                                        _BackNeckOut_T = DisplaySAV;
                                                        RaisePropertyChanged(() => BackNeckOut_T);
                                                    }
                                                    BackNeckout = SAV;
                                                    if ((BackNeckout!=0.0)&&(BackNeckin!=0.0)) {
                                                        if ((Math.Abs(BackNeckout - BackNeckin) < WET)||(BackNeckin < BackNeckout)) {
                                                            _BackNeck = "blue"; RaisePropertyChanged(() => BackNeck);
                                                            _BackNeckWet = "Wet"; RaisePropertyChanged(() => BackNeckWet);
                                                        }
                                                        else {
                                                            _BackNeck = "green"; RaisePropertyChanged(() => BackNeck);
                                                            _BackNeckWet = "Dry"; RaisePropertyChanged(() => BackNeckWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt26.LeftUpArm)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        Out_LeftUpRecent.Add(dt, SAV);
                                                        _LeftUpOut_T = Out_LeftUpRecent.Slope();
                                                        RaisePropertyChanged(() => LeftUpOut_T);
                                                    }
                                                    else
                                                    {
                                                        _LeftUpOut_T = DisplaySAV;
                                                        RaisePropertyChanged(() => LeftUpOut_T);
                                                    }
                                                    LeftUpout = SAV;
                                                    if ((LeftUpout!=0.0)&&(LeftUpin!=0.0)) {
                                                        if ((Math.Abs(LeftUpout - LeftUpin) < WET)||(LeftUpin < LeftUpout)) {
                                                            _LeftUp = "blue"; RaisePropertyChanged(() => LeftUp);
                                                            _LeftUpWet = "Wet"; RaisePropertyChanged(() => LeftUpWet);
                                                        }
                                                        else {
                                                            _LeftUp = "green"; RaisePropertyChanged(() => LeftUp);
                                                            _LeftUpWet = "Dry"; RaisePropertyChanged(() => LeftUpWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt26.LeftLowArm)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        Out_LeftLowRecent.Add(dt, SAV);
                                                        _LeftLowOut_T = Out_LeftLowRecent.Slope();
                                                        RaisePropertyChanged(() => LeftLowOut_T);
                                                    }
                                                    else
                                                    {
                                                        _LeftLowOut_T = DisplaySAV;
                                                        RaisePropertyChanged(() => LeftLowOut_T);
                                                    }
                                                    LeftLowout = SAV;
                                                    if ((LeftLowout!=0.0)&&(LeftLowin!=0.0)) {
                                                        if ((Math.Abs(LeftLowout - LeftLowin) < WET)||(LeftLowin < LeftLowout)) {
                                                            _LeftLow = "blue"; RaisePropertyChanged(() => LeftLow);
                                                            _LeftLowWet = "Wet"; RaisePropertyChanged(() => LeftLowWet);
                                                        }
                                                        else {
                                                            _LeftLow = "green"; RaisePropertyChanged(() => LeftLow);
                                                            _LeftLowWet = "Dry"; RaisePropertyChanged(() => LeftLowWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt26.RightUpArm)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        Out_RightUpRecent.Add(dt, SAV);
                                                        _RightUpOut_T = Out_RightUpRecent.Slope();
                                                        RaisePropertyChanged(() => RightUpOut_T);
                                                    }
                                                    else
                                                    {
                                                        _RightUpOut_T = DisplaySAV;
                                                        RaisePropertyChanged(() => RightUpOut_T);
                                                    }
                                                    RightUpout = SAV;
                                                    if ((RightUpout!=0.0)&&(RightUpin!=0.0)) {
                                                        if ((Math.Abs(RightUpout - RightUpin) < WET)||(RightUpin < RightUpout)) {
                                                            _RightUp = "blue"; RaisePropertyChanged(() => RightUp);
                                                            _RightUpWet = "Wet"; RaisePropertyChanged(() => RightUpWet);
                                                        }
                                                        else {
                                                            _RightUp = "green"; RaisePropertyChanged(() => RightUp);
                                                            _RightUpWet = "Dry"; RaisePropertyChanged(() => RightUpWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt26.RightLowArm)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        Out_RightLowRecent.Add(dt, SAV);
                                                        _RightLowOut_T = Out_RightLowRecent.Slope();
                                                        RaisePropertyChanged(() => RightLowOut_T);
                                                    }
                                                    else
                                                    {
                                                        _RightLowOut_T = DisplaySAV;
                                                        RaisePropertyChanged(() => RightLowOut_T);
                                                    }
                                                    RightLowout = SAV;
                                                    if ((RightLowout!=0.0)&&(RightLowin!=0.0)) {
                                                        if ((Math.Abs(RightLowout - RightLowin) < WET)||(RightLowin < RightLowout)) {
                                                            _RightLow = "blue"; RaisePropertyChanged(() => RightLow);
                                                            _RightLowWet = "Wet"; RaisePropertyChanged(() => RightLowWet);
                                                        }
                                                        else {
                                                            _RightLow = "green"; RaisePropertyChanged(() => RightLow);
                                                            _RightLowWet = "Dry"; RaisePropertyChanged(() => RightLowWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt26.LeftAb)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        Out_LeftAbRecent.Add(dt, SAV);
                                                        _LeftAbOut_T = Out_LeftAbRecent.Slope();
                                                        RaisePropertyChanged(() => LeftAbOut_T);
                                                    }
                                                    else
                                                    {
                                                        _LeftAbOut_T = DisplaySAV;
                                                        RaisePropertyChanged(() => LeftAbOut_T);
                                                    }
                                                    LeftAbout = SAV;
                                                    if ((LeftAbout!=0.0)&&(LeftAbin!=0.0)) {
                                                        if ((Math.Abs(LeftAbout - LeftAbin) < WET)||(LeftAbin < LeftAbout)) {
                                                            _LeftAb = "blue"; RaisePropertyChanged(() => LeftAb);
                                                            _LeftAbWet = "Wet"; RaisePropertyChanged(() => LeftAbWet);
                                                        }
                                                        else {
                                                            _LeftAb = "green"; RaisePropertyChanged(() => LeftAb);
                                                            _LeftAbWet = "Dry"; RaisePropertyChanged(() => LeftAbWet);
                                                        }
                                                    }
                                                }
                                                else if (tEPC==shirt26.RightAb)
                                                {
                                                    if (IS_GRAD==true)
                                                    {
                                                        Out_RightAbRecent.Add(dt, SAV);
                                                        _RightAbOut_T = Out_RightAbRecent.Slope();
                                                        RaisePropertyChanged(() => RightAbOut_T);
                                                    }
                                                    else
                                                    {
                                                        _RightAbOut_T = DisplaySAV;
                                                        RaisePropertyChanged(() => RightAbOut_T);
                                                    }
                                                    RightAbout = SAV;
                                                    if ((RightAbout!=0.0)&&(RightAbin!=0.0)) {
                                                        if ((Math.Abs(RightAbout - RightAbin) < WET)||(RightAbin < RightAbout)) {
                                                            _RightAb = "blue"; RaisePropertyChanged(() => RightAb);
                                                            _RightAbWet = "Wet"; RaisePropertyChanged(() => RightAbWet);
                                                        }
                                                        else {
                                                            _RightAb = "green"; RaisePropertyChanged(() => RightAb);
                                                            _RightAbWet = "Dry"; RaisePropertyChanged(() => RightAbWet);
                                                        }
                                                    }
                                                }
                                            }

                                        }  // End of Try/Finally block

                                    }     // If caldata is nonzero...
                                }         // If temp within range...
                            }
                            else {}
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        RFMicroTagInfoViewModel item = new RFMicroTagInfoViewModel();
                        item.EPC = info.epc.ToString();
                        item.SensorAvgValue = "";
                        item.SucessCount = 0;
                        item.DisplayName = item.EPC;
                        item.OCRSSI = ocRSSI;

                        if (ocRSSI >= BleMvxApplication._rfMicro_minOCRSSI && ocRSSI <= BleMvxApplication._rfMicro_maxOCRSSI) {
                            if (temp>=1300 && temp<=3500) {
                                UInt64 caldata = (UInt64)(((UInt64)info.Bank2Data[0]<<48) | ((UInt64)info.Bank2Data[1]<<32) | ((UInt64)info.Bank2Data[2]<<16) | ((UInt64)info.Bank2Data[3]));

                                if (caldata==0) { item.SensorAvgValue = "NoCalData"; }
                                else
                                {
                                    item.SucessCount++;
                                    double SAV = Math.Round(getTempC(temp, caldata), 1);   
                                    item.SensorAvgValue = SAV.ToString();
                                    item.TimeString = DateTime.Now.ToString("HH:mm:ss");

                                    List<string> t_time = new List<string>{ item.TimeString };
                                    List<string> t_data = new List<string>{ item.SensorAvgValue };
                                    List<string> t_RSSI = new List<string>{ item.OCRSSI.ToString() };

                                    try
                                    {
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

        public string fpath;

        private void AutoSaveData() {    // Function for Sharing time series data from tags
            InvokeOnMainThread(()=> {
                string fpath = "tags_" + r.ToString() + ".csv";
                string rssipath = "RSSI_" + r.ToString() + ".csv";

                // string fileName = pick_result.FullPath;    // Get file name from picker
                // string rssiName = rssi_result.FullPath;    // Get file name from picker

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
    
