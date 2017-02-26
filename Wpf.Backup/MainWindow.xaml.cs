using System.Windows;
using Wpf.Backup.ViewModels;

namespace Wpf.Backup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
            Reload();
            //Visibility = Visibility.Hidden;
        }

        public void Reload()
        {
            DataContext = new MainWindowViewModel();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
    }
}
