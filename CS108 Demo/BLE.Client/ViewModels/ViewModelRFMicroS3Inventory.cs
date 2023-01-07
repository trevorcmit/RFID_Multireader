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
// using SkiaSharp;


namespace BLE.Client.ViewModels {

    // public interface IAccessFileService {
    //     void CreateFile(string FileName);
    // }

    public class ViewModelRFMicroS3Inventory : BaseViewModel {
        public class RFMicroTagInfoViewModel : BindableBase {

            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // CLASS UPDATES/ADDITIONS
            private string _TimeString; // Time at which last tag was read
            public string TimeString { get { return this._TimeString; } set { this.SetProperty(ref this._TimeString, value); } }
            private DateTime _CurrentTime; // DateTime object for Live Plotting Comparison
            public DateTime CurrentTime { get { return this._CurrentTime; } set { this.SetProperty(ref this._CurrentTime, value); } }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////

            private string _EPC; public string EPC { get { return this._EPC; } set { this.SetProperty(ref this._EPC, value); } }
            private string _NickName; public string NickName { get { return this._NickName; } set { this.SetProperty(ref this._NickName, value); } }
            private string _TagName; public string TagName { get { return this._TagName; } set { this.SetProperty(ref this._TagName, value); } }
        
            private string _DisplayName; public string DisplayName { get { return this._DisplayName; } set { this.SetProperty(ref this._DisplayName, value); } }
            private uint _OCRSSI; public uint OCRSSI {get {return this._OCRSSI;} set {this.SetProperty(ref this._OCRSSI, value);}}
            private string _sensorAvgValue;
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

        private ObservableCollection<RFMicroTagInfoViewModel> _TagInfoList = new ObservableCollection<RFMicroTagInfoViewModel>();
        public ObservableCollection<RFMicroTagInfoViewModel> TagInfoList {get {return _TagInfoList;} set {SetProperty(ref _TagInfoList, value);}}

        private string _startInventoryButtonText = "Start Inventory"; public string startInventoryButtonText {get {return _startInventoryButtonText;}}

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////// For Saving Data / CSV exporting ///////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        List<string> tag_List = new List<string>();
        Dictionary<string, List<string>> tag_Time = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> tag_Data = new Dictionary<string, List<string>>();
        private List<string> _epcs; public List<string> epcs {get => _epcs; set {_epcs = value; OnPropertyChanged("epcs");}}
        private Dictionary<string, string> _map; public Dictionary<string, string> map {get => _map; set {_map = value; OnPropertyChanged("map");}}
        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////

        private string _labelVoltage = ""; public string labelVoltage {get {return _labelVoltage;}}
        public bool _startInventory = true;
        bool _cancelVoltageValue = false;


        ////////////////////////////////////////////////////
        ///////////// Variables for Duty Cycle /////////////
        ////////////////////////////////////////////////////
        private string _Duty; public string Duty { get => _Duty; set { _Duty = value; OnPropertyChanged("Duty"); } }
        private string _DutyColor; public string DutyColor { get => _DutyColor; set { _DutyColor = value; OnPropertyChanged("DutyColor"); } }
        private int _active_time; public int active_time { get => _active_time; set { _active_time = value; OnPropertyChanged("active_time"); } }
        private int _inactive_time; public int inactive_time { get => _inactive_time; set { _inactive_time = value; OnPropertyChanged("inactive_time"); } }
        public System.Timers.Timer activetimer = new System.Timers.Timer();
        public System.Timers.Timer downtimer = new System.Timers.Timer();
        ////////////////////////////////////////////////////

        // Save FilePicker.PickAsync() result for use in Autosave function
        public FileResult pick_result; 
        private string _DebugLabel; public string DebugLabel {get => _DebugLabel; set {_DebugLabel = value; OnPropertyChanged("DebugLabel");}}
  
        #endregion


        #region ------------- Person Selection ----------------

        private string _Back1; public string Back1 { get => _Back1; set { _Back1 = value; OnPropertyChanged("Back1"); } }
        private string _BackNeck1; public string BackNeck1 { get => _BackNeck1; set { _BackNeck1 = value; OnPropertyChanged("BackNeck1"); } }
        private string _Chest1; public string Chest1 { get => _Chest1; set { _Chest1 = value; OnPropertyChanged("Chest1"); } }
        private string _LeftAb1; public string LeftAb1 { get => _LeftAb1; set { _LeftAb1 = value; OnPropertyChanged("LeftAb1"); } }
        private string _RightAb1; public string RightAb1 { get => _RightAb1; set { _RightAb1 = value; OnPropertyChanged("RightAb1"); } }
        private string _LeftUpArm1; public string LeftUpArm1 { get => _LeftUpArm1; set { _LeftUpArm1 = value; OnPropertyChanged("LeftUpArm1"); } }
        private string _RightUpArm1; public string RightUpArm1 { get => _RightUpArm1; set { _RightUpArm1 = value; OnPropertyChanged("RightUpArm1"); } }
        private string _LeftLowArm1; public string LeftLowArm1 { get => _LeftLowArm1; set { _LeftLowArm1 = value; OnPropertyChanged("LeftLowArm1"); } }
        private string _RightLowArm1; public string RightLowArm1 { get => _RightLowArm1; set { _RightLowArm1 = value; OnPropertyChanged("RightLowArm1"); } }

        private string _Back1_T; public string Back1_T { get => _Back1_T; set { _Back1_T = value; OnPropertyChanged("Back1_T"); } }
        private string _BackNeck1_T; public string BackNeck1_T { get => _BackNeck1_T; set { _BackNeck1_T = value; OnPropertyChanged("BackNeck1_T"); } }
        private string _Chest1_T; public string Chest1_T { get => _Chest1_T; set { _Chest1_T = value; OnPropertyChanged("Chest1_T"); } }
        private string _LeftAb1_T; public string LeftAb1_T { get => _LeftAb1_T; set { _LeftAb1_T = value; OnPropertyChanged("LeftAb1_T"); } }
        private string _RightAb1_T; public string RightAb1_T { get => _RightAb1_T; set { _RightAb1_T = value; OnPropertyChanged("RightAb1_T"); } }
        private string _LeftUpArm1_T; public string LeftUpArm1_T { get => _LeftUpArm1_T; set { _LeftUpArm1_T = value; OnPropertyChanged("LeftUpArm1_T"); } }
        private string _RightUpArm1_T; public string RightUpArm1_T { get => _RightUpArm1_T; set { _RightUpArm1_T = value; OnPropertyChanged("RightUpArm1_T"); } }
        private string _LeftLowArm1_T; public string LeftLowArm1_T { get => _LeftLowArm1_T; set { _LeftLowArm1_T = value; OnPropertyChanged("LeftLowArm1_T"); } }
        private string _RightLowArm1_T; public string RightLowArm1_T { get => _RightLowArm1_T; set { _RightLowArm1_T = value; OnPropertyChanged("RightLowArm1_T"); } }

        #endregion


        public ViewModelRFMicroS3Inventory(IAdapter adapter, IUserDialogs userDialogs) : base(adapter) {
            _userDialogs = userDialogs;

            Back1        = "gray";
            BackNeck1    = "gray";
            Chest1       = "gray";
            LeftAb1      = "gray";
            RightAb1     = "gray";
            LeftUpArm1   = "gray";
            RightUpArm1  = "gray";
            LeftLowArm1  = "gray";
            RightLowArm1 = "gray";

            GetTimes();        // Get Duty Cycle Times

            OnStartInventoryButtonCommand = new Command(StartInventoryClick);
            OnClearButtonCommand = new Command(ClearClick);
            OnShareDataCommand = new Command(ShareDataButtonClick);
            OnAddNicknameCommand = new Command(Add_Nickname);
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
        //     set {
                // if (value != null) {
                    // BleMvxApplication._SELECT_EPC = value.EPC;
                    // ShowViewModel<ViewModelRFMicroReadTemp>(new MvxBundle());
                // }
        //     }
        //     get => null;
        // }

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
            // string active_time_str = await Application.Current.MainPage.DisplayPromptAsync( // Get tag name
            //     title: "Input ACTIVE Time for Duty Cycle", 
            //     message: "Example: 2000 (means 2 seconds)",
            //     placeholder: ""
            // );

            // string inactive_time_str = await Application.Current.MainPage.DisplayPromptAsync( // Get tag name
            //     title: "Input INACTIVE Time for Duty Cycle",
            //     message: "Example: 3000 (means 3 seconds)",
            //     placeholder: ""
            // );

            // Necessary part for picking autosave location
            pick_result = await FilePicker.PickAsync();
            _DebugLabel = pick_result.FullPath;
            RaisePropertyChanged(() => DebugLabel);

            // Save every second and we cycle by half seconds
            _active_time   = 500;
            _inactive_time = 500;

            // _active_time   = Convert.ToInt32(active_time_str);
            // _inactive_time = Convert.ToInt32(inactive_time_str);
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
            // _Duty = "ACTIVE"; RaisePropertyChanged(() => Duty);
            // StartInventory();

            activetimer.Enabled = false;
            downtimer.Enabled = true;
        }

        private void DownEvent(object sender, System.Timers.ElapsedEventArgs e) {
            // _Duty = "DOWN"; RaisePropertyChanged(() => Duty);
            // StopInventory();

            AutoSaveData();    // Autosave while Down is occurring
            activetimer.Enabled = true;
            downtimer.Enabled = false;
        }

        //////////////////////////////////////////////////////////////////



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
                    UInt16 sensorCode = (UInt16)(info.Bank1Data[0] & 0x1ff);   // Address c
                    UInt16 ocRSSI     = info.Bank1Data[1];                     // Address d
                    UInt16 temp       = info.Bank1Data[2];                     // Address e

                    for (cnt = 0; cnt < TagInfoList.Count; cnt++) {
                        // if (epcs.Contains(info.epc.ToString()) && (TagInfoList[cnt].EPC == info.epc.ToString())) {
                        if (TagInfoList[cnt].EPC == info.epc.ToString()) {
                            TagInfoList[cnt].OCRSSI = ocRSSI;
                            TagInfoList[cnt].RSSIColor = "Black";

                            if (ocRSSI >= BleMvxApplication._rfMicro_minOCRSSI && ocRSSI <= BleMvxApplication._rfMicro_maxOCRSSI) {
                                // BleMvxApplication._rfMicro_SensorType // 0 = Sensor code, 1 = Temp
                                // BleMvxApplication._rfMicro_SensorUnit // 0 = code, 1 = f, 2 = c, 3 = %

                                switch (BleMvxApplication._rfMicro_SensorType) {
                                    case 0: 
                                        break;
                                    default:
                                        if (temp >= 1300 && temp <= 3500) {
                                            TagInfoList[cnt].SucessCount++;
                                            UInt64 caldata = (UInt64)(((UInt64)info.Bank2Data[0] << 48) | ((UInt64)info.Bank2Data[1] << 32) | ((UInt64)info.Bank2Data[2] << 16) | ((UInt64)info.Bank2Data[3]));

                                            if (caldata == 0) { TagInfoList[cnt].SensorAvgValue = "NoCalData"; }
                                            else {
                                                switch (BleMvxApplication._rfMicro_SensorUnit) {
                                                    case 2: // F
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
                            item.SensorAvgValue = "";
                            item.RSSIColor = "Black";
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

        async void ShowDialog(string Msg)
        {
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
    
