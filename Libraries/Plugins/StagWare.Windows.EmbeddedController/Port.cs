using OpenHardwareMonitor.Hardware;
using StagWare.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StagWare.Windows.EmbeddedController
{
    internal class Port : IPort
    {
        #region Private Fields

        Computer computer;

        #endregion

        #region Constructor

        public Port(Computer computer)
        {
            this.computer = computer;
        }

        #endregion

        #region IPort implementation

        public void Write(int port, byte value)
        {
            this.computer.WriteIoPort((int)port, value);
        }

        public byte Read(int port)
        {
            return this.computer.ReadIoPort((int)port);
        }

        #endregion
    }
}
