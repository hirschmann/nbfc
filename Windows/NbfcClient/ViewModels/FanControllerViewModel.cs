using GalaSoft.MvvmLight;
using System;
using System.Windows.Threading;

namespace NbfcClient.ViewModels
{
    public class FanControllerViewModel : ViewModelBase
    {
        #region Constants

        private const int SetFanSpeedDelay = 500; // milliseconds

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

        public float FanSpeedSliderValue
        {
            get { return this.fanSpeedSliderValue; }
            set
            {
                this.Set(ref this.fanSpeedSliderValue, value);
                SetSpeedDelayed();
            }
        }

        public int FanIndex
        {
            get { return this.fanIndex; }
            set { this.Set(ref this.fanIndex, value); }
        }

        public string FanDisplayName
        {
            get { return this.fanDisplayName; }
            set { this.Set(ref this.fanDisplayName, value); }
        }        

        public float TargetFanSpeed
        {
            get { return this.targetFanSpeed; }
            set { this.Set(ref this.targetFanSpeed, value); }
        }

        public float CurrentFanSpeed
        {
            get { return this.currentFanSpeed; }
            set { this.Set(ref this.currentFanSpeed, value); }
        }
        
        public bool IsAutoFanControlEnabled
        {
            get { return this.isAutoFanControlEnabled; }
            set { this.Set(ref this.isAutoFanControlEnabled, value); }
        }

        public bool IsCriticalModeEnabled
        {
            get { return this.isCriticalModeEnabled; }
            set { this.Set(ref this.isCriticalModeEnabled, value); }
        }
        
        public int FanSpeedSteps
        {
            get { return this.fanSpeedSteps; }
            set { this.Set(ref this.fanSpeedSteps, value); }
        }

        #endregion

        #region Private Methods

        private void SetSpeedDelayed()
        {
            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer.Start();
            }
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
