using System.Windows;

namespace Messenger
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ViewModels.MainViewModel mainViewModel = new ViewModels.MainViewModel();
            View.MainWindow mainWindow = new View.MainWindow(mainViewModel);
            mainWindow.Show();
        }
    }
}