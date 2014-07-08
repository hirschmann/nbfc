using System;

namespace StagWare.FanControl.Configurations
{
    public class TemperatureThreshold : ICloneable
    {
        #region Private Fields

        private int upThreshold;
        private int downThreshold;
        private float fanSpeed;

        #endregion

        #region Properties

        public int UpThreshold
        {
            get { return upThreshold; }
            set { upThreshold = value; }
        }

        public int DownThreshold
        {
            get { return downThreshold; }
            set { downThreshold = value; }
        }

        public float FanSpeed
        {
            get 
            { 
                return fanSpeed; 
            }

            set
            {
                if (value > 100)
                {
                    fanSpeed = 100;
                }
                else if (value < 0)
                {
                    fanSpeed = 0;
                }
                else
                {
                    fanSpeed = value;
                }
            }
        }

        #endregion

        #region Constructors

        public TemperatureThreshold()
        { }

        public TemperatureThreshold(int upThreshold, int downThreshold, float fanSpeed)
        {
            this.UpThreshold = upThreshold;
            this.DownThreshold = downThreshold;
            this.FanSpeed = fanSpeed;
        }

        #endregion

        #region ICloneable implementation

        public virtual object Clone()
        {
            return new TemperatureThreshold(this.upThreshold, this.downThreshold, this.fanSpeed);
        }

        #endregion
    }
}
