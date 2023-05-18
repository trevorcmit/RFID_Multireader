using Acr.UserDialogs;
using Plugin.BLE.Abstractions.Contracts;


namespace BLE.Client.ViewModels
{
    public class ViewModelAbout : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;
        public ViewModelAbout (IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
        }

    }
}
