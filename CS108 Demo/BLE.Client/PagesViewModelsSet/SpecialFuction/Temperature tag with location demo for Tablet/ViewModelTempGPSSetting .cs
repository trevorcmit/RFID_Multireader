using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using Plugin.BLE.Abstractions.Contracts;

using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace BLE.Client.ViewModels
{
    public class ViewModelTempGPSSetting : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string buttonPowerText { get; set; }
        public string buttonTargetText { get; set; }
        public string buttonIndicatorsProfileText { get; set; }
        public string buttonSensorTypeText { get; set; }
        public string buttonSensorUnitText { get; set; }
        public string entryMinOCRSSIText { get; set; }
        public string entryMaxOCRSSIText { get; set; }
        public ICommand OnOKButtonCommand { protected set; get; }
        public ICommand OnNicknameButtonCommand { protected set; get; }

        public ViewModelTempGPSSetting(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnOKButtonCommand = new Command(OnOKButtonClicked);
            OnNicknameButtonCommand = new Command(OnNicknameButtonClicked);
        }

        public override void Resume()
        {
            base.Resume();
        }

        public override void Suspend()
        {
            base.Suspend();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);
        }

        void OnNicknameButtonClicked(object ind)
        {
            ShowViewModel<ViewModelRFMicroNickname>(new MvxBundle());
        }

        void OnOKButtonClicked(object ind)
        {
            if (ind != null)
                if ((int)ind == 1)
                    switch (BleMvxApplication._rfMicro_TagType)
                    {
                        case 0: // S2
                            ShowViewModel<ViewModelRFMicroS2Inventory>(new MvxBundle());
                            break;

                        case 1: // S3
                            ShowViewModel<ViewModelTempGPSInventory>(new MvxBundle());
                            break;
                    }
        }
    }
}
