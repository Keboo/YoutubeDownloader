
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
            InitializeComponent();
        }

        public FileDownloadViewModel ViewModel
        {
            get { return (FileDownloadViewModel) DataContext; }
        }
    }
}
