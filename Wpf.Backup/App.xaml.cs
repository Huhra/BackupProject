using System;
using System.Drawing;
using System.Windows;
using Core.Backup;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using System.Globalization;
using Core.Backup.Parameters;
using log4net;

namespace Wpf.Backup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(App));
        private readonly NotifyIcon _trayIcon = new NotifyIcon();
        private readonly BackupTimer _backup = new BackupTimer();

        public App()
        {
            try
            {
                var config = ConfigManager.GetConfig();
                var culture = new CultureInfo(config.Culture);
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.ToString());
            }
            var test = Wpf.Backup.Properties.Resources.DeleteRemoteFiles;
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
