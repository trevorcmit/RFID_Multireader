using CS108XamarinFormsDemo.ViewModels;
using Xamarin.Forms;

namespace CS108XamarinFormsDemo.Views
{
    public class BasePage : ContentPage
    {
        protected override void OnAppearing()
        {
            base.OnAppearing();

            var viewModel = BindingContext as BaseViewModel;
            viewModel?.Resume();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            var viewModel = BindingContext as BaseViewModel;
            viewModel?.Suspend();
        }
    }

}
