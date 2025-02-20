using FirstFloor.ModernUI.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Views;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using YoutubeDownloader.Controls;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.ViewModel
{
    public class DownloadViewModel : ViewModelBase
    {
        private readonly IDialogService _DialogService;
        private readonly RelayCommand<string> _FindVideoCommand;
        private readonly RelayCommand<IStreamInfo> _DownloadFileCommand;

        public DownloadViewModel(IDialogService dialogService)
        {
            if (dialogService == null) throw new ArgumentNullException(nameof(dialogService));
            _DialogService = dialogService;
            _FindVideoCommand = new RelayCommand<string>(OnFindVideo, CanFindVideo);
            _DownloadFileCommand = new RelayCommand<IStreamInfo>(OnDownloadFile, CanDownloadFile);
            try
            {
                if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();
                    Uri uri;
                    if (Uri.TryCreate(text, UriKind.Absolute, out uri) &&
                        uri.Host.ToLowerInvariant().Contains("youtube"))
                    {
                        _YoutubeUrl = uri.ToString();
                    }
                }
            }
            catch (Exception)
            { }
        }

        public ObservableCollection<IStreamInfo> Videos { get; } = new ObservableCollection<IStreamInfo>();

        public ICommand FindVideoCommand => _FindVideoCommand;

        public ICommand DownloadFileCommand => _DownloadFileCommand;

        private string _YoutubeUrl;
        public string YoutubeUrl
        {
            get { return _YoutubeUrl; }
            set
            {
                if (Set(ref _YoutubeUrl, value))
                {
                    _FindVideoCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { Set(ref _Title, value); }
        }

        private IStreamInfo _SelectedVideo;
        public IStreamInfo SelectedVideo
        {
            get { return _SelectedVideo; }
            set
            {
                if (Set(ref _SelectedVideo, value))
                {
                    _DownloadFileCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _AudioOnly;
        public bool AudioOnly
        {
            get { return _AudioOnly; }
            set { Set(ref _AudioOnly, value); }
        }

        private bool _IsDownloading;
        public bool IsDownloading
        {
            get { return _IsDownloading; }
            set { Set(ref _IsDownloading, value); }
        }

        private async void OnFindVideo(string url)
        {
            try
            {
                Videos.Clear();
                YoutubeClient client = new YoutubeClient();
                var id = VideoId.Parse(url);
                //Ver video info
                var video = await client.Videos.GetAsync(id);
                Title = video?.Title;

                // Get metadata for all streams in this video
                var streamInfoSet = await client.Videos.Streams.GetManifestAsync(id);

                // Select one of the streams, e.g. highest quality muxed stream
                foreach (IStreamInfo streamInfo in streamInfoSet.Streams)
                {
                    Videos.Add(streamInfo);
                }
            }
            catch (Exception ex)
            {
                await _DialogService.ShowError(ex, "Cannot Locate Video", "OK", null);
            }
        }

        private bool CanFindVideo(string url)
        {
            return !string.IsNullOrWhiteSpace(url);
        }

        private bool CanDownloadFile(IStreamInfo video)
        {
            return video != null;
        }

        private void OnDownloadFile(IStreamInfo video)
        {
            string fileFilter =
                "Video|*" + video.Container.Name;
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save File",
                AddExtension = true,
                OverwritePrompt = true,
                RestoreDirectory = true,
                Filter = fileFilter + "|All Files|*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var downloadControl = new DownloadingControl();
                var downloadingVM = downloadControl.ViewModel;
                var dlg = new ModernDialog
                {
                    Title = "Downloading File",
                    Content = downloadControl
                };
                dlg.OkButton.Content = "_done";
                dlg.OkButton.SetBinding(UIElement.IsEnabledProperty,
                    new Binding("DownloadFinished")
                    {
                        Source = downloadingVM
                    });
                dlg.Buttons = new[] { dlg.OkButton, dlg.CancelButton };

                Task.Run(async () =>
                {
                    try
                    {
                        await downloadingVM.DownloadFile(video, saveFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        await _DialogService.ShowError(ex, "Error downloading", "OK", () => { });
                    }
                });

                if (dlg.ShowDialog() != true)
                {
                    downloadingVM.CancelDownload();
                }
            }
        }
    }
}