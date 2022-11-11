using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.Forms.Uwp.Presenters;
using MvvmCross.Platform;


namespace BLE.Client.UWP {
    public sealed partial class MainPage : Xamarin.Forms.Platform.UWP.WindowsPage {
        public MainPage() {
            this.InitializeComponent();

            var startup = Mvx.Resolve<IMvxAppStart>();
            startup.Start();

            var presenter = Mvx.Resolve<IMvxViewPresenter>() as MvxFormsUwpPagePresenter;
            LoadApplication(presenter.FormsApplication);
        }

    }
}
