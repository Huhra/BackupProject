using Core.Backup.Parameters;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Wpf.Backup.Models;
using Wpf.Backup.Properties;

namespace Wpf.Backup.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly Configuration _config;

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

        private string _remoteDirectory;
        public string RemoteDirectory
        {
            get { return _remoteDirectory; }
            set
            {
                _remoteDirectory = value;
                OnPropertyChanged(() => RemoteDirectory);
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

        private bool _deleteFiles;
        public bool DeleteFiles
        {
            get { return _deleteFiles; }
            set
            {
                _deleteFiles = value;
                OnPropertyChanged(() => DeleteFiles);
            }
        }

        private bool _showTestResults;
        public bool ShowTestResults
        {
            get { return _showTestResults; }
            set
            {
                _showTestResults = value;
                OnPropertyChanged(() => ShowTestResults);
            }
        }

        private bool _testInProgress;
        public bool TestInProgress
        {
            get { return _testInProgress; }
            set
            {
                _testInProgress = value;
                OnPropertyChanged(() => TestInProgress);
            }
        }

        public ObservableCollection<string> Languages { get; }

        private string _selectedLanguage;
        public string SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged(() => SelectedLanguage);
            }
        }


        public ObservableCollection<ConfigurationTest> ConfigurationTests { get; }
            = new ObservableCollection<ConfigurationTest>();
        

        public ICommand Save { get; }
        public ICommand Exit { get; }
        public ICommand TestConfiguration { get; }
        
        public MainWindowViewModel()
        {
            Languages = new ObservableCollection<string>
            {
                Resources.Language_English,
                Resources.Language_French,
            };
            _config = ConfigManager.GetConfig();
            StartHour = _config.StartHour;
            DeleteFiles = _config.DeleteFiles;
            RootDirectory = _config.RootDirectory;
            RemoteDirectory = _config.RemoteDirectory;
            if (_config.Culture == "en-US")
                SelectedLanguage = Languages[0];
            else
                SelectedLanguage = Languages[1];
            Save = new DelegateCommand(DoSave);
            Exit = new DelegateCommand(DoExit);
            TestConfiguration = new DelegateCommand(DoTestConfiguration);
        }

        private void SetConfigFields()
        {
            _config.RemoteDirectory = RemoteDirectory;
            _config.StartHour = StartHour;
            _config.DeleteFiles = DeleteFiles;
            _config.RootDirectory = RootDirectory;
            if (_selectedLanguage == Resources.Language_English)
                _config.Culture = "en-US";
            else
                _config.Culture = "fr-FR";
        }

        private void DoSave()
        {
            SetConfigFields();
            MessageBox.Show(ConfigManager.SetConfig(_config)
                ? Resources.Message_ConfigurationSaveSuccess
                : Resources.Message_ConfigurationSaveError);
        }

        private void DoExit()
        {
            var app = Application.Current;
            var backupApp = app as App;
            backupApp?.DisposeAndExit();
        }

        private void DoTestConfiguration()
        {
            TestInProgress = true;
            ConfigurationTests.Clear();
            SetConfigFields();
            System.Threading.Tasks.Task<List<ConfigurationTest>>.Factory.StartNew(() =>
            {
                var result = _config.TestConfiguration();
                var testModels = new List<ConfigurationTest>();
                testModels.Add(new ConfigurationTest(
                    result.Contains(Configuration.TestConfigResult.RootDirectoryVisible),
                    "Root directory seems to be visible by the program.",
                    "Root directory is not visible by the program or does not exists."
                    ));
                testModels.Add(new ConfigurationTest(
                    result.Contains(Configuration.TestConfigResult.RemoteDirectoryVisible),
                    "Remote directory seems to be visible by the program.",
                    "Remote directory is not visible by the program or does not exists."
                    ));
                testModels.Add(new ConfigurationTest(
                    result.Contains(Configuration.TestConfigResult.WriteRights),
                    "The program have write permission in remote directory.",
                    "The program does not have write permission in remote directory."
                    ));
                testModels.Add(new ConfigurationTest(
                    result.Contains(Configuration.TestConfigResult.DeleteRights),
                    "The program have delete permission in remote directory.",
                    "The program does not have delete permission in remote directory."
                    ));
                if (result.Contains(Configuration.TestConfigResult.UnknownError))
                    testModels.Add(new ConfigurationTest(
                        "An unknown error occured, please check the logs."
                        ));
                return testModels;
            })
            .ContinueWith(task =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ConfigurationTests.AddRange(task.Result);
                    ShowTestResults = true;
                    TestInProgress = false;
                });
            });
        }
    }
}
