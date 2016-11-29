using System.ServiceModel;

namespace StagWare.FanControl.Service
{
    [ServiceContract]
    public interface IFanControlService
    {
        [OperationContract]
        void SetTargetFanSpeed(float value, int fanIndex);

        [OperationContract]
        FanControlInfo GetFanControlInfo();

        [OperationContract]
        void Start(bool readOnly);

        [OperationContract]
        void Stop();

        [OperationContract]
        void SetConfig(string uniqueConfigId);

        [OperationContract]
        string[] GetConfigNames();

        [OperationContract]
        string[] GetRecommendedConfigs();
    }
}
