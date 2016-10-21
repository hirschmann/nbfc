using StagWare.FanControl.Service;
using System;
using System.Diagnostics;
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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
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

        protected override void OnContinue()
        {
            this.service.Continue();
        }

        protected override void OnPause()
        {
            this.service.Pause();
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
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

        #region Exception logging

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (this.EventLog != null)
            {
                string message = "";
                var exception = e.ExceptionObject as Exception;

                if (exception == null)
                {
                    message = "An unknown exception occurred.";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(exception.Message))
                    {
                        message = exception.StackTrace;
                    }
                    else
                    {
                        message = exception.Message
                            + Environment.NewLine
                            + Environment.NewLine
                            + exception.StackTrace;
                    }
                }

                this.EventLog.WriteEntry(message, EventLogEntryType.Error);
            }
        }

        #endregion
    }
}
