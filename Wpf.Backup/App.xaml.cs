using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Drawing;
using System.Windows;
using Core.Backup;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Core.Backup.Parameters;
using log4net;
using Application = System.Windows.Application;

namespace Wpf.Backup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(App));
        private readonly NotifyIcon _trayIcon = new NotifyIcon();
        private readonly BackupTimer _backup;

        public App()
        {
            Reset();
            Logger.Info("Starting backup application");
            SetUiCulture();
            InitTrayIcon();
            _backup = new BackupTimer(ShowMessage);
            _backup.Start();
            //var title = Backup.Properties.Resources.StartNotificationTitle;
            //var content = Backup.Properties.Resources.StartNotificationContent;
            //_trayIcon.ShowBalloonTip(5000, title, content, System.Windows.Forms.ToolTipIcon.Info);
        }

        private void ShowMessage(string message)
        {
            _trayIcon.ShowBalloonTip(2000, "Backup", message, ToolTipIcon.Info);
        }

        private void Reset()
        {
            var config = ConfigManager.GetConfig();
            config.LastRootDirectory = string.Empty;
            try
            {
                var directories =
                        System.IO.Directory.GetDirectories(config.RemoteDirectory, "*", SearchOption.AllDirectories)
                            .OrderByDescending(d => d.Length)
                            .ToList();
                var files =
                    System.IO.Directory.GetFiles(config.RemoteDirectory, "*.*", SearchOption.AllDirectories)
                        .OrderByDescending(f => f.Length)
                        .ToList();
                foreach (var file in files.AsParallel())
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch (Exception e)
                    {
                    }
                }
                foreach (var directory in directories.AsParallel())
                {
                    try
                    {
                        System.IO.Directory.Delete(directory, true);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            catch (Exception)
            {
                //ignore
            }
            ConfigManager.SetConfig(config);
        }

        private void InitTrayIcon()
        {
            var iconPath = System.IO.Path.Combine(Environment.CurrentDirectory, "backup.ico");
            _trayIcon.Icon = new Icon(iconPath);
            _trayIcon.Visible = true;
            _trayIcon.Click += TrayIconClick;
        }

        private static void SetUiCulture()
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
            _trayIcon.Click -= TrayIconClick;
            _trayIcon.Dispose();
            Shutdown();
        }
    }
}
