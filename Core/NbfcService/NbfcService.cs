using StagWare.FanControl.Service;
using System.ServiceModel;
using System.ServiceProcess;

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

            this.service = new FanControlService();
            this.host = new ServiceHost(service);
            this.host.Open();
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
            try
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
            }
            finally
            {
                if (this.service != null)
                {
                    this.service.Dispose();
                    this.service = null;
                }
            }
        }

        #endregion
    }
}
