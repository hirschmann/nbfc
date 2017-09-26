using NLog;
using StagWare.FanControl.Service;
using System;
using System.ServiceModel;
using System.ServiceProcess;

namespace NbfcService
{
    public partial class NoteBookFanControlService : ServiceBase
    {
        #region Private Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private ServiceHost host;
        private FanControlService service;

        #endregion

        #region Constructors

        public NoteBookFanControlService()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                logger.Fatal(args.ExceptionObject as Exception, "An unhandled exception occurred");
            };

            AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
            {
                logger.Debug(args.Exception, "A first chance exception occurred");
            };
            
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
            logger.Info("Starting NoteBookFanControlService");
            StopServiceHost();

            this.service = new FanControlService();
            this.host = new ServiceHost(service);
            this.host.Open();
        }

        protected override void OnStop()
        {
            logger.Info("Stopping NoteBookFanControlService");
            StopServiceHost();
        }

        protected override void OnContinue()
        {
            logger.Info("Continuing NoteBookFanControlService");
            this.service.Continue();
        }

        protected override void OnPause()
        {
            logger.Info("Pausing NoteBookFanControlService");
            this.service.Pause();
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            logger.Info(() => "Handling power event: " + powerStatus.ToString());

            switch (powerStatus)
            {
                case PowerBroadcastStatus.ResumeAutomatic:
                case PowerBroadcastStatus.ResumeCritical:
                case PowerBroadcastStatus.ResumeSuspend:
                case PowerBroadcastStatus.QuerySuspendFailed:
                    this.service.Continue();
                    break;
                case PowerBroadcastStatus.QuerySuspend:
                case PowerBroadcastStatus.Suspend:
                    this.service.Pause();
                    break;
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
