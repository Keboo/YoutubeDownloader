
using YoutubeDownloader.ViewModel;

namespace YoutubeDownloader.Controls
{
    /// <summary>
    /// Interaction logic for DownloadingControl.xaml
    /// </summary>
    public partial class DownloadingControl
    {
        public DownloadingControl()
        {
            DataContext = ViewModel = new FileDownloadViewModel();
            InitializeComponent();
        }

        public FileDownloadViewModel ViewModel { get; }
    }
}
