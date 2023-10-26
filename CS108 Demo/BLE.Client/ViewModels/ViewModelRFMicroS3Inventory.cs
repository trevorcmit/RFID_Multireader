using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using Plugin.BLE.Abstractions;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin;
using Xamarin.Forms;
using Xamarin.Essentials;


namespace BLE.Client.ViewModels
{
    public class ViewModelRFMicroS3Inventory : BaseViewModel
    {
        public class RFMicroTagInfoViewModel : BindableBase
        {
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // CLASS UPDATES/ADDITIONS
            private string _TimeString; public string TimeString { get { return this._TimeString; } set { this.SetProperty(ref this._TimeString, value); } }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////

            private string _EPC; public string EPC { get { return this._EPC; } set { this.SetProperty(ref this._EPC, value); } }
            private string _DisplayName; public string DisplayName { get { return this._DisplayName; } set { this.SetProperty(ref this._DisplayName, value); } }
            private uint _OCRSSI; public uint OCRSSI { get { return this._OCRSSI; } set { this.SetProperty(ref this._OCRSSI, value); } }
            private string _sensorAvgValue; public string SensorAvgValue { get { return this._sensorAvgValue;} set { this.SetProperty(ref this._sensorAvgValue, value); } }
            private uint _sucessCount; public uint SucessCount { get { return this._sucessCount; } set { this.SetProperty(ref this._sucessCount, value); } }

            public RFMicroTagInfoViewModel() {}
        }

        private readonly IUserDialogs _userDialogs;

        #region -------------- RFID inventory -----------------

        public ICommand OnStartInventoryButtonCommand { protected set; get; }
        public ICommand OnClearButtonCommand { protected set; get; }
        public ICommand OnShareDataCommand { protected set; get; }
        private ObservableCollection<RFMicroTagInfoViewModel> _TagInfoList = new ObservableCollection<RFMicroTagInfoViewModel>();
        public ObservableCollection<RFMicroTagInfoViewModel> TagInfoList { get { return _TagInfoList; } set { SetProperty(ref _TagInfoList, value); } }

        private ObservableCollection<RFMicroTagInfoViewModel> _TagInfoList2 = new ObservableCollection<RFMicroTagInfoViewModel>();
        public ObservableCollection<RFMicroTagInfoViewModel> TagInfoList2 { get { return _TagInfoList2; } set { SetProperty(ref _TagInfoList2, value); } }

        private string _startInventoryButtonText = "Start Inventory";
        public string startInventoryButtonText { get { return _startInventoryButtonText; } }

        private string _HeaderColor1 = "#FF4F4F"; public string HeaderColor1 { get { return _HeaderColor1; } }
        private string _HeaderColor2 = "#FF4F4F"; public string HeaderColor2 { get { return _HeaderColor2; } }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // For Saving Data / CSV exporting
        List<string> tag_List = new List<string>();
        Dictionary<string, List<string>> tag_Time = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> tag_Data = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> tag_RSSI = new Dictionary<string, List<string>>();

        List<string> tag_List2 = new List<string>();
        Dictionary<string, List<string>> tag_Time2 = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> tag_Data2 = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> tag_RSSI2 = new Dictionary<string, List<string>>();

        ///////////////// Public/Private Variables for Body Model /////////////////
        private List<string> _epcs; public List<string> epcs { get => _epcs; set { _epcs = value; OnPropertyChanged("epcs"); } }
        ///////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////
        ///////////// Variables for Duty Cycle /////////////
        ////////////////////////////////////////////////////
        private int _active_time;   public int active_time   { get => _active_time; set { _active_time = value; OnPropertyChanged("active_time"); } }
        private int _inactive_time; public int inactive_time { get => _inactive_time; set { _inactive_time = value; OnPropertyChanged("inactive_time"); } }
        public System.Timers.Timer activetimer = new System.Timers.Timer();
        public System.Timers.Timer downtimer   = new System.Timers.Timer();
        ////////////////////////////////////////////////////

        public FileResult pick_result;  // Save FilePicker.PickAsync() result for use in Autosave function
        public FileResult rssi_result; 
        public FileResult pick_result2;
        public FileResult rssi_result2;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool _startInventory = true;
        #endregion


        public ViewModelRFMicroS3Inventory(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            GetTimes();  // Get Duty Cycle Times

            OnStartInventoryButtonCommand = new Command(StartInventoryClick);
            OnClearButtonCommand = new Command(ClearClick);
            OnShareDataCommand = new Command(ShareDataButtonClick);
        }

        ~ViewModelRFMicroS3Inventory() {}

        //////////////////////////////////////////////////////////////////
        //////////////// Timer Function and Event Section ////////////////
        //////////////////////////////////////////////////////////////////

        async void GetTimes()
        {
            // Necessary part for picking autosave location
            pick_result = await FilePicker.PickAsync();
            rssi_result = await FilePicker.PickAsync();

            pick_result2 = await FilePicker.PickAsync();
            rssi_result2 = await FilePicker.PickAsync();

            // Save every second and we cycle by half seconds
            _active_time   = 5000;
            _inactive_time = 5000;

            RaisePropertyChanged(() => active_time);
            RaisePropertyChanged(() => inactive_time);

            ActiveTimer();
            DownTimer();
        }

        private void ActiveTimer() // READER IS OFF FOR THIS DURATION
        {  
            activetimer.Interval = inactive_time;
            activetimer.Elapsed += ActiveEvent;  
            activetimer.Enabled = false;
        }

        private void DownTimer()  // READER IS ACTIVE FOR THIS LONG
        {
            downtimer.Interval = active_time;
            downtimer.Elapsed += DownEvent;
            downtimer.Enabled = false;
        }

        private void ActiveEvent(object sender, System.Timers.ElapsedEventArgs e)
        {  
            activetimer.Enabled = false;
            downtimer.Enabled = true;
            // StartInventory();   // Turn on for Duty Cycle

            // if (BleMvxApplication._reader1.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT)
            // {
            //     // SetPower(BleMvxApplication._rfMicro_Power, 1);
            //     // BleMvxApplication._reader1.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_EXERANGING);
            //     BleMvxApplication._reader1.rfid.SetPowerLevel(300); 

            //     _HeaderColor1 = "#5FFF6F";
            //     RaisePropertyChanged(() => HeaderColor1);
            // }

            // // BleMvxApplication._reader2.rfid.StopOperation();
            // BleMvxApplication._reader2.rfid.SetPowerLevel(0); 

            // _HeaderColor2 = "#FF4F4F";
            // RaisePropertyChanged(() => HeaderColor2);
        }

        private void DownEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            // StopInventory();    // Turn on for Duty Cycle
            AutoSaveData();    // Autosave while Down is occurring
            activetimer.Enabled = true;
            downtimer.Enabled = false;

            // if (BleMvxApplication._reader2.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT)
            // {
            //     // SetPower(BleMvxApplication._rfMicro_Power, 2);
            //     // BleMvxApplication._reader2.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_EXERANGING);
            //     BleMvxApplication._reader2.rfid.SetPowerLevel(300); 

            //     _HeaderColor2 = "#5FFF6F";
            //     RaisePropertyChanged(() => HeaderColor2);
            // }

            // // BleMvxApplication._reader1.rfid.StopOperation();
            // BleMvxApplication._reader1.rfid.SetPowerLevel(0); 

            // _HeaderColor1 = "#FF4F4F";
            // RaisePropertyChanged(() => HeaderColor1);
        }

        //////////////////////////////////////////////////////////////////

        public override void Resume()
        {
            base.Resume();

            if (BleMvxApplication._reader1.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT)
            {
                BleMvxApplication._reader1.rfid.OnAsyncCallback += new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent1); // RFID event handler
                BleMvxApplication._reader1.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent); // Key Button event handler
                BleMvxApplication._reader1.notification.OnVoltageEvent += new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent1);
            }

            if (BleMvxApplication._reader2.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT)
            {
                BleMvxApplication._reader2.rfid.OnAsyncCallback += new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent2);
                BleMvxApplication._reader2.notification.OnKeyEvent += new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);
                BleMvxApplication._reader2.notification.OnVoltageEvent += new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent2);
            }

            InventorySetting();
        }

        public override void Suspend()
        {
            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);

            if (BleMvxApplication._reader1.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT)
            {
                BleMvxApplication._reader1.rfid.CancelAllSelectCriteria(); // Confirm cancel all filter
                BleMvxApplication._reader1.rfid.StopOperation();
                BleMvxApplication._reader1.barcode.Stop();

                // Cancel RFID event handler
                BleMvxApplication._reader1.rfid.OnAsyncCallback -= new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent1);
                BleMvxApplication._reader1.rfid.OnStateChanged  += new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent1);

                // Key Button event handler
                BleMvxApplication._reader1.notification.OnKeyEvent     -= new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);
                BleMvxApplication._reader1.notification.OnVoltageEvent -= new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent1);
            }

            if (BleMvxApplication._reader2.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT)
            {
                BleMvxApplication._reader2.rfid.CancelAllSelectCriteria(); // Confirm cancel all filter
                BleMvxApplication._reader2.rfid.StopOperation();
                BleMvxApplication._reader2.barcode.Stop();

                // Cancel RFID event handler
                BleMvxApplication._reader2.rfid.OnAsyncCallback -= new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent2);
                BleMvxApplication._reader2.rfid.OnStateChanged  += new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent2);

                // Key Button event handler
                BleMvxApplication._reader2.notification.OnKeyEvent     -= new EventHandler<CSLibrary.Notification.HotKeyEventArgs>(HotKeys_OnKeyEvent);
                BleMvxApplication._reader2.notification.OnVoltageEvent -= new EventHandler<CSLibrary.Notification.VoltageEventArgs>(VoltageEvent2);
            }

            base.Suspend();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
        }

        private void ClearClick()
        {
            InvokeOnMainThread(() =>
            {
                lock (TagInfoList)
                {
                    TagInfoList.Clear();
                }
            });
        }

        public RFMicroTagInfoViewModel objItemSelected1
        {
            set {
                if (value != null)
                {
                    BleMvxApplication._SELECT_EPC = value.EPC;
                    ShowViewModel<ViewModelRFMicroReadTemp>(new MvxBundle());
                }
            }
            get => null;
        }

        public RFMicroTagInfoViewModel objItemSelected2
        {
            set {
                if (value != null) {
                    BleMvxApplication._SELECT_EPC = value.EPC;
                    ShowViewModel<ViewModelRFMicroReadTemp>(new MvxBundle());
                }
            }
            get => null;
        }

        void InventorySetting()
        {
            if (BleMvxApplication._reader1.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT)
            {
                switch (BleMvxApplication._config1.RFID_FrequenceSwitch) { // READER #1
                    case 0:
                        BleMvxApplication._reader1.rfid.SetHoppingChannels(BleMvxApplication._config1.RFID_Region);
                        break;
                    case 1:
                        BleMvxApplication._reader1.rfid.SetFixedChannel(BleMvxApplication._config1.RFID_Region, BleMvxApplication._config1.RFID_FixedChannel);
                        break;
                    case 2:
                        BleMvxApplication._reader1.rfid.SetAgileChannels(BleMvxApplication._config1.RFID_Region);
                        break;
                }

                BleMvxApplication._reader1.rfid.Options.TagRanging.flags = CSLibrary.Constants.SelectFlags.ZERO;
                SetPower(BleMvxApplication._rfMicro_Power, 1);

                // Reader #1: Setting 3, MUST SET for RFMicro
                BleMvxApplication._config1.RFID_DynamicQParms.toggleTarget = (BleMvxApplication._rfMicro_Target == 2) ? 1U : 0U;
                BleMvxApplication._config1.RFID_DynamicQParms.retryCount = 5; // for RFMicro special setting
                BleMvxApplication._reader1.rfid.SetDynamicQParms(BleMvxApplication._config1.RFID_DynamicQParms);
                BleMvxApplication._config1.RFID_DynamicQParms.retryCount = 0;    

                // Setting 4
                BleMvxApplication._config1.RFID_FixedQParms.toggleTarget = (BleMvxApplication._rfMicro_Target == 2) ? 1U : 0U;
                BleMvxApplication._config1.RFID_FixedQParms.retryCount = 5;                                                    // for RFMicro special setting
                BleMvxApplication._reader1.rfid.SetFixedQParms(BleMvxApplication._config1.RFID_FixedQParms);
                BleMvxApplication._config1.RFID_FixedQParms.retryCount = 0;  

                // Setting 2
                BleMvxApplication._reader1.rfid.SetOperationMode(BleMvxApplication._config1.RFID_OperationMode);
                BleMvxApplication._reader1.rfid.SetCurrentSingulationAlgorithm(BleMvxApplication._config1.RFID_Algorithm);
                BleMvxApplication._reader1.rfid.SetCurrentLinkProfile(BleMvxApplication._config1.RFID_Profile);

                BleMvxApplication._reader1.rfid.SetTagGroup(CSLibrary.Constants.Selected.ASSERTED, CSLibrary.Constants.Session.S0, CSLibrary.Constants.SessionTarget.A);

                // extraSlecetion Section
                CSLibrary.Structures.SelectCriterion extraSlecetion = new CSLibrary.Structures.SelectCriterion();
                extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.ASLINVA_DSLINVB, 0);
                extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.TID, 0, 28, new byte[] {0xe2, 0x82, 0x40, 0x30});
                BleMvxApplication._reader1.rfid.SetSelectCriteria(0, extraSlecetion);

                // Set OCRSSI Limit
                extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.NOTHING_DSLINVB, 0);
                extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0xd0, 8, new byte[] {(byte)(0x20 | BleMvxApplication._rfMicro_minOCRSSI)});
                BleMvxApplication._reader1.rfid.SetSelectCriteria(1, extraSlecetion);
                extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.NOTHING_DSLINVB, 0);
                extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0xd0, 8, new byte[] {(byte)(BleMvxApplication._rfMicro_maxOCRSSI)});
                BleMvxApplication._reader1.rfid.SetSelectCriteria(2, extraSlecetion);

                // Temperature and Sensor code
                extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.NOTHING_DSLINVB, 0);
                extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0xe0, 0, new byte[] {0x00});
                BleMvxApplication._reader1.rfid.SetSelectCriteria(3, extraSlecetion);

                BleMvxApplication._reader1.rfid.Options.TagRanging.flags |= CSLibrary.Constants.SelectFlags.SELECT;
                BleMvxApplication._reader1.rfid.Options.TagRanging.multibanks = 2;
                BleMvxApplication._reader1.rfid.Options.TagRanging.bank1 = CSLibrary.Constants.MemoryBank.BANK0;
                BleMvxApplication._reader1.rfid.Options.TagRanging.offset1 = 12; // Address C
                BleMvxApplication._reader1.rfid.Options.TagRanging.count1 = 3;
                BleMvxApplication._reader1.rfid.Options.TagRanging.bank2 = CSLibrary.Constants.MemoryBank.USER;
                BleMvxApplication._reader1.rfid.Options.TagRanging.offset2 = 8;
                BleMvxApplication._reader1.rfid.Options.TagRanging.count2 = 4;
                BleMvxApplication._reader1.rfid.Options.TagRanging.compactmode = false;

                BleMvxApplication._reader1.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PRERANGING);
            }

            if (BleMvxApplication._reader2.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT)
            {
                switch (BleMvxApplication._config2.RFID_FrequenceSwitch) { // READER #2
                    case 0:
                        BleMvxApplication._reader2.rfid.SetHoppingChannels(BleMvxApplication._config2.RFID_Region);
                        break;
                    case 1:
                        BleMvxApplication._reader2.rfid.SetFixedChannel(BleMvxApplication._config2.RFID_Region, BleMvxApplication._config2.RFID_FixedChannel);
                        break;
                    case 2:
                        BleMvxApplication._reader2.rfid.SetAgileChannels(BleMvxApplication._config2.RFID_Region);
                        break;
                }

                BleMvxApplication._reader2.rfid.Options.TagRanging.flags = CSLibrary.Constants.SelectFlags.ZERO;
                SetPower(BleMvxApplication._rfMicro_Power, 2);

                // Reader #2
                BleMvxApplication._config2.RFID_DynamicQParms.toggleTarget = (BleMvxApplication._rfMicro_Target == 2) ? 1U : 0U;
                BleMvxApplication._config2.RFID_DynamicQParms.retryCount = 5;
                BleMvxApplication._reader2.rfid.SetDynamicQParms(BleMvxApplication._config2.RFID_DynamicQParms);
                BleMvxApplication._config2.RFID_DynamicQParms.retryCount = 0;

                BleMvxApplication._config2.RFID_FixedQParms.toggleTarget = (BleMvxApplication._rfMicro_Target == 2) ? 1U : 0U;
                BleMvxApplication._config2.RFID_FixedQParms.retryCount = 5;
                BleMvxApplication._reader2.rfid.SetFixedQParms(BleMvxApplication._config2.RFID_FixedQParms);
                BleMvxApplication._config2.RFID_FixedQParms.retryCount = 0;

                BleMvxApplication._reader2.rfid.SetOperationMode(BleMvxApplication._config2.RFID_OperationMode);

                BleMvxApplication._reader2.rfid.SetTagGroup(CSLibrary.Constants.Selected.ASSERTED, CSLibrary.Constants.Session.S0, CSLibrary.Constants.SessionTarget.A);

                BleMvxApplication._reader2.rfid.SetCurrentSingulationAlgorithm(BleMvxApplication._config2.RFID_Algorithm);
                BleMvxApplication._reader2.rfid.SetCurrentLinkProfile(BleMvxApplication._config2.RFID_Profile);

                CSLibrary.Structures.SelectCriterion extraSlecetion = new CSLibrary.Structures.SelectCriterion();
                extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.ASLINVA_DSLINVB, 0);
                extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.TID, 0, 28, new byte[] {0xe2, 0x82, 0x40, 0x30});
                BleMvxApplication._reader2.rfid.SetSelectCriteria(0, extraSlecetion);
                extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.NOTHING_DSLINVB, 0);
                extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0xd0, 8, new byte[] {(byte)(0x20 | BleMvxApplication._rfMicro_minOCRSSI)});
                BleMvxApplication._reader2.rfid.SetSelectCriteria(1, extraSlecetion);
                extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.NOTHING_DSLINVB, 0);
                extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0xd0, 8, new byte[] {(byte)(BleMvxApplication._rfMicro_maxOCRSSI)});
                BleMvxApplication._reader2.rfid.SetSelectCriteria(2, extraSlecetion);
                extraSlecetion.action = new CSLibrary.Structures.SelectAction(CSLibrary.Constants.Target.SELECTED, CSLibrary.Constants.Action.NOTHING_DSLINVB, 0);
                extraSlecetion.mask = new CSLibrary.Structures.SelectMask(CSLibrary.Constants.MemoryBank.BANK3, 0xe0, 0, new byte[] {0x00});
                BleMvxApplication._reader2.rfid.SetSelectCriteria(3, extraSlecetion);
                
                BleMvxApplication._reader2.rfid.Options.TagRanging.flags |= CSLibrary.Constants.SelectFlags.SELECT;
                BleMvxApplication._reader2.rfid.Options.TagRanging.multibanks = 2;
                BleMvxApplication._reader2.rfid.Options.TagRanging.bank1 = CSLibrary.Constants.MemoryBank.BANK0;
                BleMvxApplication._reader2.rfid.Options.TagRanging.offset1 = 12; // Address C
                BleMvxApplication._reader2.rfid.Options.TagRanging.count1 = 3;
                BleMvxApplication._reader2.rfid.Options.TagRanging.bank2 = CSLibrary.Constants.MemoryBank.USER;
                BleMvxApplication._reader2.rfid.Options.TagRanging.offset2 = 8;
                BleMvxApplication._reader2.rfid.Options.TagRanging.count2 = 4;
                BleMvxApplication._reader2.rfid.Options.TagRanging.compactmode = false;

                BleMvxApplication._reader2.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_PRERANGING);
            }

        }

        void StartInventory()
        {
            if ( _startInventory==false ) return;

            {
                _startInventory = false;
                _startInventoryButtonText = "Stop Inventory";
            }

            if (BleMvxApplication._reader1.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT)
            {
                SetPower(BleMvxApplication._rfMicro_Power, 1);
                BleMvxApplication._reader1.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_EXERANGING);
            }
            
            if (BleMvxApplication._reader2.Status != CSLibrary.HighLevelInterface.READERSTATE.DISCONNECT)
            {
                SetPower(BleMvxApplication._rfMicro_Power, 2);
                BleMvxApplication._reader2.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_EXERANGING);
            }

            ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.INVENTORY);
            RaisePropertyChanged(() => startInventoryButtonText);
        }

        void StopInventory()
        {
            _startInventory = true;
            _startInventoryButtonText = "Start Inventory";

            BleMvxApplication._reader1.rfid.StopOperation();
            BleMvxApplication._reader2.rfid.StopOperation();

            RaisePropertyChanged(() => startInventoryButtonText);
        }

        void StartInventoryClick()
        {
            if (_startInventory)
            {
                StartInventory(); 
                activetimer.Enabled = true; 
            }
            else
            {
                StopInventory();
                activetimer.Enabled = false;
                downtimer.Enabled = false; 
            }
        }

        void TagInventoryEvent1(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e)
        {
            if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING) return;
            if (e.info.Bank1Data == null || e.info.Bank2Data == null) return;
            InvokeOnMainThread(() => {
                AddOrUpdateTagData(e.info, 1);
                // AddUpdateData(e.info, 1);
            });
        }

        void TagInventoryEvent2(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e)
        {
            if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING) return;
            if (e.info.Bank1Data == null || e.info.Bank2Data == null) return;
            InvokeOnMainThread(() => {
                AddOrUpdateTagData(e.info, 2);
                // AddOrUpdateTagData(e.info, 2);
                // AddUpdateData(e.info, 2);
            });
        }

        void StateChangedEvent1(object sender, CSLibrary.Events.OnStateChangedEventArgs e)
        {
            switch (e.state)
            {
                case CSLibrary.Constants.RFState.IDLE:
                    ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
                    switch (BleMvxApplication._reader1.rfid.LastMacErrorCode)
                    {
                        case 0x00:  // normal end
                            break;
                        case 0x0309:
                            _userDialogs.Alert("Too near to metal, please move CS108 away from metal and start inventory again.");
                            break;
                        default:
                            _userDialogs.Alert("Mac error : 0x" + BleMvxApplication._reader1.rfid.LastMacErrorCode.ToString("X4"));
                            break;
                    }
                    break;
            }
        }

        void StateChangedEvent2(object sender, CSLibrary.Events.OnStateChangedEventArgs e)
        {
            switch (e.state)
            {
                case CSLibrary.Constants.RFState.IDLE:
                    ClassBattery.SetBatteryMode(ClassBattery.BATTERYMODE.IDLE);
                    switch (BleMvxApplication._reader2.rfid.LastMacErrorCode)
                    {
                        case 0x00:  // normal end
                            break;
                        case 0x0309:
                            _userDialogs.Alert("Too near to metal, please move CS108 away from metal and start inventory again.");
                            break;
                        default:
                            _userDialogs.Alert("Mac error : 0x" + BleMvxApplication._reader2.rfid.LastMacErrorCode.ToString("X4"));
                            break;
                    }
                    break;
            }
        }

        // private void add_update_tagdata(CSLibrary.Structures.TagCallbackInfo info, int readerID)
        // {
        //     InvokeOnMainThread(() =>
        //     {
        //         bool found = false;
        //         int cnt;

        //         UInt16 sensorCode = (UInt16)(info.Bank1Data[0] & 0x1ff);  // address c
        //         UInt16 ocRSSI     = info.Bank1Data[1];                    // address d
        //         UInt16 temp       = info.Bank1Data[2];                    // address e
        //         UInt64 caldata    = (UInt64)(((UInt64)info.Bank2Data[0] << 48) | ((UInt64)info.Bank2Data[1] << 32) | ((UInt64)info.Bank2Data[2] << 16) | ((UInt64)info.Bank2Data[3]));
        //     });
        // }

        private void AddOrUpdateTagData(CSLibrary.Structures.TagCallbackInfo info, int readerID)
        {
            InvokeOnMainThread(() =>
            {
                bool found = false;
                int cnt;

                UInt16 sensorCode = (UInt16)(info.Bank1Data[0] & 0x1ff);  // address c
                UInt16 ocRSSI     = info.Bank1Data[1];                    // address d
                UInt16 temp       = info.Bank1Data[2];                    // address e

                if (readerID==1)
                {
                    lock (TagInfoList)
                    {

                        for (cnt = 0; cnt < TagInfoList.Count; cnt++)
                        {
                            if (TagInfoList[cnt].EPC==info.epc.ToString())
                            {
                                TagInfoList[cnt].OCRSSI = ocRSSI;

                                if (ocRSSI >= BleMvxApplication._rfMicro_minOCRSSI && ocRSSI <= BleMvxApplication._rfMicro_maxOCRSSI) {
                                            if (temp >= 1300 && temp <= 3500)
                                            {
                                                TagInfoList[cnt].SucessCount++;
                                                UInt64 caldata = (UInt64)(((UInt64)info.Bank2Data[0] << 48) | ((UInt64)info.Bank2Data[1] << 32) | ((UInt64)info.Bank2Data[2] << 16) | ((UInt64)info.Bank2Data[3]));

                                                if (caldata == 0) { TagInfoList[cnt].SensorAvgValue = "NoCalData"; }
                                                else {
                                                    switch (BleMvxApplication._rfMicro_SensorUnit) {
                                                        case 0: // Code
                                                            break;
                                                        case 2: // F
                                                            break;
                                                        default: // C
                                                            double SAV = Math.Round(getTempC(temp, caldata), 2);                                               
                                                            TagInfoList[cnt].SensorAvgValue = SAV.ToString();
                                                            TagInfoList[cnt].TimeString = DateTime.Now.ToString("HH:mm:ss");

                                                            try
                                                            {
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

                                                                if (!tag_RSSI.ContainsKey(TagInfoList[cnt].EPC)) {
                                                                    List<string> t_rssi = new List<string>{TagInfoList[cnt].OCRSSI.ToString()};
                                                                    tag_RSSI.Add(TagInfoList[cnt].EPC, t_rssi);
                                                                }
                                                                else {
                                                                    tag_RSSI[TagInfoList[cnt].EPC].Add(TagInfoList[cnt].OCRSSI.ToString());
                                                                }
                                                            }
                                                            finally {}
                                                            
                                                            break;
                                                    }
                                                }
                                            }
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
                            item.DisplayName = item.EPC;
                            item.OCRSSI = ocRSSI;
                            item.SucessCount = 0;
                            item.SensorAvgValue = "";

                            if (ocRSSI >= BleMvxApplication._rfMicro_minOCRSSI && ocRSSI <= BleMvxApplication._rfMicro_maxOCRSSI) {
                                    switch (BleMvxApplication._rfMicro_SensorType) {
                                        case 0:
                                            break;
                                        default:
                                            if (temp >= 1300 && temp <= 3500) {
                                                item.SucessCount++;

                                                UInt64 caldata = (UInt64)(((UInt64)info.Bank2Data[0]<<48) | ((UInt64)info.Bank2Data[1]<<32) | ((UInt64)info.Bank2Data[2]<<16) | ((UInt64)info.Bank2Data[3]));

                                                if (caldata == 0) item.SensorAvgValue = "NoCalData";
                                                else
                                                    switch (BleMvxApplication._rfMicro_SensorUnit)
                                                    {
                                                        case 0:     // Code
                                                            break;
                                                        case 2:     // F
                                                            break;
                                                        default:    // C
                                                            double SAV = Math.Round(getTempC(temp, caldata), 2);   
                                                            item.SensorAvgValue = SAV.ToString();
                                                            item.TimeString = DateTime.Now.ToString("HH:mm:ss");

                                                            // if (epcs.Contains(item.EPC))
                                                            // {
                                                                List<string> t_time = new List<string>{ item.TimeString };
                                                                List<string> t_data = new List<string>{ item.SensorAvgValue };
                                                                List<string> t_rssi = new List<string>{ item.OCRSSI.ToString() };

                                                                try
                                                                {
                                                                    tag_Time.Add(item.EPC, t_time);
                                                                    tag_Data.Add(item.EPC, t_data);
                                                                    tag_RSSI.Add(item.EPC, t_rssi);
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
                                else {}
                                TagInfoList.Insert(0, item);
                        }
                    }
                }   // if readerID==1

                if (readerID==2)
                {
                    lock (TagInfoList2)
                    {
                        for (cnt = 0; cnt < TagInfoList2.Count; cnt++)
                        {
                            if (TagInfoList2[cnt].EPC==info.epc.ToString())
                            {
                                TagInfoList2[cnt].OCRSSI = ocRSSI;

                                if (ocRSSI >= BleMvxApplication._rfMicro_minOCRSSI && ocRSSI <= BleMvxApplication._rfMicro_maxOCRSSI) {
                                            if (temp >= 1300 && temp <= 3500)
                                            {
                                                TagInfoList2[cnt].SucessCount++;
                                                UInt64 caldata = (UInt64)(((UInt64)info.Bank2Data[0] << 48) | ((UInt64)info.Bank2Data[1] << 32) | ((UInt64)info.Bank2Data[2] << 16) | ((UInt64)info.Bank2Data[3]));

                                                if (caldata == 0) { TagInfoList2[cnt].SensorAvgValue = "NoCalData"; }
                                                else {
                                                    switch (BleMvxApplication._rfMicro_SensorUnit) {
                                                        case 0: // Code
                                                            break;
                                                        case 2: // F
                                                            break;
                                                        default: // C
                                                            double SAV = Math.Round(getTempC(temp, caldata), 2);                                               
                                                            TagInfoList2[cnt].SensorAvgValue = SAV.ToString();
                                                            TagInfoList2[cnt].TimeString = DateTime.Now.ToString("HH:mm:ss");

                                                            try {
                                                                if (!tag_List2.Contains(TagInfoList2[cnt].EPC)) { // Check Tag_List contains tags, add new data
                                                                    tag_List2.Add(TagInfoList2[cnt].EPC);
                                                                }

                                                                if (!tag_Time2.ContainsKey(TagInfoList2[cnt].EPC)) { // Check Tag_Time contains tags, add new data
                                                                    List<string> t_time = new List<string>{TagInfoList2[cnt].TimeString};
                                                                    tag_Time2.Add(TagInfoList2[cnt].EPC, t_time);
                                                                }
                                                                else {
                                                                    tag_Time2[TagInfoList2[cnt].EPC].Add(TagInfoList2[cnt].TimeString);
                                                                }

                                                                if (!tag_Data2.ContainsKey(TagInfoList2[cnt].EPC)) { // Check Tag_Data contains tags, add new data
                                                                    List<string> t_data = new List<string>{TagInfoList2[cnt].SensorAvgValue};
                                                                    tag_Data2.Add(TagInfoList2[cnt].EPC, t_data);
                                                                }
                                                                else {
                                                                    tag_Data2[TagInfoList2[cnt].EPC].Add(TagInfoList2[cnt].SensorAvgValue);
                                                                }

                                                                if (!tag_RSSI2.ContainsKey(TagInfoList2[cnt].EPC)) {
                                                                    List<string> t_rssi = new List<string>{TagInfoList2[cnt].OCRSSI.ToString()};
                                                                    tag_RSSI2.Add(TagInfoList2[cnt].EPC, t_rssi);
                                                                }
                                                                else {
                                                                    tag_RSSI2[TagInfoList2[cnt].EPC].Add(TagInfoList2[cnt].OCRSSI.ToString());
                                                                }
                                                            }
                                                            finally {}
                                                            
                                                            break;
                                                    }
                                                }
                                            }
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
                            item.DisplayName = item.EPC;
                            item.OCRSSI = ocRSSI;
                            item.SucessCount = 0;
                            item.SensorAvgValue = "";

                                if (ocRSSI >= BleMvxApplication._rfMicro_minOCRSSI && ocRSSI <= BleMvxApplication._rfMicro_maxOCRSSI) {
                                    switch (BleMvxApplication._rfMicro_SensorType) {
                                        case 0:
                                            break;
                                        default:
                                            if (temp >= 1300 && temp <= 3500) {
                                                item.SucessCount++;
                                                UInt64 caldata = (UInt64)(((UInt64)info.Bank2Data[0]<<48) | ((UInt64)info.Bank2Data[1]<<32) | ((UInt64)info.Bank2Data[2]<<16) | ((UInt64)info.Bank2Data[3]));

                                                if (caldata == 0) item.SensorAvgValue = "NoCalData";
                                                else
                                                    switch (BleMvxApplication._rfMicro_SensorUnit)
                                                    {
                                                        case 0:     // Code
                                                            break;
                                                        case 2:     // F
                                                            break;
                                                        default:    // C
                                                            double SAV = Math.Round(getTempC(temp, caldata), 2);   
                                                            item.SensorAvgValue = SAV.ToString();
                                                            item.TimeString = DateTime.Now.ToString("HH:mm:ss");

                                                            // if (epcs.Contains(item.EPC))
                                                            // {
                                                                List<string> t_time = new List<string>{ item.TimeString };
                                                                List<string> t_data = new List<string>{ item.SensorAvgValue };
                                                                List<string> t_rssi = new List<string>{ item.OCRSSI.ToString() };

                                                                try
                                                                {
                                                                    tag_Time2.Add(item.EPC, t_time);
                                                                    tag_Data2.Add(item.EPC, t_data);
                                                                    tag_RSSI2.Add(item.EPC, t_rssi);
                                                                    tag_List2.Add(item.EPC);
                                                                }
                                                                finally {}
                                                            // }
                                                            break;
                                                    }
                                            }
                                            break;
                                    }
                                }
                                else {}
                                TagInfoList2.Insert(0, item);
                        }
                    }
                }   // if readerID==2

            });
        }

        private void ShareDataButtonClick() // Function for Sharing time series data from tags
        {
            InvokeOnMainThread(() =>
            {
                // string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tags.txt");
                // using (StreamWriter writer = new StreamWriter(fileName, true)) {

                // var file = await Xamarin.Essentials.FilePicker.PickAsync();
                // using (StreamWriter writer = new StreamWriter(file.FileName, true)) {

                //     foreach (string name in tag_List)
                //     {
                //         writer.WriteLine(name + "\n" + "[");
                //         foreach (var i in tag_Time[name]) { writer.WriteLine(i); }
                //         writer.WriteLine("]\n[");
                //         foreach (var j in tag_Data[name]) { writer.WriteLine(j); }
                //         writer.WriteLine("]\n ");
                //     }
                // }

            });
        }

        private void AutoSaveData() // Function for Sharing time series data from tags
        {
            InvokeOnMainThread(()=>
            {
                string fileName = pick_result.FullPath;    // Get file name from picker
                string rssiName = rssi_result.FullPath;    // Get file name from picker

                string fileName2 = pick_result2.FullPath;    // Get file name from picker
                string rssiName2 = rssi_result2.FullPath;    // Get file name from picker

                // string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fpath);
                // string rssiName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), rssipath);
                // for UWP cannot use filepicker, use local folder instead

                File.WriteAllText(fileName, String.Empty); // Empty text file to rewrite database
                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    foreach (string name in tag_List)
                    {
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

                File.WriteAllText(fileName2, String.Empty); // Empty text file to rewrite database
                using (StreamWriter writer = new StreamWriter(fileName2, true))
                {
                    foreach (string name in tag_List2)
                    {
                        writer.WriteLine(name + "\n" + "[");
                        foreach (var i in tag_Time2[name]) { writer.WriteLine(i); }
                        writer.WriteLine("]\n[");
                        foreach (var j in tag_Data2[name]) { writer.WriteLine(j); }
                        writer.WriteLine("]\n ");
                    }
                    writer.Close();
                }

                File.WriteAllText(rssiName2, String.Empty); // Empty text file to rewrite database
                using (StreamWriter writer = new StreamWriter(rssiName2, true)) {
                    foreach (string name in tag_List2) {
                        writer.WriteLine(name + "\n" + "[");
                        foreach (var i in tag_Time2[name]) { writer.WriteLine(i); }
                        writer.WriteLine("]\n[");
                        foreach (var j in tag_RSSI2[name]) { writer.WriteLine(j); }
                        writer.WriteLine("]\n ");
                    }
                    writer.Close();
                }

            });
        }

        #region Key_Event
        void HotKeys_OnKeyEvent(object sender, CSLibrary.Notification.HotKeyEventArgs e)
        {
            if (e.KeyCode == CSLibrary.Notification.Key.BUTTON)
            {
                if (e.KeyDown) { StartInventory(); }
                else           { StopInventory(); }
            }
        }

        async void ShowDialog(string Msg)
        {
            var config = new ProgressDialogConfig()
            {
                Title = Msg,
                IsDeterministic = true,
                MaskType = MaskType.Gradient,
            };

            using (var progress = _userDialogs.Progress(config))
            {
                progress.Show();
                await System.Threading.Tasks.Task.Delay(1000);
            }
        }
        #endregion


    }
}
    
