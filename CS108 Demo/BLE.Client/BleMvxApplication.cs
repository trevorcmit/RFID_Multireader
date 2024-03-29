﻿using System;
using System.Threading.Tasks;
using BLE.Client.ViewModels;
using MvvmCross.Core.ViewModels;
using PCLStorage;
using Newtonsoft.Json;
using CSLibrary;


namespace BLE.Client {
    public class CONFIG {
        public class MAINMENUSHORTCUT {
            public enum FUNCTION {
                NONE,
                INVENTORY,
                BARCODE,
            }

            public FUNCTION Function = FUNCTION.NONE;
            public uint DurationMin = 0;
            public uint DurationMax = 0;
        }

        public string readerID = "";
        public CSLibrary.Constants.Machine readerModel = CSLibrary.Constants.Machine.UNKNOWN;

        public int BatteryLevelIndicatorFormat = 1; // 0 = voltage, other = percentage 

        public uint RFID_Profile;
        public int RFID_TagDelayTime;
        public CSLibrary.Constants.RadioOperationMode RFID_OperationMode;
        public uint RFID_TagPopulation;
        public bool RFID_ToggleTarget = true;
        public CSLibrary.Structures.TagGroup RFID_TagGroup;
        public CSLibrary.Constants.SingulationAlgorithm RFID_Algorithm;
        public CSLibrary.Structures.DynamicQParms RFID_DynamicQParms;
        public CSLibrary.Structures.FixedQParms RFID_FixedQParms;

        public CSLibrary.Constants.RegionCode RFID_Region = CSLibrary.Constants.RegionCode.UNKNOWN;
        public int RFID_FrequenceSwitch = 0; // 0 = hopping, 1 = fixed, 2 = agile
        public uint RFID_FixedChannel = 0;

        // Multi Bank Inventory Setting
        public bool RFID_MBI_MultiBank1Enable;
        public CSLibrary.Constants.MemoryBank RFID_MBI_MultiBank1;
        public UInt16 RFID_MBI_MultiBank1Offset;
        public UInt16 RFID_MBI_MultiBank1Count;
        public bool RFID_MBI_MultiBank2Enable;
        public CSLibrary.Constants.MemoryBank RFID_MBI_MultiBank2;
        public UInt16 RFID_MBI_MultiBank2Offset;
        public UInt16 RFID_MBI_MultiBank2Count;

        // Main Menu Shortcut
        public MAINMENUSHORTCUT[] RFID_Shortcut = new MAINMENUSHORTCUT[6];

        public bool RFID_InventoryAlertSound = true;
        public bool RFID_QOverride = false;
        public bool RFID_DBm = true;

        // Backend Server
        public bool RFID_SavetoFile = false;
        public bool RFID_SavetoCloud = true;
        public int RFID_CloudProtocol = 0;
        public string RFID_IPAddress;

        public bool RFID_Vibration = false;
        public bool RFID_VibrationTag = false;      // false = New, true = All
        public uint RFID_VibrationWindow = 2;      // 2 seconds
        public uint RFID_VibrationTime = 300;       // 300 ms

        public bool[] RFID_AntennaEnable = new bool[16] {true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false};
        public uint[] RFID_Antenna_Power = new uint[16] {300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300};
        public uint[] RFID_Antenna_Dwell = new uint[16] {2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000};

        public int RFID_PowerSequencing_NumberofPower = 0;
        public uint[] RFID_PowerSequencing_Level = new uint[16] {300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300, 300};
        public uint[] RFID_PowerSequencing_DWell = new uint[16] {2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000, 2000};

        public bool RFID_NewTagLocation = false;
        public int RFID_ShareFormat = 0;  // 0 = JSON, 1 = CSV
        public bool RFID_Focus = false;

        public uint RFID_BatteryPollingTime = 300;

        // LNA setting
        public int RFID_RFLNAcompression = 1;
        public int RFID_RFLNAGain = 1;
        public int RFID_IFLNAGain = 24;
        public int RFID_AGCGain = -6;

        public CONFIG() {
            int port = 16;
            RFID_TagPopulation = 30;
            RFID_TagDelayTime = 0;
            RFID_AntennaEnable = new bool[port];
            RFID_Antenna_Power = new uint[port];
            RFID_Antenna_Dwell = new uint[port];

            for (uint cnt = 0; cnt < port; cnt ++) {
                RFID_Antenna_Power[cnt] = 300;

                if (cnt == 0) {
                    RFID_AntennaEnable[0] = true;
					if (port == 1)
						RFID_Antenna_Dwell[0] = 0;
					else
                        RFID_Antenna_Dwell[0] = 2000;
                }
                else {
                    RFID_AntennaEnable[cnt] = false;
                    RFID_Antenna_Dwell[cnt] = 2000;
                }
            }

            RFID_OperationMode = CSLibrary.Constants.RadioOperationMode.CONTINUOUS;
            RFID_TagGroup = new CSLibrary.Structures.TagGroup(CSLibrary.Constants.Selected.ALL, CSLibrary.Constants.Session.S0, CSLibrary.Constants.SessionTarget.A);
            RFID_Algorithm = CSLibrary.Constants.SingulationAlgorithm.DYNAMICQ;
            RFID_Profile = 1;

            RFID_DynamicQParms = new CSLibrary.Structures.DynamicQParms();
            RFID_DynamicQParms.minQValue = 0;
            RFID_DynamicQParms.startQValue = 6;
            RFID_DynamicQParms.maxQValue = 15;
            RFID_DynamicQParms.toggleTarget = 1;
            RFID_DynamicQParms.thresholdMultiplier = 4;
            RFID_DynamicQParms.retryCount = 0;

            RFID_FixedQParms = new CSLibrary.Structures.FixedQParms();
            RFID_FixedQParms.qValue = 6;
            RFID_FixedQParms.retryCount = 0;
            RFID_FixedQParms.toggleTarget = 1;
            RFID_FixedQParms.repeatUntilNoTags = 0;

            RFID_MBI_MultiBank1Enable = false;
            RFID_MBI_MultiBank2Enable = false;
            RFID_MBI_MultiBank1 = CSLibrary.Constants.MemoryBank.TID;
            RFID_MBI_MultiBank1Offset = 0;
            RFID_MBI_MultiBank1Count = 2;
            RFID_MBI_MultiBank2 = CSLibrary.Constants.MemoryBank.USER;
            RFID_MBI_MultiBank2Offset = 0;
            RFID_MBI_MultiBank2Count = 2;

            RFID_InventoryAlertSound = true;
            RFID_QOverride = false;
            RFID_DBm = true;

            RFID_SavetoFile = false;
            RFID_SavetoCloud = true;
            RFID_CloudProtocol = 0;
            RFID_IPAddress = "";

            RFID_Vibration = false;
            RFID_VibrationTag = false;      // false = New, true = All
            RFID_VibrationWindow = 2;      // 2 seconds
            RFID_VibrationTime = 300;       // 500 ms

            RFID_BatteryPollingTime = 300;  // 300s


            for (int cnt = 0; cnt < RFID_Shortcut.Length; cnt++) {
                MAINMENUSHORTCUT item = new MAINMENUSHORTCUT();

                switch (cnt) {
                    case 0:
                        item.Function = MAINMENUSHORTCUT.FUNCTION.INVENTORY;
                        item.DurationMin = 0;
                        item.DurationMax = 500;
                        break;
                    case 1:
                        item.Function = MAINMENUSHORTCUT.FUNCTION.BARCODE;
                        item.DurationMin = 500;
                        item.DurationMax = 10000;
                        break;
                }

                RFID_Shortcut[cnt] = item;
            }
        }
    }

    public class BleMvxApplication : MvxApplication {
        static public HighLevelInterface _reader1 = new HighLevelInterface();
        static public HighLevelInterface _reader2 = new HighLevelInterface();
        static public HighLevelInterface _reader3 = new HighLevelInterface();
        static public HighLevelInterface _reader4 = new HighLevelInterface();

        public static CONFIG _config1;
        public static CONFIG _config2;
        public static CONFIG _config3;
        public static CONFIG _config4;

        // for Geiger and Read/Write
        public static string _SELECT_EPC = "";
        public static string _SELECT_TID = "";
        public static UInt16 _SELECT_PC = 0x0000;

        // for PreFilter
        public static string _PREFILTER_MASK_EPC = "";
        public static uint _PREFILTER_MASK_Offset = 0;
        public static int _PREFILTER_MASK_Truncate = 0;
        public static int _PREFILTER_Bank = 1;
        public static bool _PREFILTER_Enable = false;

        // for Post Filter
        public static string _POSTFILTER_MASK_EPC = "";
        public static uint _POSTFILTER_MASK_Offset = 0;
        public static bool _POSTFILTER_MASK_MatchNot = false;
        public static bool _POSTFILTER_MASK_Enable = false;

        public static int _inventoryEntryPoint = 0;
        public static bool _settingPage1TagPopulationChanged = false;

        // for battery level display
        public static bool _batteryLow = false;

        // for RFMicro
        public static int _rfMicro_TagType; // 0 = S2, 1 = S3, 2 = Axzon
        public static int _rfMicro_Power; // 0 ~ 4
        public static int _rfMicro_Target; // 0 = A, 1 = B, 2 = Toggle
        public static int _rfMicro_SensorType; // 0=Sensor code, 1=Temperature
        public static int _rfMicro_SensorUnit; // 0 = Average value, 1 = RAW, 2 = Temperature F, 3 = Temperature C, 4 = Dry/Wet
        public static int _rfMicro_minOCRSSI;
        public static int _rfMicro_maxOCRSSI;
        public static int _rfMicro_thresholdComparison; // 0 ~ 1
        public static int _rfMicro_thresholdValue;
        public static string _rfMicro_thresholdColor;
        public static int _rfMicro_WetDryThresholdValue;

        //for ColdChain
        public static int _coldChain_TempOffset;
        public static int _coldChain_Temp1THUnder;
        public static int _coldChain_Temp1THOver;
        public static int _coldChain_Temp1THCount;
        public static int _coldChain_Temp2THUnder;
        public static int _coldChain_Temp2THOver;
        public static int _coldChain_Temp2THCount;
        public static int _coldChain_LogInterval;

        // for Geiger Demo 
        public static int _geiger_Bank = 1;

        // for Large Content
        public static string _LargeContent = "";

        public override void Initialize() {
            RFMicroTagNicknameViewModel item;

            item = new RFMicroTagNicknameViewModel(); item.EPC = "477265794861743030303031"; item.Nickname = "Black Hat"; ViewModelRFMicroNickname._TagNicknameList.Add(item);
            item = new RFMicroTagNicknameViewModel(); item.EPC = "426C61636B48617446726F6E"; item.Nickname = "White Hat"; ViewModelRFMicroNickname._TagNicknameList.Add(item);

            item = new RFMicroTagNicknameViewModel(); item.EPC = "E282403E000207D6F9770273"; item.Nickname = "Left Quad";   ViewModelRFMicroNickname._TagNicknameList.Add(item);
            item = new RFMicroTagNicknameViewModel(); item.EPC = "E282403E000207D6F97786D2"; item.Nickname = "Right Quad";  ViewModelRFMicroNickname._TagNicknameList.Add(item);
            item = new RFMicroTagNicknameViewModel(); item.EPC = "E282403E000207D6F9773098"; item.Nickname = "Left Glute";  ViewModelRFMicroNickname._TagNicknameList.Add(item);
            item = new RFMicroTagNicknameViewModel(); item.EPC = "E282403E000207D6F97748C0"; item.Nickname = "Right Glute"; ViewModelRFMicroNickname._TagNicknameList.Add(item);
            item = new RFMicroTagNicknameViewModel(); item.EPC = "E282403E000207D6F9779863"; item.Nickname = "Left Calf";   ViewModelRFMicroNickname._TagNicknameList.Add(item);
            item = new RFMicroTagNicknameViewModel(); item.EPC = "E282403E000207D6F9775A25"; item.Nickname = "Right Calf";  ViewModelRFMicroNickname._TagNicknameList.Add(item);

            RegisterAppStart<ViewModelMainMenu>();
        }

        static public async Task<bool> LoadConfig(string readerID, int n) {
            if (n==1) {
                try {
                    IFolder rootFolder = FileSystem.Current.LocalStorage;
                    IFolder sourceFolder = await FileSystem.Current.LocalStorage.CreateFolderAsync("CSLReader", CreationCollisionOption.OpenIfExists);
                    IFile sourceFile = await sourceFolder.CreateFileAsync(readerID + ".cfg", CreationCollisionOption.OpenIfExists);

                    var contentJSON = await sourceFile.ReadAllTextAsync();
                    var setting = JsonConvert.DeserializeObject<CONFIG>(contentJSON);

                    if (setting != null) {
                        _config1 = setting;
                        return true;
                    }
                    else {
                        _config1 = new CONFIG();
                    }
                }
                catch (Exception ex) {}
                return false;
            }
            else if (n==2) {
                try {
                    IFolder rootFolder = FileSystem.Current.LocalStorage;
                    IFolder sourceFolder = await FileSystem.Current.LocalStorage.CreateFolderAsync("CSLReader", CreationCollisionOption.OpenIfExists);
                    IFile sourceFile = await sourceFolder.CreateFileAsync(readerID + ".cfg", CreationCollisionOption.OpenIfExists);

                    var contentJSON = await sourceFile.ReadAllTextAsync();
                    var setting = JsonConvert.DeserializeObject<CONFIG>(contentJSON);

                    if (setting != null) {
                        _config2 = setting;
                        return true;
                    }
                    else {
                        _config2 = new CONFIG();
                    }
                }
                catch (Exception ex) {}
                return false;
            }
            else if (n==3) {
                try {
                    IFolder rootFolder = FileSystem.Current.LocalStorage;
                    IFolder sourceFolder = await FileSystem.Current.LocalStorage.CreateFolderAsync("CSLReader", CreationCollisionOption.OpenIfExists);
                    IFile sourceFile = await sourceFolder.CreateFileAsync(readerID + ".cfg", CreationCollisionOption.OpenIfExists);

                    var contentJSON = await sourceFile.ReadAllTextAsync();
                    var setting = JsonConvert.DeserializeObject<CONFIG>(contentJSON);

                    if (setting != null) {
                        _config3 = setting;
                        return true;
                    }
                    else {
                        _config3 = new CONFIG();
                    }
                }
                catch (Exception ex) {}
                return false;
            }
            else if (n==4) {
                try {
                    IFolder rootFolder = FileSystem.Current.LocalStorage;
                    IFolder sourceFolder = await FileSystem.Current.LocalStorage.CreateFolderAsync("CSLReader", CreationCollisionOption.OpenIfExists);
                    IFile sourceFile = await sourceFolder.CreateFileAsync(readerID + ".cfg", CreationCollisionOption.OpenIfExists);

                    var contentJSON = await sourceFile.ReadAllTextAsync();
                    var setting = JsonConvert.DeserializeObject<CONFIG>(contentJSON);

                    if (setting != null) {
                        _config4 = setting;
                        return true;
                    }
                    else {
                        _config4 = new CONFIG();
                    }
                }
                catch (Exception ex) {}
                return false;
            }
            else
            {
                return false;
            }
        }

        static async public void SaveConfig(int n) {
            // Integer input 'n' represents number of reader instance (i.e. n=1 is BleMvxApplication._reader1)
            if (n==1) { // FOR READER #1
                IFolder rootFolder = FileSystem.Current.LocalStorage;
                IFolder sourceFolder = await FileSystem.Current.LocalStorage.CreateFolderAsync("CSLReader", CreationCollisionOption.OpenIfExists);
                IFile sourceFile = await sourceFolder.CreateFileAsync(_config1.readerID + ".cfg", CreationCollisionOption.ReplaceExisting);
                string contentJSON = JsonConvert.SerializeObject(_config1);
                await sourceFile.WriteAllTextAsync(contentJSON);
            }
            else if (n==2) { // FOR READER #2
                IFolder rootFolder = FileSystem.Current.LocalStorage;
                IFolder sourceFolder = await FileSystem.Current.LocalStorage.CreateFolderAsync("CSLReader", CreationCollisionOption.OpenIfExists);
                IFile sourceFile = await sourceFolder.CreateFileAsync(_config2.readerID + ".cfg", CreationCollisionOption.ReplaceExisting);
                string contentJSON = JsonConvert.SerializeObject(_config2);
                await sourceFile.WriteAllTextAsync(contentJSON);
            }
            else if (n==3) { // FOR READER #3
                IFolder rootFolder = FileSystem.Current.LocalStorage;
                IFolder sourceFolder = await FileSystem.Current.LocalStorage.CreateFolderAsync("CSLReader", CreationCollisionOption.OpenIfExists);
                IFile sourceFile = await sourceFolder.CreateFileAsync(_config3.readerID + ".cfg", CreationCollisionOption.ReplaceExisting);
                string contentJSON = JsonConvert.SerializeObject(_config3);
                await sourceFile.WriteAllTextAsync(contentJSON);
            }
            else if (n==4) { // FOR READER #4
                IFolder rootFolder = FileSystem.Current.LocalStorage;
                IFolder sourceFolder = await FileSystem.Current.LocalStorage.CreateFolderAsync("CSLReader", CreationCollisionOption.OpenIfExists);
                IFile sourceFile = await sourceFolder.CreateFileAsync(_config4.readerID + ".cfg", CreationCollisionOption.ReplaceExisting);
                string contentJSON = JsonConvert.SerializeObject(_config4);
                await sourceFile.WriteAllTextAsync(contentJSON);
            }
        }

        static public void ResetConfig(int n, uint port=1) {
            if (n==1) {
                var readerID = _config1.readerID;
                var readerModel = _config1.readerModel;
                _config1 = new CONFIG();
                _config1.readerID = readerID;
                _config1.readerModel = readerModel;
            }
            else if (n==2) {
                var readerID = _config2.readerID;
                var readerModel = _config2.readerModel;
                _config2 = new CONFIG();
                _config2.readerID = readerID;
                _config2.readerModel = readerModel;
            }
            else if (n==3) {
                var readerID = _config3.readerID;
                var readerModel = _config3.readerModel;
                _config3 = new CONFIG();
                _config3.readerID = readerID;
                _config3.readerModel = readerModel;
            }
            else if (n==4) {
                var readerID = _config4.readerID;
                var readerModel = _config4.readerModel;
                _config4 = new CONFIG();
                _config4.readerID = readerID;
                _config4.readerModel = readerModel;
            }
        }

    }
}
