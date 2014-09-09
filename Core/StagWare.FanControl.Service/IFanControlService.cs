using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace StagWare.FanControl.Service
{
    [ServiceContract]
    public interface IFanControlService
    {
        [OperationContract(IsOneWay = true)]
        void SetTargetFanSpeed(float value, int fanIndex);

        [OperationContract]
        FanControlInfo GetFanControlInfo();

        [OperationContract]
        bool Start();

        [OperationContract(IsOneWay = true)]
        void Stop();

        [OperationContract(IsOneWay = true)]
        void SetConfig(string uniqueConfigId);

        //TODO?: Add GetConfigNames()
    }
}
