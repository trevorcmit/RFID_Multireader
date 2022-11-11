using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using CS108XamarinFormsDemo.Models;

namespace CS108XamarinFormsDemo.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PageMainMenu : ContentPage
    {
        public PageMainMenu()
        {
            InitializeComponent();
            this.Title = "CS108 RFID Reader (C# " + DependencyService.Get<IAppVersion>().GetVersion() + ")";
        }
    }
}