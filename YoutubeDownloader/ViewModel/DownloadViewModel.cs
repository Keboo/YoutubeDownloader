using AutoDI;
using FirstFloor.ModernUI.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Views;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using YoutubeDownloader.Controls;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeDownloader.ViewModel
{
    public class DownloadViewModel : ViewModelBase
    {
        private readonly IDialogService _DialogService;
        private readonly RelayCommand<string> _FindVideoCommand;
        private readonly RelayCommand<MuxedStreamInfo> _DownloadFileCommand;

        public DownloadViewModel([Dependency]IDialogService dialogService = null)
        {
            if (dialogService == null) throw new ArgumentNullException(nameof(dialogService));
            _DialogService = dialogService;
            _FindVideoCommand = new RelayCommand<string>(OnFindVideo, CanFindVideo);
            _DownloadFileCommand = new RelayCommand<MuxedStreamInfo>(OnDownloadFile, CanDownloadFile);
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

        public ObservableCollection<MuxedStreamInfo> Videos { get; } = new ObservableCollection<MuxedStreamInfo>();

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

        private MuxedStreamInfo _SelectedVideo;
        public MuxedStreamInfo SelectedVideo
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
                var id = YoutubeClient.ParseVideoId(url);
                var client = new YoutubeClient();

                //Ver video info
                var video = await client.GetVideoAsync(id);
                Title = video?.Title;

                // Get metadata for all streams in this video
                var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);

                // Select one of the streams, e.g. highest quality muxed stream
                foreach (MuxedStreamInfo streamInfo in streamInfoSet.Muxed
                    .OrderByDescending(x => x.VideoQuality)
                    .ThenByDescending(x => x.Resolution.Width * x.Resolution.Height))
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

        private bool CanDownloadFile(MuxedStreamInfo video)
        {
            return video != null;
        }

        private void OnDownloadFile(MuxedStreamInfo video)
        {
            string fileFilter =
                "Video|*" + video.Container.GetFileExtension();
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