using System.Windows;

namespace Messenger
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Client.ViewModels.MainViewModel mainViewModel = new Client.ViewModels.MainViewModel();
            View.MainWindow mainWindow = new View.MainWindow(mainViewModel);
            mainWindow.Show();
        }
    }
}