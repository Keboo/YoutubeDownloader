using System.Windows.Controls;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModel;

namespace YoutubeDownloader.Pages
{
    /// <summary>
    /// Interaction logic for Download.xaml
    /// </summary>
    public partial class Download : Page
    {
        public Download()
        {
            DataContext = new DownloadViewModel(new DialogService());
            InitializeComponent();
        }
    }
}
