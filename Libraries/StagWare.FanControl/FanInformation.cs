
namespace StagWare.FanControl
{
    public class FanInformation
    {
        public float TargetFanSpeed { get; private set; }
        public float CurrentFanSpeed { get; private set; }
        public bool AutoFanControlEnabled { get; private set; }
        public bool CriticalModeEnabled { get; private set; }
        public string FanDisplayName { get; private set; }

        public FanInformation(
            float targetFanSpeed,
            float currentFanSpeed,
            bool autoControlEnabled,
            bool criticalModeEnabled,
            string fanDisplayName)
        {
            this.TargetFanSpeed = targetFanSpeed;
            this.CurrentFanSpeed = currentFanSpeed;
            this.AutoFanControlEnabled = autoControlEnabled;
            this.CriticalModeEnabled = criticalModeEnabled;
            this.FanDisplayName = fanDisplayName;
        }
    }
}
