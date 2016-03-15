using System;
using System.Threading.Tasks;
using System.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using GalaSoft.MvvmLight.Views;

namespace YoutubeDownloader.Services
{
    public class DialogService : IDialogService
    {
        public Task ShowError( string message, string title, string buttonText, Action afterHideCallback )
        {
            return ShowMessage( message, title, buttonText, afterHideCallback );
        }

        public Task ShowError( Exception error, string title, string buttonText, Action afterHideCallback )
        {
            return ShowError( error.ToString(), title, buttonText, afterHideCallback );
        }

        public Task ShowMessage( string message, string title )
        {
            return ShowMessage( message, title, null, null );
        }

        public Task ShowMessage( string message, string title, string buttonText, Action afterHideCallback )
        {
            var callback = afterHideCallback != null ? x => afterHideCallback() : (Action<bool>)null;
            return ShowMessage( message, title, buttonText, null, callback );
        }

        public Task<bool> ShowMessage( string message, string title, string buttonConfirmText, string buttonCancelText,
            Action<bool> afterHideCallback )
        {
            var result = ModernDialog.ShowMessage( message, title,
                string.IsNullOrWhiteSpace( buttonCancelText ) ? MessageBoxButton.OKCancel : MessageBoxButton.OK );
            if ( afterHideCallback != null )
            {
                afterHideCallback( result == MessageBoxResult.OK );
            }
            return Task.FromResult( true );
        }

        public Task ShowMessageBox( string message, string title )
        {
            return ShowMessage( message, title );
        }
    }
}