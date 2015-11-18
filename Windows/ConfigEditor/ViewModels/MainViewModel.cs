using ConfigEditor.Commands;
using StagWare.BiosInfo;
using StagWare.FanControl.Configurations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConfigEditor.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Constants

        private const string ConfigsDirectoryName = "Configs";
        private const string NotebookModelValueName = "SystemProductName";

        #endregion

        #region Private Fields

        private FanControlConfigManager configManager;

        #region Property Backing Fields

        private ObservableCollection<string> configNames;
        private string selectedConfigName;
        private string notebookModel;
        private string author;
        private int ecPollInterval;
        private int criticalTemperature;
        private bool readWriteWords;
        private ObservableCollection<FanConfigViewModel> fanConfigs;
        private ObservableCollection<RegisterWriteConfigViewModel> registerWriteConfigs;
        private string actualNotebookModel;

        #endregion

        #region Commands Backing Fields

        private RelayCommand selectConfigCommand;
        private RelayCommand createNewConfigCommand;
        private RelayCommand saveConfigCommand;
        private RelayCommand saveConfigAsCommand;
        private RelayCommand deleteConfigCommand;
        private RelayCommand importConfigCommand;

        #endregion

        #endregion

        #region Events

        public event EventHandler<DialogEventArgs<RequestConfigNameViewModel>> RequestingConfigName;
        public event EventHandler<DialogEventArgs<RequestConfigPathViewModel>> RequestingConfigPath;
        public event EventHandler<CommandExecutedEventArgs> SaveConfigCommandExecuted;
        public event EventHandler ImportConfigError;

        #endregion

        #region Properties

        #region Model Data

        public ObservableCollection<RegisterWriteConfigViewModel> RegisterWriteConfigs
        {
            get
            {
                return registerWriteConfigs;
            }

            set
            {
                if (registerWriteConfigs != value)
                {
                    registerWriteConfigs = value;
                    OnPropertyChanged("RegisterWriteConfigs");
                }
            }
        }

        public ObservableCollection<FanConfigViewModel> FanConfigs
        {
            get
            {
                return fanConfigs;
            }

            set
            {
                if (fanConfigs != value)
                {
                    fanConfigs = value;
                    OnPropertyChanged("FanConfigs");
                }
            }
        }

        public bool ReadWriteWords
        {
            get
            {
                return readWriteWords;
            }

            set
            {
                if (readWriteWords != value)
                {
                    readWriteWords = value;
                    OnPropertyChanged("ReadWriteWords");
                }
            }
        }

        public int CriticalTemperature
        {
            get
            {
                return criticalTemperature;
            }

            set
            {
                if (criticalTemperature != value)
                {
                    criticalTemperature = value;
                    OnPropertyChanged("CriticalTemperature");
                }
            }
        }

        public int EcPollInterval
        {
            get
            {
                return ecPollInterval;
            }

            set
            {
                if (ecPollInterval != value)
                {
                    ecPollInterval = value;
                    OnPropertyChanged("EcPollInterval");
                }
            }
        }

        public string NotebookModel
        {
            get
            {
                return notebookModel;
            }

            set
            {
                if (notebookModel != value)
                {
                    notebookModel = value;
                    OnPropertyChanged("NotebookModel");
                }
            }
        }

        public string Author
        {
            get { return author; }
            set
            {
                if (author != value)
                {
                    author = value;
                    OnPropertyChanged("Author");
                }
            }
        }

        public string SelectedConfigName
        {
            get
            {
                return selectedConfigName;
            }

            set
            {
                if (selectedConfigName != value)
                {
                    selectedConfigName = value;
                    OnPropertyChanged("SelectedConfigName");
                }
            }
        }

        public ObservableCollection<string> ConfigNames
        {
            get
            {
                return configNames;
            }

            set
            {
                if (configNames != value)
                {
                    configNames = value;
                    OnPropertyChanged("ConfigNames");
                }
            }
        }

        public string ActualNotebookModel
        {
            get
            {
                return actualNotebookModel;
            }
            set
            {
                if (actualNotebookModel != value)
                {
                    actualNotebookModel = value;
                    OnPropertyChanged("ActualNotebookModel");
                }
            }
        }


        #endregion

        #region Commands

        public RelayCommand SelectConfigCommand
        {
            get
            {
                if (this.selectConfigCommand == null)
                {
                    this.selectConfigCommand = new RelayCommand(o =>
                    {
                        string configName = o as string;

                        this.configManager.SelectConfig(configName);
                        UpdateViewModel();
                    });
                }

                return this.selectConfigCommand;
            }
        }

        public RelayCommand ImportConfigCommand
        {
            get
            {
                if (this.importConfigCommand == null)
                {
                    this.importConfigCommand = new RelayCommand(o =>
                    {
                        var vm = new RequestConfigPathViewModel();
                        var args = new DialogEventArgs<RequestConfigPathViewModel>(vm);

                        OnRequestingConfigPath(args);

                        if (args.Update && vm.IsPathValid)
                        {
                            FanControlConfig cfg = null;

                            if (TryLoadFanControlConfigV1(vm.ConfigFilePath, out cfg))
                            {
                                string cfgName = cfg.UniqueId;

                                if (IsConfigNameValid(cfgName) || TryRequestConfigName(ref cfgName))
                                {
                                    AddOrUpdateConfig(new FanControlConfigV2(cfg), cfgName);
                                    UpdateViewModel();
                                }
                            }
                            else
                            {
                                OnImportConfigError(EventArgs.Empty);
                            }
                        }
                    });
                }

                return this.importConfigCommand;
            }
        }

        public RelayCommand DeleteConfigCommand
        {
            get
            {
                if (this.deleteConfigCommand == null)
                {
                    this.deleteConfigCommand = new RelayCommand(o =>
                    {
                        if (this.configManager.SelectedConfig != null)
                        {
                            this.configManager.RemoveConfig(this.configManager.SelectedConfigName);
                        }

                        UpdateViewModel();
                    });
                }

                return this.deleteConfigCommand;
            }
        }

        public RelayCommand SaveConfigAsCommand
        {
            get
            {
                if (this.saveConfigAsCommand == null)
                {
                    this.saveConfigAsCommand = new RelayCommand(o =>
                    {
                        try
                        {
                            string cfgName = this.ActualNotebookModel;

                            if (IsConfigNameValid(cfgName) || TryRequestConfigName(ref cfgName))
                            {
                                AddOrUpdateConfig(ConvertViewModelToConfig(this), cfgName);
                                UpdateViewModel();

                                OnSaveConfigCommandExecuted(new CommandExecutedEventArgs()
                                {
                                    Success = true
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            OnSaveConfigCommandExecuted(new CommandExecutedEventArgs()
                            {
                                Success = false,
                                Exception = e
                            });
                        }
                    });
                }

                return this.saveConfigAsCommand;
            }
        }

        public RelayCommand SaveConfigCommand
        {
            get
            {
                if (this.saveConfigCommand == null)
                {
                    this.saveConfigCommand = new RelayCommand(o =>
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(this.SelectedConfigName))
                                {
                                    this.SaveConfigAsCommand.Execute(null);
                                }
                                else
                                {
                                    var cfg = ConvertViewModelToConfig(this);
                                    this.configManager.UpdateConfig(this.SelectedConfigName, cfg);

                                    OnSaveConfigCommandExecuted(new CommandExecutedEventArgs()
                                    {
                                        Success = true
                                    });
                                }
                            }
                            catch (Exception e)
                            {
                                OnSaveConfigCommandExecuted(new CommandExecutedEventArgs()
                                {
                                    Success = false,
                                    Exception = e
                                });
                            }
                        });
                }

                return this.saveConfigCommand;
            }
        }

        public RelayCommand CreateNewConfigCommand
        {
            get
            {
                if (this.createNewConfigCommand == null)
                {
                    this.createNewConfigCommand = new RelayCommand(o =>
                    {
                        this.configManager.SelectConfig(null);
                        UpdateViewModel();
                    });
                }

                return this.createNewConfigCommand;
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MainViewModel()
        {
            this.FanConfigs = new ObservableCollection<FanConfigViewModel>();
            this.RegisterWriteConfigs = new ObservableCollection<RegisterWriteConfigViewModel>();
            this.ActualNotebookModel = GetNotebookModel();

            InitializeConfigManager();
            UpdateViewModel();
        }

        #endregion

        #region Protected Methods

        protected void OnRequestingConfigName(DialogEventArgs<RequestConfigNameViewModel> e)
        {
            if (this.RequestingConfigName != null)
            {
                RequestingConfigName(this, e);
            }
        }

        protected void OnRequestingConfigPath(DialogEventArgs<RequestConfigPathViewModel> e)
        {
            if (this.RequestingConfigPath != null)
            {
                RequestingConfigPath(this, e);
            }
        }

        protected void OnSaveConfigCommandExecuted(CommandExecutedEventArgs e)
        {
            if (this.SaveConfigCommandExecuted != null)
            {
                SaveConfigCommandExecuted(this, e);
            }
        }

        protected void OnImportConfigError(EventArgs e)
        {
            if (this.ImportConfigError != null)
            {
                ImportConfigError(this, e);
            }
        }

        #endregion

        #region Private Methods

        private static string GetNotebookModel()
        {
            if (BiosInfo.ValueInfo.Any(x => x.ValueName.Equals(
                NotebookModelValueName, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    return BiosInfo.GetStringValue(NotebookModelValueName);
                }
                catch
                {
                }
            }

            return null;
        }

        private void InitializeConfigManager()
        {
            string path = Assembly.GetExecutingAssembly().Location;
            path = Path.GetDirectoryName(path);
            path = Path.Combine(path, ConfigsDirectoryName);
            this.configManager = new FanControlConfigManager(path);
            this.configManager.SelectConfig(this.ActualNotebookModel);
        }

        private bool TryLoadFanControlConfigV1(string configFilePath, out FanControlConfig config)
        {
            config = null;

            try
            {
                using (FileStream fs = new FileStream(configFilePath, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(FanControlConfig));
                    config = (FanControlConfig)serializer.Deserialize(fs);
                }
            }
            catch
            {
                return false;
            }

            return config != null;
        }

        private bool IsConfigNameValid(string configName)
        {
            return !(this.configManager.Contains(configName)
                    || this.configManager.ConfigFileExists(configName));
        }

        private void AddOrUpdateConfig(FanControlConfigV2 config, string configName)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (string.IsNullOrWhiteSpace(config.NotebookModel))
            {
                config.NotebookModel = this.ActualNotebookModel;
            }

            if (this.configManager.Contains(configName))
            {
                this.configManager.UpdateConfig(configName, config);
            }
            else
            {
                this.configManager.AddConfig(config, configName);
            }
        }

        private bool TryRequestConfigName(ref string configName)
        {
            var vm = new RequestConfigNameViewModel(this.configManager, configName);
            var args = new DialogEventArgs<RequestConfigNameViewModel>(vm);

            OnRequestingConfigName(args);

            if (args.Update && vm.IsConfigNameValid)
            {
                configName = vm.ConfigName;
                return true;
            }
            else
            {
                configName = null;
                return false;
            }
        }

        #region ViewModel to config conversion

        private static FanControlConfigV2 ConvertViewModelToConfig(MainViewModel viewModel)
        {
            var config = new FanControlConfigV2()
            {
                CriticalTemperature = viewModel.CriticalTemperature,
                EcPollInterval = viewModel.EcPollInterval,
                NotebookModel = viewModel.NotebookModel,
                Author = viewModel.Author,
                ReadWriteWords = viewModel.ReadWriteWords
            };

            if (viewModel.FanConfigs != null)
            {
                config.FanConfigurations = ConvertViewModelsToFanConfigs(viewModel.FanConfigs);
            }

            if (viewModel.RegisterWriteConfigs != null)
            {
                config.RegisterWriteConfigurations = ConvertViewModelsToRegisterWriteConfigs(viewModel.RegisterWriteConfigs);
            }

            return config;
        }

        private static List<FanConfiguration> ConvertViewModelsToFanConfigs(
            IEnumerable<FanConfigViewModel> viewModels)
        {
            List<FanConfiguration> configs = new List<FanConfiguration>();

            foreach (FanConfigViewModel vm in viewModels)
            {
                var cfg = new FanConfiguration()
                {
                    FanDisplayName = vm.FanDisplayName,
                    ReadRegister = vm.ReadRegister,
                    WriteRegister = vm.WriteRegister,
                    MinSpeedValue = vm.MinSpeedValue,
                    MaxSpeedValue = vm.MaxSpeedValue,
                    IndependentReadMinMaxValues = vm.IndependentReadMinMaxValues,
                    MinSpeedValueRead = vm.MinSpeedValueRead,
                    MaxSpeedValueRead = vm.MaxSpeedValueRead,
                    ResetRequired = vm.ResetRequired,
                    FanSpeedResetValue = vm.ResetValue
                };

                if (vm.FanSpeedOverrides != null)
                {
                    cfg.FanSpeedPercentageOverrides = vm.FanSpeedOverrides.Select(
                        x => new FanSpeedPercentageOverride()
                        {
                            FanSpeedPercentage = x.FanSpeedPercentage,
                            FanSpeedValue = x.FanSpeedValue,
                            TargetOperation = x.TargetOperation
                        }).ToList();
                }

                if (vm.TemperatureThresholds != null)
                {
                    cfg.TemperatureThresholds = vm.TemperatureThresholds.Select(
                        x => new TemperatureThreshold()
                        {
                            DownThreshold = x.DownThreshold,
                            UpThreshold = x.UpThreshold,
                            FanSpeed = x.FanSpeedPercentage
                        }).ToList();
                }

                configs.Add(cfg);
            }

            return configs;
        }

        private static List<RegisterWriteConfiguration> ConvertViewModelsToRegisterWriteConfigs(
            IEnumerable<RegisterWriteConfigViewModel> viewModels)
        {
            return viewModels.Select(
                x => new RegisterWriteConfiguration()
                {
                    Description = x.Description,
                    Register = x.Register,
                    Value = x.Value,
                    ResetRequired = x.ResetRequired,
                    ResetValue = x.ResetValue,
                    WriteMode = x.WriteMode,
                    ResetWriteMode = x.ResetWriteMode,
                    WriteOccasion = x.WriteOccasion
                }).ToList();
        }

        #endregion

        #region ViewModel update

        private void UpdateViewModel()
        {
            var cfg = this.configManager.SelectedConfig;
            var sortedList = this.configManager.ConfigNames.OrderBy(x => x);
            this.ConfigNames = new ObservableCollection<string>(sortedList);

            if (cfg == null)
            {
                ClearViewModel();
            }
            else
            {
                this.SelectedConfigName = this.configManager.SelectedConfigName;
                this.CriticalTemperature = cfg.CriticalTemperature;
                this.EcPollInterval = cfg.EcPollInterval;
                this.NotebookModel = cfg.NotebookModel;
                this.Author = cfg.Author;
                this.ReadWriteWords = cfg.ReadWriteWords;

                if (cfg.RegisterWriteConfigurations != null)
                {
                    var vms = ConvertRegisterWriteConfigsToViewModels(cfg.RegisterWriteConfigurations);
                    this.RegisterWriteConfigs = new ObservableCollection<RegisterWriteConfigViewModel>(vms);
                }

                if (cfg.FanConfigurations != null)
                {
                    var vms = ConvertFanConfigsToViewModels(cfg.FanConfigurations);
                    this.FanConfigs = new ObservableCollection<FanConfigViewModel>(vms);
                }
            }
        }

        private void ClearViewModel()
        {
            this.SelectedConfigName = string.Empty;
            this.CriticalTemperature = 75;
            this.EcPollInterval = 3000;
            this.NotebookModel = string.Empty;
            this.Author = string.Empty;
            this.ReadWriteWords = false;
            this.RegisterWriteConfigs.Clear();
            this.FanConfigs.Clear();
        }

        private static IEnumerable<FanConfigViewModel> ConvertFanConfigsToViewModels(
            IEnumerable<FanConfiguration> configs)
        {
            List<FanConfigViewModel> viewModels = new List<FanConfigViewModel>();

            foreach (FanConfiguration cfg in configs)
            {
                var vm = new FanConfigViewModel()
                {
                    FanDisplayName = cfg.FanDisplayName,
                    ReadRegister = cfg.ReadRegister,
                    WriteRegister = cfg.WriteRegister,
                    MinSpeedValue = cfg.MinSpeedValue,
                    MaxSpeedValue = cfg.MaxSpeedValue,
                    IndependentReadMinMaxValues = cfg.IndependentReadMinMaxValues,
                    MinSpeedValueRead = cfg.MinSpeedValueRead,
                    MaxSpeedValueRead = cfg.MaxSpeedValueRead,
                    ResetRequired = cfg.ResetRequired,
                    ResetValue = cfg.FanSpeedResetValue
                };

                if (cfg.FanSpeedPercentageOverrides != null)
                {
                    vm.FanSpeedOverrides = new ObservableCollection<FanSpeedOverrideViewModel>(
                        cfg.FanSpeedPercentageOverrides.Select(x => new FanSpeedOverrideViewModel()
                        {
                            FanSpeedPercentage = x.FanSpeedPercentage,
                            FanSpeedValue = x.FanSpeedValue,
                            TargetOperation = x.TargetOperation
                        }));
                }

                if (cfg.TemperatureThresholds != null)
                {
                    vm.TemperatureThresholds = new ObservableCollection<TemperatureThresholdViewModel>(
                        cfg.TemperatureThresholds.Select(x => new TemperatureThresholdViewModel()
                        {
                            UpThreshold = x.UpThreshold,
                            DownThreshold = x.DownThreshold,
                            FanSpeedPercentage = x.FanSpeed
                        }));
                }

                viewModels.Add(vm);
            }

            return viewModels;
        }

        private static IEnumerable<RegisterWriteConfigViewModel> ConvertRegisterWriteConfigsToViewModels(
            IEnumerable<RegisterWriteConfiguration> configs)
        {
            return configs.Select(x => new RegisterWriteConfigViewModel()
            {
                Description = x.Description,
                Register = x.Register,
                Value = x.Value,
                WriteMode = x.WriteMode,
                WriteOccasion = x.WriteOccasion,
                ResetRequired = x.ResetRequired,
                ResetValue = x.ResetValue
            });
        }

        #endregion

        #endregion
    }
}
