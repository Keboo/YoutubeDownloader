using GalaSoft.MvvmLight;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

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

        public async Task DownloadFile(MuxedStreamInfo video, string destinationFile)
        {
            if (video is null) throw new ArgumentNullException(nameof(video));
            if (destinationFile is null) throw new ArgumentNullException(nameof(destinationFile));

            var client = new YoutubeClient();
            string ext = video.Container.GetFileExtension();
            _TokenSource = new CancellationTokenSource();

            await client.DownloadMediaStreamAsync(video, Path.ChangeExtension(destinationFile, ext), new Progress<double>(x => DownloadPercentage = x * 100), _TokenSource.Token);
            DownloadFinished = true;
        }
    }
}