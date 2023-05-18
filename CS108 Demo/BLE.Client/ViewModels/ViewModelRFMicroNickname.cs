using System.Collections.ObjectModel;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions.Contracts;
using Prism.Mvvm;


namespace BLE.Client.ViewModels
{
    public class RFMicroTagNicknameViewModel : BindableBase
    {
        private string _EPC;
        public string EPC { get { return this._EPC; } set { this.SetProperty(ref this._EPC, value); } }

        private string _Nickname;
        public string Nickname { get { return this._Nickname; } set { this.SetProperty(ref this._Nickname, value); } }
    }

    public class ViewModelRFMicroNickname : BaseViewModel
    {

        private readonly IUserDialogs _userDialogs;

        #region -------------- RFID inventory -----------------
        
        static public ObservableCollection<RFMicroTagNicknameViewModel> _TagNicknameList = new ObservableCollection<RFMicroTagNicknameViewModel>();
        public ObservableCollection<RFMicroTagNicknameViewModel> TagNicknameList {get {return _TagNicknameList;} set {SetProperty(ref _TagNicknameList, value);}}

        #endregion

        public ViewModelRFMicroNickname(IAdapter adapter, IUserDialogs userDialogs) : base(adapter) {
            _userDialogs = userDialogs;
        }

        ~ViewModelRFMicroNickname() {}

        public override void Resume() {
            base.Resume();
        }

        public override void Suspend() {
            base.Suspend();
        }

        protected override void InitFromBundle(IMvxBundle parameters) {
            base.InitFromBundle(parameters);
        }

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
