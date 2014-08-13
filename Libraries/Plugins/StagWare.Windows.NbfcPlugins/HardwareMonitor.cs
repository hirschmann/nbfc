using OpenHardwareMonitor.Hardware;
using System.Linq;

namespace StagWare.Windows
{
    public sealed class HardwareMonitor
    {
        #region Private Fields

        private static object syncRoot = new object();
        private static volatile HardwareMonitor instance;

        private Computer computer;
        private IHardware cpu;

        #endregion

        #region Constructor

        private HardwareMonitor()
        {
            this.computer = new Computer();
            this.computer.CPUEnabled = true;
            this.computer.Open();

            this.cpu = computer.Hardware.FirstOrDefault(x => x.HardwareType == HardwareType.CPU);
        }

        #endregion

        #region Properties

        public static HardwareMonitor Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new HardwareMonitor();
                        }
                    }
                }

                return instance;
            }
        }

        public IHardware CPU
        {
            get { return this.cpu; }
        }

        #endregion

        #region Public Methods

        public bool WaitIsaBusMutex(int timeout)
        {
            return this.computer.WaitIsaBusMutex(timeout);
        }

        public void ReleaseIsaBusMutex()
        {
            this.computer.ReleaseIsaBusMutex();
        }

        public void WriteIoPort(int port, byte value)
        {
            this.computer.WriteIoPort(port, value);
        }

        public byte ReadIoPort(int port)
        {
            return this.computer.ReadIoPort(port);
        }

        #endregion
    }
}
