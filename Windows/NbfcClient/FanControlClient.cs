using NbfcClient.NbfcService;
using NbfcClient.ViewModels;
using System;
using System.ServiceModel;
using System.Windows.Threading;

namespace NbfcClient
{
    public class FanControlClient : IDisposable
    {
        #region Private Fields

        private MainWindowViewModel viewModel;
        private FanControlServiceClient client;
        private DispatcherTimer timer;

        #endregion

        #region Properties

        public int UpdateInterval
        {
            get
            {
                return this.timer.Interval.Seconds;
            }

            set
            {
                int updateInterval = value > 0 ? value : 1;
                timer.Interval = new TimeSpan(0, 0, updateInterval);
            }
        }

        public MainWindowViewModel ViewModel
        {
            get
            {
                return viewModel;
            }
        }

        #endregion

        #region Constructors

        public FanControlClient(MainWindowViewModel viewModel, int updateInterval)
        {
            this.viewModel = viewModel;

            InitializeClient();

            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            this.UpdateInterval = updateInterval;

            timer.Start();
        }

        #endregion

        #region Public Methods

        public void SetFanSpeed(float speed, int fanIndex)
        {
            try
            {
                this.client.SetTargetFanSpeed(speed, fanIndex);
            }
            catch
            {
                InitializeClient();
            }
        }

        public void StartFanControl()
        {
            try
            {
                client.Start();
                ResetMainViewModel();
                UpdateMainViewModel();
            }
            catch
            {
                InitializeClient();
            }
        }

        public void StopFanControl()
        {
            try
            {
                client.Stop();
                ResetMainViewModel();
                UpdateMainViewModel();
            }
            catch
            {
                InitializeClient();
            }
        }

        public void SetConfig(string uniqueConfigId)
        {
            try
            {
                client.SetConfig(uniqueConfigId);
                ResetMainViewModel();
                UpdateMainViewModel();
            }
            catch
            {
                InitializeClient();
            }
        }

        public void UpdateViewModel()
        {
            UpdateMainViewModel();
        }

        #endregion

        #region Private Methods

        private void InitializeClient()
        {
            try
            {
                if (client == null
                    || client.State == CommunicationState.Closed
                    || client.State == CommunicationState.Closing
                    || client.State == CommunicationState.Faulted)
                {
                    client = new FanControlServiceClient();
                }

                if (client.State != CommunicationState.Opened
                    || client.State != CommunicationState.Opening)
                {
                    client.Open();
                }
            }
            catch
            {
            }
        }

        private void UpdateMainViewModel()
        {
            try
            {
                FanControlInfo info = client.GetFanControlInfo();

                if (info != null)
                {
                    viewModel.CpuTemperature = info.CpuTemperature;
                    viewModel.IsServiceAvailable = info.Enabled;
                    viewModel.SelectedConfig = info.SelectedConfig;

                    if (info.FanStatus != null)
                    {
                        UpdateFanControllerViewModels(info.FanStatus);
                    }
                }
            }
            catch
            {
                ResetMainViewModel();
                InitializeClient();
            }
        }

        private void ResetMainViewModel()
        {
            viewModel.CpuTemperature = 0;
            viewModel.IsServiceAvailable = false;
            viewModel.SelectedConfig = string.Empty;
            viewModel.FanControllers.Clear();
        }

        private void UpdateFanControllerViewModels(FanStatus[] status)
        {
            for (int i = 0; i < status.Length; i++)
            {
                FanStatus fs = status[i];

                if (i >= viewModel.FanControllers.Count)
                {
                    FanControllerViewModel vm = new FanControllerViewModel(this, i)
                    {
                        CurrentFanSpeed = fs.CurrentFanSpeed,
                        TargetFanSpeed = fs.TargetFanSpeed,
                        FanDisplayName = fs.FanDisplayName,
                        FanSpeedSteps = fs.FanSpeedSteps,
                        IsAutoFanControlEnabled = fs.AutoControlEnabled,
                        IsCriticalModeEnabled = fs.CriticalModeEnabled
                    };

                    if (fs.AutoControlEnabled)
                    {
                        vm.FanSpeedSliderValue = fs.FanSpeedSteps;
                    }
                    else
                    {
                        vm.FanSpeedSliderValue = (int)Math.Round((fs.TargetFanSpeed / 100.0) * fs.FanSpeedSteps);
                    }

                    viewModel.FanControllers.Add(vm);
                }
                else
                {
                    FanControllerViewModel vm = viewModel.FanControllers[i];

                    vm.CurrentFanSpeed = fs.CurrentFanSpeed;
                    vm.TargetFanSpeed = fs.TargetFanSpeed;
                    vm.FanDisplayName = fs.FanDisplayName;
                    vm.FanSpeedSteps = fs.FanSpeedSteps;
                    vm.IsAutoFanControlEnabled = fs.AutoControlEnabled;
                    vm.IsCriticalModeEnabled = fs.CriticalModeEnabled;
                }
            }
        }

        #endregion

        #region EventHandlers

        void timer_Tick(object sender, EventArgs e)
        {
            UpdateMainViewModel();
        }

        #endregion

        #region IDisposable implementation

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposeManagedResources)
        {
            if (!disposed)
            {
                if (disposeManagedResources)
                {
                    if (client != null)
                    {
                        try
                        {
                            client.Close();
                        }
                        catch
                        {
                        }

                        this.client = null;
                    }
                }

                disposed = true;
            }
        }

        ~FanControlClient()
        {
            Dispose(false);
        }

        #endregion
    }
}
