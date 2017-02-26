using Core.Backup.Parameters;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;
using System;

namespace Wpf.Backup.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private Configuration _config;

        private string _rootDirectory;
        public string RootDirectory
        {
            get { return _rootDirectory; }
            set
            {
                _rootDirectory = value;
                OnPropertyChanged(() => RootDirectory);
            }
        }

        private int _startHour;
        public int StartHour
        {
            get { return _startHour; }
            set
            {
                _startHour = value;
                OnPropertyChanged(() => StartHour);
            }
        }

        public ICommand Exit { get; set; }

        public MainWindowViewModel()
        {
            _config = ConfigManager.GetConfig();
            _rootDirectory = _config.RootDirectory;
            StartHour = _config.StartHour;
            Exit = new DelegateCommand(() => DoExit());
        }

        private void DoExit()
        {
            var app = System.Windows.Application.Current;
            var backupApp = app as Wpf.Backup.App;
            backupApp?.DisposeAndExit();
        }
    }
}
