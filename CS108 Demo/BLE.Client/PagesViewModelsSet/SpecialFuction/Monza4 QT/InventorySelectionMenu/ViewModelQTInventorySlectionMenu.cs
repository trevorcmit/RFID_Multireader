using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using System.Windows.Input;
using Xamarin.Forms;
using Plugin.BLE.Abstractions.Contracts;


namespace BLE.Client.ViewModels {
    public class ViewModelQTInventorySlectionMenu : BaseViewModel {
        private readonly IUserDialogs _userDialogs;

        public ICommand OnPublicModeInventoryButtonCommand { protected set; get; }
        public ICommand OnPrivateModeInventoryButtonCommand { protected set; get; }


        public ViewModelQTInventorySlectionMenu(IAdapter adapter, IUserDialogs userDialogs) : base(adapter) {
            _userDialogs = userDialogs;

            OnPublicModeInventoryButtonCommand = new Command(OnPublicModeInventoryButtonClicked);
            OnPrivateModeInventoryButtonCommand = new Command(OnPrivateModeInventoryButtonClicked);
        }

        public override void Resume() {
            base.Resume();

            BleMvxApplication._reader.rfid.CancelAllSelectCriteria();
        }

        void OnPublicModeInventoryButtonClicked() {
            ShowViewModel<ViewModelQTPublicModeInventory>(new MvxBundle());
        }

        void OnPrivateModeInventoryButtonClicked() {
            ShowViewModel<ViewModelQTPrivateModeInventory>(new MvxBundle());
        }
    }
}
