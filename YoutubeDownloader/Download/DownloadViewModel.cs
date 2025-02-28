using System.IO;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Download;

public partial class DownloadViewModel : ObservableObject
{
    private CancellationTokenSource? _tokenSource;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private double _downloadPercentage;

    [ObservableProperty]
    private bool _downloadFinished;

    [RelayCommand]
    private void CancelDownload()
    {
        _tokenSource?.Cancel();
    }

    public async Task StartDownloadAsync(IStreamInfo streamInfo, string destinationFile)
    {
        ArgumentNullException.ThrowIfNull(streamInfo);
        ArgumentNullException.ThrowIfNull(destinationFile);

        var client = new YoutubeClient();
        string ext = streamInfo.Container.Name;
        _tokenSource = new CancellationTokenSource();

        string filePath = Path.ChangeExtension(destinationFile, ext);
        Progress<double> progress = new(x => DownloadPercentage = x * 100);
        try
        {
            await client.Videos.Streams.DownloadAsync(streamInfo, filePath, progress, _tokenSource.Token);
        }
        catch(OperationCanceledException)
        {
            //TODO: Anything?
        }
        DownloadFinished = true;
    }
}