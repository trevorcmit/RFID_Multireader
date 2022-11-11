using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSLibrary;


namespace CS108DesktopDemo {
    public partial class FormMain : Form {
        HighLevelInterface _reader = new HighLevelInterface();

        public FormMain() {
            InitializeComponent();

            CSLibrary.DeviceFinder.OnSearchCompleted += DeviceWatcher_Added;
        }

        private void button2_Click(object sender, EventArgs e) {
            if (listView1.SelectedIndices.Count < 1) System.Console.WriteLine("Please select reader first!");
            else {
                textBox3.Text = "Please wait, connecting..." + Environment.NewLine;
                _reader.rfid.OnStateChanged += new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent);
                _reader.ConnectAsync(CSLibrary.DeviceFinder.GetDeviceInformation(listView1.SelectedIndices[0]));
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            listView1.Clear();
            CSLibrary.DeviceFinder.SearchDevice();
        }

        private async void DeviceWatcher_Added(object sender, object deviceInfo) {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this) {
                    CSLibrary.DeviceFinder.DeviceFinderArgs dfa = (CSLibrary.DeviceFinder.DeviceFinderArgs)deviceInfo;
                    CSLibrary.DeviceFinder.DeviceInfomation di = (CSLibrary.DeviceFinder.DeviceInfomation)dfa.Found;

                    string a = String.Format("Added  {0} {1}", di.ID, di.deviceName);
                    Debug.WriteLine(a);
                    listView1.Items.Add(di.deviceName);

                }
            }
        }

        private void button3_Click(object sender, EventArgs e) {
            _reader.rfid.SetHoppingChannels(CSLibrary.Constants.RegionCode.HK);

            if (checkBox_TID.Checked) {
                _reader.rfid.Options.TagRanging.multibanks = 1;
                _reader.rfid.Options.TagRanging.bank1 = CSLibrary.Constants.MemoryBank.TID;
                _reader.rfid.Options.TagRanging.offset1 = 0;
                _reader.rfid.Options.TagRanging.count1 = 2;
            }
            else {
                _reader.rfid.Options.TagRanging.multibanks = 0;
            }

            _reader.rfid.OnAsyncCallback += new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
            _reader.rfid.Options.TagRanging.flags = 0;
            _reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_RANGING);
        }

        void StateChangedEvent(object sender, CSLibrary.Events.OnStateChangedEventArgs e) {
            if (e.state == CSLibrary.Constants.RFState.INITIALIZATION_COMPLETE) {
                textBox3.Text += "Connected" + Environment.NewLine;
            }
        }

        void TagInventoryEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e) {
            if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING) return;

            if (textBox2.Text.Length > 30000) textBox2.Text = "";

            if (checkBox_TID.Checked) {
                textBox2.Text = "EPC:" + e.info.epc + " TID:" + CSLibrary.Tools.Hex.ToString(e.info.Bank1Data) + Environment.NewLine + textBox2.Text;
            }
            else {
                textBox2.Text = "EPC:" + e.info.epc + Environment.NewLine + textBox2.Text;
            }
        }

        private void button4_Click(object sender, EventArgs e) {
            _reader.rfid.StopOperation();
            _reader.rfid.OnAsyncCallback -= new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
        }

        private void button5_Click(object sender, EventArgs e) {
            _reader.rfid.OnStateChanged -= new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent);
            _reader.DisconnectAsync();
            textBox3.Text += "Please wait to disconnect CS108, BT led will change to flash" + Environment.NewLine;
        }

        private void button6_Click(object sender, EventArgs e) {
            textBox2.Text = "";
        }

    }
}
