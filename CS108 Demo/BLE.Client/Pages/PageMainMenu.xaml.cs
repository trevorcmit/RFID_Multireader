// using System;
// using System.Collections.Generic;
// using System.ComponentModel;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using Xamarin.Forms;
// using Xamarin.Forms.Xaml;


namespace BLE.Client.Pages {
    public partial class PageMainMenu {
        public PageMainMenu() {
            InitializeComponent();
            this.Title = "IFM fID-T";
            // Task.Run(AnimateBackground);
        }

        // private async void AnimateBackground() {
        //     Action<double> forward = input => bdGradient.AnchorY = input;
        //     Action<double> backward = input => bdGradient.AnchorY = input;

        //     while (true) {
        //         bdGradient.Animate(name: "forward", callback: forward, start: 0, end: 1, length: 5000, easing: Easing.SinIn);
        //         await Task.Delay(5000);
        //         bdGradient.Animate(name: "backward", callback: backward, start: 1, end: 0, length: 5000, easing: Easing.SinIn);
        //         await Task.Delay(5000);
        //     }
        // }

    }
}