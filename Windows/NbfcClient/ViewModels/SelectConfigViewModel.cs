using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NbfcClient.Messages;
using NbfcClient.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace NbfcClient.ViewModels
{
    public class SelectConfigViewModel : ViewModelBase
    {
        #region Constants

        private const string ConfigsDirectoryName = "Configs";
        private const string ConfigEditorExecutableName = "ConfigEditor.exe";
        private const string SelectConfigArgumentPrefix = "-s:";

        #endregion

        #region Private Fields

        private readonly string ConfigEditorPath;
        private IFanControlClient client;
        private FileSystemWatcher configWatcher;

        private string selectedConfig;
        private bool dialogResult;
        private bool isBusy;
        private ObservableCollection<string> availableConfigs;

        private RelayCommand editConfigCommand;
        private RelayCommand applyConfigCommand;
        private RelayCommand cancelCommand;

        #endregion

        #region Constructors

        public SelectConfigViewModel(IFanControlClient client)
        {
            this.client = client;
            string path = Assembly.GetExecutingAssembly()?.Location;

            if (path != null)
            {
                path = Path.GetDirectoryName(path);
                ConfigEditorPath = Path.Combine(path, ConfigEditorExecutableName);

                string configsDirPath = Path.Combine(path, ConfigsDirectoryName);

                if (Directory.Exists(configsDirPath))
                {
                    configWatcher = new FileSystemWatcher(configsDirPath);
                    configWatcher.Created += ConfigWatcher_Changed;
                    configWatcher.Deleted += ConfigWatcher_Changed;
                    configWatcher.Renamed += ConfigWatcher_Changed;
                    configWatcher.EnableRaisingEvents = true;
                }
            }
        }

        #endregion

        #region Properties

        public bool DialogResult
        {
            get { return this.dialogResult; }
            set { this.Set(ref this.dialogResult, value); }
        }

        public ObservableCollection<string> AvailableConfigs
        {
            get
            {
                if (this.availableConfigs == null)
                {
                    this.availableConfigs = new ObservableCollection<string>(client.GetConfigNames());
                }

                return this.availableConfigs;
            }
            private set { this.Set(ref this.availableConfigs, value); }
        }

        public bool IsBusy
        {
            get { return this.isBusy; }
            set
            {
                if (this.Set(ref this.isBusy, value))
                {
                    applyConfigCommand.RaiseCanExecuteChanged();
                    editConfigCommand.RaiseCanExecuteChanged();
                    cancelCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string SelectedConfig
        {
            get
            {
                if (this.selectedConfig == null)
                {
                    this.selectedConfig = this.client.FanControlInfo?.SelectedConfig;
                }

                return this.selectedConfig;
            }
            set { this.Set(ref this.selectedConfig, value); }
        }

        #endregion

        #region Commands

        public RelayCommand EditConfigCommand
        {
            get
            {
                if (this.editConfigCommand == null)
                {
                    this.editConfigCommand = new RelayCommand(EditConfig, CanExecuteEditConfig);
                }

                return this.editConfigCommand;
            }
        }

        public RelayCommand ApplyConfigCommand
        {
            get
            {
                if (this.applyConfigCommand == null)
                {
                    this.applyConfigCommand = new RelayCommand(ApplyConfig, CanExecuteApplyConfig);
                }

                return this.applyConfigCommand;
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                {
                    this.cancelCommand = new RelayCommand(Cancel, CanExecuteCancel);
                }

                return this.cancelCommand;
            }
        }

        #endregion

        #region Private Methods

        // Create parameterless methods for use in RelayCommands, because RelayCommands do not
        // support closures (see https://mvvmlight.codeplex.com/workitem/7721)
        private void EditConfig()
        {
            string args = SelectConfigArgumentPrefix + "\"" + selectedConfig + "\"";
            Process.Start(ConfigEditorPath, args);
        }

        private bool CanExecuteEditConfig()
        {
            return !IsBusy && File.Exists(ConfigEditorPath);
        }

        private async void ApplyConfig()
        {
            IsBusy = true;

            try
            {
                await Task.Factory.StartNew(() => client.SetConfig(this.selectedConfig));

                var refreshMsg = new ReloadFanControlInfoMessage(true);
                Messenger.Default.Send(refreshMsg);
                DialogResult = true;
            }
            finally
            {
                IsBusy = false;
            }

            var closeMsg = new CloseSelectConfigDialogMessage(dialogResult, selectedConfig);
            Messenger.Default.Send(closeMsg);
        }

        private bool CanExecuteApplyConfig()
        {
            return !IsBusy && !string.IsNullOrEmpty(this.selectedConfig);
        }

        private void Cancel()
        {
            DialogResult = false;
            var msg = new CloseSelectConfigDialogMessage(dialogResult, selectedConfig);
            Messenger.Default.Send(msg);
        }

        private bool CanExecuteCancel()
        {
            return !IsBusy;
        }

        #endregion

        #region EventHandlers

        private void ConfigWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            AvailableConfigs = new ObservableCollection<string>(client.GetConfigNames());
        }

        #endregion
    }
}
