using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace Wpf.Backup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        System.Windows.Forms.NotifyIcon _trayIcon = new System.Windows.Forms.NotifyIcon();

        public App()
        {
            var iconPath = System.IO.Path.Combine(Environment.CurrentDirectory, "backup.ico");
            _trayIcon.Icon = new Icon(iconPath);
            _trayIcon.Visible = true;
            _trayIcon.Click += TrayIconClick;
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
            _trayIcon.Dispose();
            Shutdown();
        }
    }
}
