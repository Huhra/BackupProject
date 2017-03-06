using System;
using System.Drawing;
using System.Windows;
using Core.Backup;
using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace Wpf.Backup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly NotifyIcon _trayIcon = new NotifyIcon();
        private readonly BackupTimer _backup = new BackupTimer();

        public App()
        {
            var iconPath = System.IO.Path.Combine(Environment.CurrentDirectory, "backup.ico");
            _trayIcon.Icon = new Icon(iconPath);
            _trayIcon.Visible = true;
            _trayIcon.Click += TrayIconClick;
            _backup.Start();
            //var title = Backup.Properties.Resources.StartNotificationTitle;
            //var content = Backup.Properties.Resources.StartNotificationContent;
            //_trayIcon.ShowBalloonTip(5000, title, content, System.Windows.Forms.ToolTipIcon.Info);
        }

        private void TrayIconClick(object sender, EventArgs e)
        {
            ShowWindow();
        }

        private void ShowWindow()
        {
            MainWindow.Visibility = Visibility.Visible;
            MainWindow.WindowState = WindowState.Normal;
            (MainWindow as MainWindow)?.Reload();
        }

        public void DisposeAndExit()
        {
            _backup.Dispose();
            _trayIcon.Dispose();
            Shutdown();
        }
    }
}
