using Messenger.ViewModels;
using System.Windows;

namespace Messenger.View
{

    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();
            DataContext = mainViewModel;
        }
    }
}