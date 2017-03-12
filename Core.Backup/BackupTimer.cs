using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Core.Backup.Parameters;
using log4net;

namespace Core.Backup
{
    public class BackupTimer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BackupTimer));
        private DateTime _lastStart = DateTime.Today.Subtract(TimeSpan.FromHours(24));
        private readonly Timer _timer = new Timer();
        private readonly Action<string> _showMessage;

        public BackupTimer(Action<string> showMessage)
        {
            Logger.Debug("Constructor");
            _showMessage = showMessage;
            InitTimer();
        }

        private void InitTimer()
        {
            Logger.Debug("InitTimer");
            _timer.Interval = TimeSpan.FromSeconds(10).TotalMilliseconds;
            _timer.AutoReset = true;
            _timer.Elapsed += TimerOnElapsed;
        }

        public void Start()
        {
            _timer.Start();
        }

        private void OnTimerElapsed()
        {
            var config = ConfigManager.GetConfig();
            if (config.StartHour > DateTime.Now.Hour)
            {
                Logger.Info("Too early to start backup");
                return;
            }
            if (DateTime.Now - _lastStart < TimeSpan.FromHours(12))
            {
                var minutes = (DateTime.Now - _lastStart).TotalMinutes;
                Logger.Info($"Backup was already ran {minutes:N1} minutes ago");
                return;
            }
            _lastStart = DateTime.Now;
            Logger.Info("Starting backup");
            var task = Task.Factory.StartNew(() =>
            {
                try
                {
                    var backupWorker = new BackupWorker(_showMessage);
                    backupWorker.DoBackup().Wait();
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                    return false;
                }
            });
            task.Wait(-1);
            Logger.Info("End of backup");
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                Logger.Debug("TimerOnElapsed");
                _timer.Stop();
                OnTimerElapsed();
            }
            finally
            {
                Logger.Debug("TimerOnElapsed, starting timer");
                _timer.Start();
            }
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Elapsed -= TimerOnElapsed;
                _timer.Dispose();
            }
        }
    }
}
