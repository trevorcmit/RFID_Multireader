using System;
// using System.Drawing;
using Acr.UserDialogs;


namespace CS108XamarinFormsDemo.Extensions {
    public static class IUserDialogsExtensions {
        public static IDisposable ErrorToast(this IUserDialogs dialogs, string title, string message, TimeSpan duration) {
            return dialogs.Toast(new ToastConfig(message) { Duration = duration });
        }
    }
}
