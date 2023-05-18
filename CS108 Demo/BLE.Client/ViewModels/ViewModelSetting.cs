using System;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions.Contracts;


namespace BLE.Client.ViewModels
{
    public class ViewModelSetting : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public ViewModelSetting(IAdapter adapter, IUserDialogs userDialogs) : base(adapter) {
            _userDialogs = userDialogs;
        }

        public override void Resume() {
            base.Resume();
            BleMvxApplication._reader1.siliconlabIC.OnAccessCompleted += new EventHandler<CSLibrary.SiliconLabIC.Events.OnAccessCompletedEventArgs>(OnAccessCompletedEvent);
        }

        public override void Suspend() {
            BleMvxApplication._reader1.siliconlabIC.OnAccessCompleted -= new EventHandler<CSLibrary.SiliconLabIC.Events.OnAccessCompletedEventArgs>(OnAccessCompletedEvent);
            base.Suspend();
        }

        protected override void InitFromBundle(IMvxBundle parameters) {
            base.InitFromBundle(parameters);
        }

        void OnAccessCompletedEvent(object sender, CSLibrary.SiliconLabIC.Events.OnAccessCompletedEventArgs e) {
            InvokeOnMainThread(() => {
                switch (e.type) {
                    case CSLibrary.SiliconLabIC.Constants.AccessCompletedCallbackType.SERIALNUMBER:
                        _userDialogs.Alert("Serial Number : " + (string)e.info);
                        break;
                }
            });
        }

    }
}
