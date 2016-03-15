using GalaSoft.MvvmLight;
using System;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExtractor;

namespace YoutubeDownloader.ViewModel
{
    public class FileDownloadViewModel : ViewModelBase
    {
        private CancellationTokenSource _TokenSource;

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { Set( ref _Title, value ); }
        }

        private double _DownloadPercentage;
        public double DownloadPercentage
        {
            get { return _DownloadPercentage; }
            set { Set( ref _DownloadPercentage, value ); }
        }

        private bool _DownloadFinished;
        public bool DownloadFinished
        {
            get { return _DownloadFinished; }
            set { Set(ref _DownloadFinished, value); }
        }

        public void CancelDownload()
        {
            if ( _TokenSource != null )
            {
                _TokenSource.Cancel();
            }
        }

        public void StartVideoDownload( VideoInfo video, string destinationFile )
        {
            if ( video == null ) throw new ArgumentNullException( "video" );
            if ( destinationFile == null ) throw new ArgumentNullException( "destinationFile" );

            var downloader = new VideoDownloader(video, destinationFile);
            downloader.DownloadProgressChanged += OnDownloadProgressChanged;
            StartDownload( downloader );
        }

        public void StartAudioDownload( VideoInfo video, string destinationFile )
        {
            if ( video == null ) throw new ArgumentNullException( "video" );
            if ( destinationFile == null ) throw new ArgumentNullException( "destinationFile" );

            var downloader = new AudioDownloader( video, destinationFile );
            downloader.DownloadProgressChanged += OnDownloadProgressChanged;
            StartDownload( downloader );
        }

        private void StartDownload( Downloader downloader )
        {
            if ( downloader.Video.RequiresDecryption )
            {
                DownloadUrlResolver.DecryptDownloadUrl( downloader.Video );
            }
            Title = downloader.Video.Title;

            _TokenSource = new CancellationTokenSource();
            downloader.DownloadFinished += (sender, e) => DownloadFinished = true;
            Task.Run( () => downloader.Execute(), _TokenSource.Token );
        }

        private void OnDownloadProgressChanged( object sender, ProgressEventArgs e )
        {
            if ( _TokenSource != null && _TokenSource.IsCancellationRequested )
            {
                e.Cancel = true;
            }
            DownloadPercentage = e.ProgressPercentage;
        }
    }
}