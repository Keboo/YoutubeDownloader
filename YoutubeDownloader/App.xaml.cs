
using System.Windows;
using AutoDI;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using YoutubeDownloader.Services;

namespace YoutubeDownloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ServiceLocator.SetLocatorProvider( () => SimpleIoc.Default );

            SimpleIoc.Default.Register<IDialogService, DialogService>();

            DependencyResolver.Set(new SimpleIoCResolver());

            base.OnStartup(e);
        }
    }

    public class SimpleIoCResolver : IDependencyResolver
    {
        public T Resolve<T>(params object[] parameters)
        {
            return ServiceLocator.Current.GetInstance<T>();
        }
    }
}
