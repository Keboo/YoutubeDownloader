using System.Windows.Controls;
using System.Windows.Data;
using FirstFloor.ModernUI.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Views;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using YoutubeDownloader.Controls;
using YoutubeExtractor;

namespace YoutubeDownloader.ViewModel
{
    public class DownloadViewModel : ViewModelBase
    {
        private readonly IDialogService _DialogService;
        private readonly ObservableCollection<VideoInfo> _Videos = new ObservableCollection<VideoInfo>();
        private readonly RelayCommand<string> _FindVideoCommand;
        private readonly RelayCommand<VideoInfo> _DownloadFileCommand;

        public DownloadViewModel( IDialogService dialogService )
        {
            if ( dialogService == null ) throw new ArgumentNullException( "dialogService" );
            _DialogService = dialogService;
            _FindVideoCommand = new RelayCommand<string>( OnFindVideo, CanFindVideo );
            _DownloadFileCommand = new RelayCommand<VideoInfo>( OnDownloadFile, CanDownloadFile );
            try
            {
                if ( Clipboard.ContainsText() )
                {
                    var text = Clipboard.GetText();
                    Uri uri;
                    if ( Uri.TryCreate( text, UriKind.Absolute, out uri ) &&
                        uri.Host.ToLowerInvariant().Contains( "youtube" ) )
                    {
                        _YoutubeUrl = uri.ToString();
                    }
                }
            }
            catch ( Exception )
            { }
        }

        public ObservableCollection<VideoInfo> Videos
        {
            get { return _Videos; }
        }

        public ICommand FindVideoCommand
        {
            get { return _FindVideoCommand; }
        }

        public ICommand DownloadFileCommand
        {
            get { return _DownloadFileCommand; }
        }

        private string _YoutubeUrl;
        public string YoutubeUrl
        {
            get { return _YoutubeUrl; }
            set
            {
                if ( Set( ref _YoutubeUrl, value ) )
                {
                    _FindVideoCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { Set( ref _Title, value ); }
        }

        private VideoInfo _SelectedVideo;
        public VideoInfo SelectedVideo
        {
            get { return _SelectedVideo; }
            set
            {
                if ( Set( ref _SelectedVideo, value ) )
                {
                    _DownloadFileCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _AudioOnly;
        public bool AudioOnly
        {
            get { return _AudioOnly; }
            set { Set( ref _AudioOnly, value ); }
        }

        private bool _IsDownloading;
        public bool IsDownloading
        {
            get { return _IsDownloading; }
            set { Set( ref _IsDownloading, value ); }
        }

        private void OnFindVideo( string url )
        {
            try
            {
                _Videos.Clear();
                foreach ( var video in DownloadUrlResolver.GetDownloadUrls( url )
                    .OrderByDescending( x => x.Resolution )
                    .ThenByDescending( x => x.VideoType ) )
                {
                    _Videos.Add( video );
                }
                if ( _Videos.Count > 0 )
                {
                    Title = _Videos[0].Title;
                }
            }
            catch ( Exception ex )
            {
                _DialogService.ShowError( ex, "Cannot Locate Video", "OK", null );
            }
        }

        private bool CanFindVideo( string url )
        {
            return !string.IsNullOrWhiteSpace( url );
        }

        private bool CanDownloadFile( VideoInfo video )
        {
            return video != null;
        }

        private void OnDownloadFile( VideoInfo video )
        {
            string fileFilter = AudioOnly
                ? "Audio|*" + video.AudioExtension
                : "Video|*" + video.VideoExtension;
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save File",
                AddExtension = true,
                OverwritePrompt = true,
                RestoreDirectory = true,
                Filter = fileFilter + "|All Files|*"
            };

            if ( saveFileDialog.ShowDialog() == true )
            {
                var downloadControl = new DownloadingControl();
                var downloadingVM = downloadControl.ViewModel;
                var dlg = new ModernDialog
                {
                    Title = "Downloading File",
                    Content = downloadControl
                };
                dlg.OkButton.Content = "_done";
                dlg.OkButton.SetBinding( UIElement.IsEnabledProperty,
                    new Binding( "DownloadFinished" )
                    {
                        Source = downloadingVM
                    } );
                dlg.Buttons = new[] { dlg.OkButton, dlg.CancelButton };

                if ( AudioOnly )
                {
                    downloadingVM.StartAudioDownload( video, saveFileDialog.FileName );
                }
                else
                {
                    downloadingVM.StartVideoDownload( video, saveFileDialog.FileName );
                }
                if ( dlg.ShowDialog() != true )
                {
                    downloadingVM.CancelDownload();
                }
            }
        }
    }
}