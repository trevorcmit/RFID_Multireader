using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CS108XamarinFormsDemo.Views;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace CS108XamarinFormsDemo {
    public partial class App : Application {

        // var ble = Plugin.BLE.CrossBluetoothLE.Current;
        // var adapter = Plugin.BLE.CrossBluetoothLE.Current.Adapter;

        public App() {
            InitializeComponent();
            MainPage = new PageMainMenu();
        }

        protected override void OnStart() {
            // Handle when your app starts
        }

        protected override void OnSleep() {
            // Handle when your app sleeps
        }

        protected override void OnResume() {
            // Handle when your app resumes
        }
    }
}
