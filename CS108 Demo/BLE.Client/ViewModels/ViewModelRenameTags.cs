using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using Xamarin.Forms;
using Plugin.BLE.Abstractions;
using Plugin.Settings.Abstractions;
using Plugin.Permissions.Abstractions;


namespace BLE.Client.ViewModels {
    public class ViewModelRenameTags : BaseViewModel {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly IUserDialogs _userDialogs;
        public ICommand TagSetOneCommand { protected set; get; }
        public ICommand TagSetTwoCommand { protected set; get; }
        public ICommand TagSetThreeCommand { protected set; get; }
        public ICommand TagSetFourCommand { protected set; get; }

        private string _TagOneColor;
        public string TagOneColor {get => _TagOneColor; set {_TagOneColor = value; OnPropertyChanged("TagOneColor");}}

        private string _TagOneText;
        public string TagOneText {get => _TagOneText; set {_TagOneText = value; OnPropertyChanged("TagOneText");}}

        public ViewModelRenameTags(IAdapter adapter, IUserDialogs userDialogs) : base(adapter) {
            _userDialogs = userDialogs;

            TagOneColor = "#ba0900";
            TagOneText = "Tag Set #1: No Name";

            TagSetOneCommand   = new Command(TagSetOne);
            TagSetTwoCommand   = new Command(TagSetTwo);
            TagSetThreeCommand = new Command(TagSetThree);
            TagSetFourCommand  = new Command(TagSetFour);
        }

        ~ViewModelRenameTags() {}

        async void TagSetOne() {
            string tn = await Application.Current.MainPage.DisplayPromptAsync( // Get tag name
                title: "Assign name to Tags labeled #1", 
                message: "Type in name.",
                placeholder: "Example: Gabriel"
            );

            _TagOneColor = "#008c17";
            _TagOneText = "Tag Set #1: " + tn;
            RaisePropertyChanged(() => TagOneColor);
            RaisePropertyChanged(() => TagOneText);
        }

        async void TagSetTwo() {}

        async void TagSetThree() {}

        async void TagSetFour() {}

    }
}