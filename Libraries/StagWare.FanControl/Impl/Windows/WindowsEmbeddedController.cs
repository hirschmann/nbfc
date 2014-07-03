using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EC = OpenHardwareMonitor.Hardware.LPC.EmbeddedController;

namespace StagWare.FanControl.Impl.Windows
{
    public class WindowsEmbeddedController : EmbeddedController
    {
        private const int MaxRetries = 10;

        public override void WriteByte(byte register, byte value)
        {
            int tries = 0;
            int successfulTries = 0;

            while ((successfulTries < 3) && (tries < MaxRetries))
            {
                tries++;

                if (EC.TryWriteByte(register, value))
                {
                    successfulTries++;
                }
            }
        }

        public override void WriteWord(byte register, ushort value)
        {
            int tries = 0;
            int successfulTries = 0;

            while ((successfulTries < 3) && (tries < MaxRetries))
            {
                tries++;

                if (EC.TryWriteWord(register, value))
                {
                    successfulTries++;
                }
            }
        }

        public override byte ReadByte(byte register)
        {
            byte result = 0;
            int tries = 0;
            bool success = false;

            while (!success && (tries < MaxRetries))
            {
                tries++;
                success = EC.TryReadByte(register, out result);
            }

            return result;
        }

        public override ushort ReadWord(byte register)
        {
            int result = 0;
            int tries = 0;
            bool success = false;

            while (!success && (tries < MaxRetries))
            {
                tries++;
                success = EC.TryReadWord(register, out result);
            }

            return (ushort)result;
        }

        public override bool AquireLock(int timeout)
        {
            return EC.WaitIsaBusMutex(timeout);
        }

        public override void ReleaseLock()
        {
            EC.ReleaseIsaBusMutex();
        }
    }
}
