using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions.Contracts;
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


namespace BLE.Client.ViewModels {

    public interface IAccessFileService {
        void CreateFile(string FileName);
    }

    public class ViewModelRFMicroS3Inventory : BaseViewModel {
        public class RFMicroTagInfoViewModel : BindableBase {

            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // CLASS UPDATES/ADDITIONS
            private string _TimeString; // Time at which last tag was read
            public string TimeString {get {return this._TimeString;} set{this.SetProperty(ref this._TimeString, value);}}
            private DateTime _CurrentTime; // DateTime object for Live Plotting Comparison
            public DateTime CurrentTime {get {return this._CurrentTime;} set{this.SetProperty(ref this._CurrentTime, value);}}
            private double _SAV; public double SAV;
            /////////////////////////////////////////////////////////////////////////////////////////////////////////

            private string _EPC; public string EPC {get {return this._EPC;} set {this.SetProperty(ref this._EPC, value);}}
            private string _NickName; public string NickName {get {return this._NickName;} set {this.SetProperty(ref this._NickName, value);}}
            private string _TagName; public string TagName {get {return this._TagName;} set {this.SetProperty(ref this._TagName, value);}}
        
            private string _DisplayName; public string DisplayName {get {return this._DisplayName;} set {this.SetProperty(ref this._DisplayName, value);}}
            private uint _OCRSSI; public uint OCRSSI {get {return this._OCRSSI;} set {this.SetProperty(ref this._OCRSSI, value);}}
            public double _sensorValueSum; private string _sensorAvgValue;
            public string SensorAvgValue {get {return this._sensorAvgValue;} set {this.SetProperty(ref this._sensorAvgValue, value);}}
            private uint _sucessCount; public uint SucessCount {get {return this._sucessCount;} set {this.SetProperty(ref this._sucessCount, value);}}
            private string _RSSIColor; public string RSSIColor {get {return this._RSSIColor;} set {this.SetProperty(ref this._RSSIColor, value);}}
            private string _Performance; public string Performance {get {return this._Performance;} set {this.SetProperty(ref this._Performance, value);}}

            public RFMicroTagInfoViewModel() {}
        }

        private readonly IUserDialogs _userDialogs;

        #region -------------- RFID inventory -----------------

        public ICommand OnStartInventoryButtonCommand {protected set; get;}
        public ICommand OnClearButtonCommand {protected set; get;}
        public ICommand OnShareDataCommand {protected set; get;}
        public ICommand OnAddNicknameCommand {protected set; get;}

        public MvxCommand ConnectToPreviousCommand {protected set; get;}

        // private async void Reconnect() {
        //     DeviceListViewModel1 dlvm1 = new DeviceListViewModel1();
        //     dlvm1.ConnectToPreviousDeviceAsync();
        // }
        //private bool CanConnectToPrevious() {
        //    return PreviousGuid != default(Guid);
        //}
        //private Guid _previousGuid;
        //public Guid PreviousGuid {
         //   get {return _previousGuid;}
         //   set {
           //     _previousGuid = value;
           //     _settings.AddOrUpdateValue("lastguid", _previousGuid.ToString());
           //     RaisePropertyChanged();
          //      RaisePropertyChanged(() => ConnectToPreviousCommand);
         //   }
        //}

        private ObservableCollection<RFMicroTagInfoViewModel> _TagInfoList = new ObservableCollection<RFMicroTagInfoViewModel>();
        public ObservableCollection<RFMicroTagInfoViewModel> TagInfoList {get {return _TagInfoList;} set {SetProperty(ref _TagInfoList, value);}}

        private string _startInventoryButtonText = "Start Inventory"; public string startInventoryButtonText {get {return _startInventoryButtonText;}}

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // For Saving Data / CSV exporting
        List<string> tag_List = new List<string>();
        Dictionary<string, List<string>> tag_Time = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> tag_Data = new Dictionary<string, List<string>>();

        private string _Duty; public string Duty {get => _Duty; set {_Duty = value; OnPropertyChanged("Duty");}}
        private int _active_time; public int active_time {get => _active_time; set {_active_time = value; OnPropertyChanged("active_time");}}
        private int _inactive_time; public int inactive_time {get => _inactive_time; set {_inactive_time = value; OnPropertyChanged("inactive_time");}}
        private List<string> _epcs; public List<string> epcs {get => _epcs; set {_epcs = value; OnPropertyChanged("epcs");}}
        private Dictionary<string, string> _map; public Dictionary<string, string> map {get => _map; set {_map = value; OnPropertyChanged("map");}}

        ///////////////////////////////////////////////////////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////

        private string _labelVoltage = ""; public string labelVoltage {get {return _labelVoltage;}}
        public bool _startInventory = true;
        bool _cancelVoltageValue = false;
        public System.Timers.Timer activetimer = new System.Timers.Timer();
        public System.Timers.Timer downtimer = new System.Timers.Timer();

        // Save FilePicker.PickAsync() result for use in Autosave function
        public FileResult pick_result; 

        private string _DebugLabel; public string DebugLabel {get => _DebugLabel; set {_DebugLabel = value; OnPropertyChanged("DebugLabel");}}

        #endregion

        public ViewModelRFMicroS3Inventory(IAdapter adapter, IUserDialogs userDialogs) : base(adapter) {
            _userDialogs = userDialogs;

            GetTimes();   // Get Duty Cycle Times

            Duty = "N/A"; 
            RaisePropertyChanged(() => Duty);

            OnStartInventoryButtonCommand = new Command(StartInventoryClick);
            OnClearButtonCommand = new Command(ClearClick);
            OnShareDataCommand = new Command(ShareDataButtonClick);
            OnAddNicknameCommand = new Command(Add_Nickname);
        }

        async void GetTimes() {
            string active_time_str = await Application.Current.MainPage.DisplayPromptAsync( // Get tag name
                title: "Input ACTIVE Time for Duty Cycle", 
                message: "Example: 2000 (means 2 seconds)",
                placeholder: ""
            );

            string inactive_time_str = await Application.Current.MainPage.DisplayPromptAsync( // Get tag name
                title: "Input INACTIVE Time for Duty Cycle",
                message: "Example: 3000 (means 3 seconds)",
                placeholder: ""
            );

            pick_result = await FilePicker.PickAsync();

            _DebugLabel = pick_result.FullPath;
            RaisePropertyChanged(() => DebugLabel);

            _active_time   = Convert.ToInt32(active_time_str);
            _inactive_time = Convert.ToInt32(inactive_time_str);
            RaisePropertyChanged(() => active_time);
            RaisePropertyChanged(() => inactive_time);

            ActiveTimer();
            DownTimer();
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

        public RFMicroTagInfoViewModel objItemSelected {
            set {
                if (value != null) {
                    BleMvxApplication._SELECT_EPC = value.EPC;
                    ShowViewModel<ViewModelRFMicroReadTemp>(new MvxBundle());
                }
            }
            get => null;
        }

        async void InventorySetting() {

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

        void SetConfigPower() {
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

        void SetPower(int index) {
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

                case 3:
                    SetPower(_powerRunning);
                    break;
                case 4:
                    SetConfigPower();
                    break;
            }
        }

        int _powerRunning = 0;
        void StartInventory() {
            if (_startInventory == false) return;

            SetPower(BleMvxApplication._rfMicro_Power);
            {
                _startInventory = false;
                _startInventoryButtonText = "Stop Inventory";
            }

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_EXERANGING);
            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.INVENTORY);
            _cancelVoltageValue = true;

            RaisePropertyChanged(() => startInventoryButtonText);
        }

        void StopInventory() {
            _startInventory = true;
            _startInventoryButtonText = "Start Inventory";

            BleMvxApplication._reader.rfid.StopOperation();
            RaisePropertyChanged(() => startInventoryButtonText);

            if (_powerRunning >= 2) _powerRunning = 0;
            else                    _powerRunning++;
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
            _Duty = "ACTIVE"; RaisePropertyChanged(() => Duty);
            StartInventory();
            activetimer.Enabled = false;
            downtimer.Enabled = true;
        }

        private void DownEvent(object sender, System.Timers.ElapsedEventArgs e) {
            _Duty = "DOWN"; RaisePropertyChanged(() => Duty);
            StopInventory();

            AutoSaveData(); // Autosave while Down is occurring

            activetimer.Enabled = true;
            downtimer.Enabled = false;
        }

        void TagInventoryEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e) {
            if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING) return;
            if (e.info.Bank1Data == null || e.info.Bank2Data == null) return;
            InvokeOnMainThread(() => { AddOrUpdateTagData(e.info); });
        }

        void StateChangedEvent(object sender, CSLibrary.Events.OnStateChangedEventArgs e) {
            switch (e.state) {
                case CSLibrary.Constants.RFState.IDLE:
                    ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
                    _cancelVoltageValue = true;
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
                    UInt16 sensorCode = (UInt16)(info.Bank1Data[0] & 0x1ff);  // Address c
                    UInt16 ocRSSI     = info.Bank1Data[1];                    // Address d
                    UInt16 temp       = info.Bank1Data[2];                    // Address e

                    for (cnt = 0; cnt < TagInfoList.Count; cnt++) {
                        // if (epcs.Contains(info.epc.ToString()) && (TagInfoList[cnt].EPC == info.epc.ToString())) {
                        if (TagInfoList[cnt].EPC == info.epc.ToString()) {
                            TagInfoList[cnt].OCRSSI = ocRSSI;
                            TagInfoList[cnt].RSSIColor = "Black";

                            if (ocRSSI >= BleMvxApplication._rfMicro_minOCRSSI && ocRSSI <= BleMvxApplication._rfMicro_maxOCRSSI) {
                                // BleMvxApplication._rfMicro_SensorType // 0 = Sensor code, 1 = Temp
                                // BleMvxApplication._rfMicro_SensorUnit // 0 = code, 1 = f, 2 = c, 3 = %

                                switch (BleMvxApplication._rfMicro_SensorType) {
                                    case 0: break;
                                    default:
                                        if (temp >= 1300 && temp <= 3500) {
                                            double SensorAvgValue;
                                            TagInfoList[cnt].SucessCount++;
                                            UInt64 caldata = (UInt64)(((UInt64)info.Bank2Data[0] << 48) | ((UInt64)info.Bank2Data[1] << 32) | ((UInt64)info.Bank2Data[2] << 16) | ((UInt64)info.Bank2Data[3]));

                                            if (caldata == 0) { TagInfoList[cnt].SensorAvgValue = "NoCalData"; }
                                            else {
                                                switch (BleMvxApplication._rfMicro_SensorUnit) {
                                                    case 0: // Code
                                                        TagInfoList[cnt]._sensorValueSum += temp;
                                                        SensorAvgValue = Math.Round(TagInfoList[cnt]._sensorValueSum / TagInfoList[cnt].SucessCount, 2);
                                                        TagInfoList[cnt].SensorAvgValue = SensorAvgValue.ToString();
                                                        break;
                                                    case 2: // F
                                                        TagInfoList[cnt]._sensorValueSum += getTempF(temp, caldata);
                                                        SensorAvgValue = Math.Round(TagInfoList[cnt]._sensorValueSum / TagInfoList[cnt].SucessCount, 2);
                                                        TagInfoList[cnt].SensorAvgValue = SensorAvgValue.ToString();
                                                        break;
                                                    default: // C
                                                        double SAV = Math.Round(getTempC(temp, caldata), 2);                                               
                                                        TagInfoList[cnt].SensorAvgValue = SAV.ToString();
                                                        TagInfoList[cnt].TimeString = DateTime.Now.ToString("HH:mm:ss");

                                                        try {
                                                            if (!tag_List.Contains(TagInfoList[cnt].EPC)) { // Check Tag_List contains tags, add new data
                                                                tag_List.Add(TagInfoList[cnt].EPC);
                                                            }

                                                            if (!tag_Time.ContainsKey(TagInfoList[cnt].EPC)) { // Check Tag_Time contains tags, add new data
                                                                List<string> t_time = new List<string>{TagInfoList[cnt].TimeString};
                                                                tag_Time.Add(TagInfoList[cnt].EPC, t_time);
                                                            }
                                                            else {
                                                                tag_Time[TagInfoList[cnt].EPC].Add(TagInfoList[cnt].TimeString);
                                                            }

                                                            if (!tag_Data.ContainsKey(TagInfoList[cnt].EPC)) { // Check Tag_Data contains tags, add new data
                                                                List<string> t_data = new List<string>{TagInfoList[cnt].SensorAvgValue};
                                                                tag_Data.Add(TagInfoList[cnt].EPC, t_data);
                                                            }
                                                            else {
                                                                tag_Data[TagInfoList[cnt].EPC].Add(TagInfoList[cnt].SensorAvgValue);
                                                            }
                                                        }

                                                        finally {
                                                            // AutoSaveData();
                                                        }

                                                        break;
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                            else { TagInfoList[cnt].RSSIColor = "Red"; }
                            found = true;
                            break;
                        }
                    }

                    if (!found) {
                        // if (epcs.Contains(info.epc.ToString())) {
                            RFMicroTagInfoViewModel item = new RFMicroTagInfoViewModel();

                            item.EPC = info.epc.ToString();
                            item.TagName  = GetTagName(item.EPC);
                            item.NickName = GetNickName(item.EPC);
                            item.DisplayName = GetTagName(item.EPC);

                            item.OCRSSI = ocRSSI;
                            item.SucessCount = 0;
                            item._sensorValueSum = 0;
                            item.SensorAvgValue = "";
                            item.RSSIColor = "Black";
                            item.SAV = 0;
                            item.Performance = "";

                            if (ocRSSI >= BleMvxApplication._rfMicro_minOCRSSI && ocRSSI <= BleMvxApplication._rfMicro_maxOCRSSI) {
                                // BleMvxApplication._rfMicro_SensorType // 0 = Sensor code, 1 = Temp
                                // BleMvxApplication._rfMicro_SensorUnit // 0 = code, 1 = f, 2 = c, 3 = %

                                switch (BleMvxApplication._rfMicro_SensorType) {
                                    case 0:
                                        break;
                                    default:
                                        if (temp >= 1300 && temp <= 3500) {
                                            item.SucessCount++;
                                            UInt64 caldata = (UInt64)(((UInt64)info.Bank2Data[0] << 48) | ((UInt64)info.Bank2Data[1] << 32) | ((UInt64)info.Bank2Data[2] << 16) | ((UInt64)info.Bank2Data[3]));

                                            if (caldata == 0) item.SensorAvgValue = "NoCalData";
                                            else
                                                switch (BleMvxApplication._rfMicro_SensorUnit) {
                                                    case 0:      // code
                                                        break;
                                                    case 2:      // F
                                                        break;
                                                    default:     // C
                                                        double SAV = Math.Round(getTempC(temp, caldata), 2);   
                                                        item.SensorAvgValue = SAV.ToString();
                                                        item.TimeString = DateTime.Now.ToString("HH:mm:ss");

                                                        // if (epcs.Contains(item.EPC)) {
                                                            List<string> t_time = new List<string>{ item.TimeString };
                                                            List<string> t_data = new List<string>{ item.SensorAvgValue };

                                                            try {
                                                                tag_Time.Add(item.EPC, t_time);
                                                                tag_Data.Add(item.EPC, t_data);
                                                                tag_List.Add(item.EPC);
                                                            }
                                                            finally {}
                                                        // }
                                                        break;
                                                }
                                        }
                                        break;
                                }
                            }
                            else { item.RSSIColor = "Red"; }
                            TagInfoList.Insert(0, item);
                            Trace.Message("EPC Data = {0}", item.EPC);
                        // }
                    }
                }
            });
        }

        string GetNickName(string EPC) {
            for (int index = 0; index < ViewModelRFMicroNickname._TagNicknameList.Count; index++)
                if (ViewModelRFMicroNickname._TagNicknameList[index].EPC == EPC)
                    return ViewModelRFMicroNickname._TagNicknameList[index].Nickname;
            return EPC;
        }

        string GetTagName(string EPC) {
            for (int index = 0; index < ViewModelRFMicroNickname._TagNicknameList.Count; index++)
                if (ViewModelRFMicroNickname._TagNicknameList[index].EPC == EPC)
                    return ViewModelRFMicroNickname._TagNicknameList[index].TagName;
            return EPC;
        }

        async void Add_Nickname() {
            string tn = await Application.Current.MainPage.DisplayPromptAsync( // Get tag name
                title: "Step 1: Pick Tag", 
                message: "Which tag to select?",
                placeholder: "Example: Left Sock #1"
            );
            
            string nn = await Application.Current.MainPage.DisplayPromptAsync( // Set tag name
                title: "Step 2: Select Nickname", 
                message: "What is the tag's new name?",
                placeholder: "Example: Gabriel's Left Sock"
            );

            for (int cnt = 0; cnt < TagInfoList.Count; cnt++) {
                if (TagInfoList[cnt].TagName == tn) { TagInfoList[cnt].DisplayName = nn; }
            }
        }

        double getTempF(UInt16 temp, UInt64 CalCode) {
            return (getTemperatue(temp, CalCode) * 1.8 + 32.0);
        }

        double getTempC(UInt16 temp, UInt64 CalCode) {
            return getTemperatue(temp, CalCode);
        }

        double getTemperatue(UInt16 temp, UInt64 CalCode) { // VERIFIED FROM MAGNUS AXZON DOCUMENTATION
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

        void VoltageEvent(object sender, CSLibrary.Notification.VoltageEventArgs e) {
            if (e.Voltage == 0xffff) { _labelVoltage = "CS108 Bat. ERROR"; }
            else {
                if (_cancelVoltageValue) { _cancelVoltageValue = false; return; }

                switch (BleMvxApplication._config.BatteryLevelIndicatorFormat) {
                    case 0:
                        _labelVoltage = "" + ((double)e.Voltage / 1000).ToString("0.000") + "v";
                        break;
                    default:
                        _labelVoltage = "" + ClassBattery.Voltage2Percent((double)e.Voltage / 1000).ToString("0") + "%";
                        break;
                }
            }
			RaisePropertyChanged(() => labelVoltage);
		}

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

        private async void ShareDataButtonClick() {
            string fileName = pick_result.FullPath;

            await Share.RequestAsync(new ShareFileRequest {
                Title = "Share Tags",
                File = new ShareFile(fileName)
            });
        }

        #region Key_event
        void HotKeys_OnKeyEvent(object sender, CSLibrary.Notification.HotKeyEventArgs e) {
            if (e.KeyCode == CSLibrary.Notification.Key.BUTTON) {
                if (e.KeyDown) { StartInventory(); }
                else           { StopInventory(); }
            }
        }
        #endregion

        async void ShowDialog(string Msg) {
            var config = new ProgressDialogConfig() {
                Title = Msg,
                IsDeterministic = true,
                MaskType = MaskType.Gradient,
            };

            using (var progress = _userDialogs.Progress(config)) {
                progress.Show();
                await System.Threading.Tasks.Task.Delay(1000);
            }
        }

    }
}
    
