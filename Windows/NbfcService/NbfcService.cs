using StagWare.FanControl.Service;
using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace NbfcService
{
    public partial class NoteBookFanControlService : ServiceBase
    {
        #region Private Fields

        private ServiceHost host;
        private FanControlService service;

        #endregion

        #region Constructors

        public NoteBookFanControlService()
        {
            InitializeComponent();
        }

        #endregion

        #region Main

        public void Main()
        {
            ServiceBase.Run(new NoteBookFanControlService());
        }

        #endregion

        #region Overrides

        protected override void OnStart(string[] args)
        {
            StopServiceHost();

            service = new FanControlService();
            host = new ServiceHost(service);
            host.Open();
        }

        protected override void OnStop()
        {
            StopServiceHost();
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            if (powerStatus == PowerBroadcastStatus.ResumeSuspend
                || powerStatus == PowerBroadcastStatus.ResumeAutomatic
                || powerStatus == PowerBroadcastStatus.ResumeCritical)
            {
                this.service.ReInitializeFanControl();
            }

            return true;
        }

        protected override void OnShutdown()
        {
            StopServiceHost();
        }

        #endregion

        #region Private Methods

        private void StopServiceHost()
        {
            if (this.host != null)
            {
                if (this.host.State == CommunicationState.Faulted)
                {
                    this.host.Abort();
                }
                else
                {
                    this.host.Close();
                }

                this.host = null;
            }

            if (this.service != null)
            {
                this.service.Dispose();
                this.service = null;
            }
        }

        #endregion
    }
}
