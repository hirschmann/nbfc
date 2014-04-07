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
            // base.RequestAdditionalTime(2000);

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

        #region Debug

        private static void LogException(string fileName, Exception e)
        {
            if (e == null)
            {
                return;
            }

            string path = Assembly.GetExecutingAssembly().Location;
            path = Path.GetDirectoryName(path);
            path = Path.Combine(path, fileName);

            StringBuilder exception = new StringBuilder();
            exception.AppendLine("-------------------------------------------------------");
            exception.AppendFormat("Timestamp: {0}{1}", DateTime.Now.ToString(), Environment.NewLine);

            if (e.TargetSite != null)
            {
                exception.AppendFormat("Method Name: {0}{1}", e.TargetSite.Name, Environment.NewLine);

                if (e.TargetSite.DeclaringType != null)
                {
                    exception.AppendFormat("Declaring Type: {0}{1}", e.TargetSite.DeclaringType.Name, Environment.NewLine);
                }
            }

            exception.AppendFormat("Message: {0}{1}", e.Message, Environment.NewLine);

            Exception inner = e.InnerException;

            while (inner != null)
            {
                exception.AppendFormat("Inner Message: {0}{1}", inner.Message, Environment.NewLine);
                inner = inner.InnerException;
            }

            exception.AppendFormat("Source: {0}{1}", e.Source, Environment.NewLine);
            exception.AppendFormat("StackTrace: {0}{1}", e.StackTrace, Environment.NewLine);

            File.AppendAllText(path, exception.ToString());
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            if (ex != null)
            {
                LogException("Exceptions.log", ex);
            }
        }

        #endregion
    }
}
