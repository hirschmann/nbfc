using NbfcClient.NbfcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace NbfcClient.ViewModels
{
    public class FanControllerViewModel : ViewModelBase
    {
        #region Constants

        private const int SetFanSpeedDelay = 500; // milliseconds
        private const int AutoControlFanSpeedPercentage = 101;

        #endregion

        #region Private Fields

        private float currentFanSpeed;
        private float targetFanSpeed;
        private float fanSpeedSliderValue;
        private int fanSpeedSteps;
        private bool isAutoFanControlEnabled;
        private string fanDisplayName;
        private bool isCriticalModeEnabled;

        private FanControlClient client;
        private int fanIndex;
        private DispatcherTimer timer;

        #endregion

        #region Constructors

        public FanControllerViewModel()
        {
            this.FanSpeedSliderValue = AutoControlFanSpeedPercentage;
            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromMilliseconds(SetFanSpeedDelay);

            timer.Tick += timer_Tick;
        }

        public FanControllerViewModel(FanControlClient client, int fanIndex)
            : this()
        {
            this.client = client;
            this.fanIndex = fanIndex;
        }

        #endregion

        #region Properties

        public int FanIndex
        {
            get
            {
                return fanIndex;
            }

            set
            {
                if (fanIndex != value)
                {
                    fanIndex = value;
                    OnPropertyChanged("FanIndex");
                }
            }
        }

        public string FanDisplayName
        {
            get
            {
                return
                    fanDisplayName;
            }

            set
            {
                if (fanDisplayName != value)
                {
                    fanDisplayName = value;
                    OnPropertyChanged("FanDisplayName");
                }
            }
        }

        public float FanSpeedSliderValue
        {
            get
            {
                return fanSpeedSliderValue;
            }

            set
            {
                if (fanSpeedSliderValue != value)
                {
                    fanSpeedSliderValue = value;
                    OnPropertyChanged("FanSpeed");
                    SetSpeedDelayed();
                }
            }
        }

        public float TargetFanSpeed
        {
            get
            {
                return targetFanSpeed;
            }

            set
            {
                if (targetFanSpeed != value)
                {
                    targetFanSpeed = value;
                    OnPropertyChanged("TargetFanSpeed");
                }
            }
        }

        public float CurrentFanSpeed
        {
            get
            {
                return currentFanSpeed;
            }

            set
            {
                if (currentFanSpeed != value)
                {
                    currentFanSpeed = value;
                    OnPropertyChanged("CurrentFanSpeed");
                }
            }
        }

        public bool IsAutoFanControlEnabled
        {
            get
            {
                return isAutoFanControlEnabled;
            }

            set
            {
                if (isAutoFanControlEnabled != value)
                {
                    isAutoFanControlEnabled = value;
                    OnPropertyChanged("IsAutoFanControlEnabled");
                }
            }
        }

        public bool IsCriticalModeEnabled
        {
            get
            {
                return isCriticalModeEnabled;
            }

            set
            {
                if (isCriticalModeEnabled != value)
                {
                    isCriticalModeEnabled = value;
                    OnPropertyChanged("IsCriticalModeEnabled");
                }
            }
        }

        public int FanSpeedSteps
        {
            get
            {
                return fanSpeedSteps;
            }

            set
            {
                if (fanSpeedSteps != value)
                {
                    fanSpeedSteps = value;
                    OnPropertyChanged("FanSpeedSteps");
                }
            }
        }

        #endregion

        #region Private Methods

        private void SetSpeedDelayed()
        {
            this.timer.Stop();
            this.timer.Start();
        }

        private float GetFanSpeedPercentage()
        {
            if (this.fanSpeedSteps == 1)
            {
                return 0;
            }
            else
            {
                // subtract 1 from steps, because last step is reserved for "auto control" setting
                return (this.fanSpeedSliderValue / (this.fanSpeedSteps - 1)) * 100.0f;
            }
        }

        #endregion

        #region Event Handlers

        void timer_Tick(object sender, EventArgs e)
        {
            this.timer.Stop();
            this.client.SetFanSpeed(GetFanSpeedPercentage(), this.fanIndex);
        }

        #endregion
    }
}
