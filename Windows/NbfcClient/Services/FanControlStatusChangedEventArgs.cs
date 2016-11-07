using NbfcClient.NbfcService;
using System;

namespace NbfcClient.Services
{
    public class FanControlStatusChangedEventArgs : EventArgs
    {
        #region Constructors

        public FanControlStatusChangedEventArgs()
        {
        }

        public FanControlStatusChangedEventArgs(FanControlInfo info)
        {
            Status = info;
        }

        #endregion

        #region Properties

        public FanControlInfo Status { get; set; }

        #endregion
    }
}
