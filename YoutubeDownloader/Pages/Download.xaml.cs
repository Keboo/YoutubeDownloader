using System.Windows.Controls;
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
            DataContext = new DownloadViewModel();
            InitializeComponent();
        }
    }
}
