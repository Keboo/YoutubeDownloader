using GalaSoft.MvvmLight;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.ViewModel
{
    public class FileDownloadViewModel : ViewModelBase
    {
        private CancellationTokenSource _TokenSource;

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { Set(ref _Title, value); }
        }

        private double _DownloadPercentage;
        public double DownloadPercentage
        {
            get { return _DownloadPercentage; }
            set { Set(ref _DownloadPercentage, value); }
        }

        private bool _DownloadFinished;
        public bool DownloadFinished
        {
            get { return _DownloadFinished; }
            set { Set(ref _DownloadFinished, value); }
        }

        public void CancelDownload()
        {
            _TokenSource?.Cancel();
        }

        public async Task DownloadFile(IStreamInfo streamInfo, string destinationFile)
        {
            if (streamInfo is null) throw new ArgumentNullException(nameof(streamInfo));
            if (destinationFile is null) throw new ArgumentNullException(nameof(destinationFile));

            var client = new YoutubeClient();
            string ext = streamInfo.Container.Name;
            _TokenSource = new CancellationTokenSource();

            string filePath = Path.ChangeExtension(destinationFile, ext);
            Progress<double> progress = new Progress<double>(x => DownloadPercentage = x * 100);
            await client.Videos.Streams.DownloadAsync(streamInfo, filePath, progress, _TokenSource.Token);
            DownloadFinished = true;
        }
    }
}