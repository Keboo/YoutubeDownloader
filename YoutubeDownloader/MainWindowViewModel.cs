using System.Collections.ObjectModel;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MaterialDesignThemes.Wpf;

using Microsoft.Win32;

using YoutubeDownloader.About;

using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader;

public partial class MainWindowViewModel : ObservableObject
{
    //private readonly IDialogService _dialogService;
    private readonly Func<AboutViewModel> _aboutViewModelFactory;


    public MainWindowViewModel(Func<AboutViewModel> aboutViewModelFactory)
    {
        /*
        if (dialogService == null) throw new ArgumentNullException(nameof(dialogService));
        _dialogService = dialogService;
        */
        _aboutViewModelFactory = aboutViewModelFactory;

        try
        {
            if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();
                if (Uri.TryCreate(text, UriKind.Absolute, out Uri? uri) &&
                    uri.Host.ToLowerInvariant().Contains("youtube"))
                {
                    YoutubeUrl = uri.ToString();
                }
            }
        }
        catch (Exception)
        { }

    }

    [RelayCommand]
    private void OnShowAbout()
    {
        DialogHost.Show(_aboutViewModelFactory());
    }

    public ObservableCollection<IStreamInfo> Videos { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FindVideoCommand))]
    private string? _youtubeUrl;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadFileCommand))]
    private IStreamInfo? _selectedVideo;

    [ObservableProperty]
    private bool _AudioOnly;

    [RelayCommand(CanExecute = nameof(CanFindVideo))]
    private async Task OnFindVideo(string? url)
    {
        try
        {
            Videos.Clear();
            YoutubeClient client = new();
            var id = VideoId.Parse(url!);
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
            //await _DialogService.ShowError(ex, "Cannot Locate Video", "OK", null);
        }
    }

    private static bool CanFindVideo(string? url) => !string.IsNullOrWhiteSpace(url);

    [RelayCommand(CanExecute = nameof(CanDownloadFile))]
    private void OnDownloadFile(IStreamInfo? video)
    {
        string fileFilter =
            "Video|*" + video!.Container.Name;
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
            //var downloadControl = new DownloadingControl();
            //var downloadingVM = downloadControl.ViewModel;
            //var dlg = new ModernDialog
            //{
            //    Title = "Downloading File",
            //    Content = downloadControl
            //};
            //dlg.OkButton.Content = "_done";
            //dlg.OkButton.SetBinding(UIElement.IsEnabledProperty,
            //    new Binding("DownloadFinished")
            //    {
            //        Source = downloadingVM
            //    });
            //dlg.Buttons = new[] { dlg.OkButton, dlg.CancelButton };

            //Task.Run(async () =>
            //{
            //    try
            //    {
            //        await downloadingVM.DownloadFile(video, saveFileDialog.FileName);
            //    }
            //    catch (Exception ex)
            //    {
            //        await _DialogService.ShowError(ex, "Error downloading", "OK", () => { });
            //    }
            //});

            //if (dlg.ShowDialog() != true)
            //{
            //    downloadingVM.CancelDownload();
            //}
        }
    }
    private static bool CanDownloadFile(IStreamInfo? video) => video != null;
}
