using Acr.UserDialogs;
using Android.App;
using Android.Content.PM;
using Android.OS;
using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.Forms.Droid.Presenters;
using MvvmCross.Platform;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


namespace BLE.Client.Droid {
    [Activity(ScreenOrientation = ScreenOrientation.User)]
    public class MainActivity : FormsAppCompatActivity {
        protected override void OnCreate(Bundle bundle) {
            ToolbarResource = Resource.Layout.toolbar;
            TabLayoutResource = Resource.Layout.tabs;

            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate {
                return true;
            };

            base.OnCreate(bundle);
            Xamarin.Essentials.Platform.Init(this, bundle); // add this line to your code, it may also be called: bundle

            if (Device.Idiom == TargetIdiom.Phone)
                this.RequestedOrientation = ScreenOrientation.Portrait;
            else
                this.RequestedOrientation = ScreenOrientation.Landscape;

            UserDialogs.Init(this);
            Forms.Init(this, bundle);
            var formsApp = new BleMvxFormsApp();
            LoadApplication(formsApp);

            var presenter = (MvxFormsDroidPagePresenter)Mvx.Resolve<IMvxViewPresenter>();
            presenter.FormsApplication = formsApp;
            Mvx.Resolve<IMvxAppStart>().Start();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults) {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

    }
}
