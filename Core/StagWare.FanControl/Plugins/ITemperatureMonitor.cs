namespace StagWare.FanControl.Plugins
{
    public interface ITemperatureMonitor : IFanControlPlugin
    {
        string TemperatureSourceDisplayName { get; }
        double GetTemperature();
    }
}
