using GalaSoft.MvvmLight.Ioc;
using NbfcClient.NbfcService;
using System;
using System.Windows.Threading;
using NLog;

namespace NbfcClient.Services
{
    public class FanControlClient : IDisposable, IFanControlClient
    {
        #region Private Fields

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private FanControlServiceClient client;
        private FanControlInfo fanControlInfo;
        private DispatcherTimer timer;

        #endregion

        #region Properties

        public FanControlInfo FanControlInfo
        {
            get { return fanControlInfo; }
        }

        public int UpdateInterval
        {
            get { return this.timer.Interval.Seconds; }
            set
            {
                int updateInterval = value > 0 ? value : 1000;
                timer.Interval = TimeSpan.FromMilliseconds(updateInterval);
            }
        }

        #endregion

        #region Constructors

        [PreferredConstructor]
        public FanControlClient() : this(3000)
        {
        }

        public FanControlClient(int updateInterval = 3000)
        {
            fanControlInfo = new FanControlInfo();
            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            this.UpdateInterval = updateInterval;

            timer.Start();
        }

        #endregion

        #region IFanControlService implementation

        #region Events

        public event EventHandler<FanControlStatusChangedEventArgs> FanControlStatusChanged;        

        protected void OnFanControlStatusChanged(FanControlInfo info)
        {
            if (this.FanControlStatusChanged != null)
            {
                FanControlStatusChanged(this, new FanControlStatusChangedEventArgs(info));
            }
        }

        #endregion

        #region Public Methods

        public void SetTargetFanSpeed(float value, int fanIndex)
        {
            CallServiceMethod(client => client.SetTargetFanSpeed(value, fanIndex));
        }

        public FanControlInfo GetFanControlInfo()
        {
            FanControlInfo info = null;
            CallServiceMethod(client => info = client.GetFanControlInfo());

            if (info == null)
            {
                info = new FanControlInfo();
            }

            return info;
        }

        public void Start(bool readOnly)
        {
            CallServiceMethod(client => client.Start(readOnly));
        }

        public void Stop()
        {
            CallServiceMethod(client => client.Stop());
        }

        public string[] GetConfigNames()
        {
            string[] configNames = null;
            CallServiceMethod(client => configNames = client.GetConfigNames());

            return configNames;
        }

        public void SetConfig(string uniqueConfigId)
        {
            CallServiceMethod(client => client.SetConfig(uniqueConfigId));
        }

        public string[] GetRecommendedConfigs()
        {
            string[] configNames = null;
            CallServiceMethod(client => configNames = client.GetRecommendedConfigs());

            return configNames;
        }

        #endregion

        #endregion

        #region IDisposable implementation

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposeManagedResources)
        {
            if (!disposed)
            {
                if (disposeManagedResources)
                {
                    if (client != null)
                    {
                        try
                        {
                            client.Close();
                        }
                        catch
                        {
                            client.Abort();
                        }

                        this.client = null;
                    }
                }

                disposed = true;
            }
        }

        ~FanControlClient()
        {
            Dispose(false);
        }

        #endregion        

        #region Private Methods

        private void CallServiceMethod(Action<FanControlServiceClient> action)
        {
            try
            {
                if (client == null)
                {
                    client = new FanControlServiceClient();
                    client.Open();
                }

                action(client);
            }
            catch (Exception e)
            {
                logger.Warn(e, "Attempt to call a service method failed");
                client.Abort();
                client = null;
            }
        }

        #endregion

        #region EventHandlers

        void timer_Tick(object sender, EventArgs e)
        {
            FanControlInfo info = GetFanControlInfo();

            fanControlInfo = info;
            OnFanControlStatusChanged(info);
        }        

        #endregion
    }
}
