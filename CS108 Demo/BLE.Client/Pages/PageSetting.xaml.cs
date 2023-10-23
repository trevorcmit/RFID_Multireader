using Xamarin.Forms;


namespace BLE.Client.Pages
{
	public partial class PageSetting : TabbedPage
    {
        public PageSetting()
        {
            InitializeComponent();

            switch (BleMvxApplication._reader1.rfid.GetModelName())
            {
                case "CS108":
                    this.Children.RemoveAt(2); break;
                default:
                    this.Children.RemoveAt(3); break;
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called
        }

    }
}
