using EmissorNFe.View;
using GalaSoft.MvvmLight.Views;
using NFe.WPF.ViewModel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NFe.WPF.View
{
    public class MessageService : IDialogService
    {
        public MessageService()
        {
        }

        public Task ShowError(string message, string title, string buttonText, Action afterHideCallback)
        {
            return Application.Current.Dispatcher.InvokeAsync(() => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information)).Task;
        }

        public Task ShowError(Exception error, string title, string buttonText, Action afterHideCallback)
        {
            return ShowError(error.InnerException.Message, title, buttonText, afterHideCallback);
        }

        public Task ShowMessage(string message, string title)
        {
            var app = Application.Current;
            var _mainWindow = app.MainWindow;

            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(_mainWindow, message,
                       title, MessageBoxButton.OK, MessageBoxImage.Information);
            }).Task;
        }

        public Task ShowMessage(string message, string title, string buttonText, Action afterHideCallback)
        {
            return ShowMessage(message, title);
        }

        public Task<bool> ShowMessage(string message, string title, string buttonConfirmText, string buttonCancelText, Action<bool> afterHideCallback)
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var app = Application.Current;
                var mainWindow = app.MainWindow;

                var result = MessageBox.Show(mainWindow, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

                return result == MessageBoxResult.Yes;
            }).Task;
        }

        public Task ShowMessageBox(string message, string title)
        {
            return ShowMessage(message, title);
        }
    }
}
