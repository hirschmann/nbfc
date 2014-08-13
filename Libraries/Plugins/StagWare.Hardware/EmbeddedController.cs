using System;
using System.Threading;

namespace StagWare.Hardware
{
    public abstract class EmbeddedControllerBase
    {
        #region Enums

        // See ACPI specs ch.12.2
        [Flags]
        enum ECStatus : byte
        {
            OutputBufferFull = 0x01,    // EC_OBF
            InputBufferFull = 0x02,     // EC_IBF
            Command = 0x04,             // CMD
            BurstMode = 0x08,           // BURST
            SCIEventPending = 0x10,     // SCI_EVT
            SMIEventPending = 0x20      // SMI_EVT
        }

        // See ACPI specs ch.12.3
        enum ECCommand : byte
        {
            Read = 0x80,            // RD_EC
            Write = 0x81,           // WR_EC
            BurstEnable = 0x82,     // BE_EC
            BurstDisable = 0x83,    // BD_EC
            Query = 0x84            // QR_EC
        }

        #endregion

        #region Constants

        const int CommandPort = 0x66;    //EC_SC
        const int DataPort = 0x62;       //EC_DATA

        const int RWTimeout = 500;      // spins
        const int IsaBusTimeout = 100;  // ms
        const int WaitReadFailsLimit = 20;

        #endregion

        #region Private Fields

        int waitReadConsecFails = 0;
        bool skipWaitRead = false;

        #endregion

        #region Public Methods

        public bool TryReadByte(byte register, out byte value)
        {
            if (WaitFree())
            {
                WritePort(CommandPort, (byte)ECCommand.Read);

                if (WaitWrite())
                {
                    WritePort(DataPort, register);

                    if (WaitWrite() && (skipWaitRead || WaitRead()))
                    {
                        value = ReadPort(DataPort);
                        return true;
                    }
                }
            }

            value = 0;
            return false;
        }

        public bool TryWriteByte(byte register, byte value)
        {
            if (WaitFree())
            {
                WritePort(CommandPort, (byte)ECCommand.Write);

                if (WaitWrite())
                {
                    WritePort(DataPort, register);

                    if (WaitWrite())
                    {
                        WritePort(DataPort, value);

                        if (WaitWrite())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool TryReadWord(byte register, out int value)
        {
            //Byte order: little endian

            byte result = 0;
            value = 0;

            if (!TryReadByte(register, out result))
            {
                return false;
            }

            value = result;

            if (!TryReadByte((byte)(register + 1), out result))
            {
                return false;
            }

            value |= (ushort)(result << 8);

            return true;
        }

        public bool TryWriteWord(byte register, int value)
        {
            //Byte order: little endian

            ushort val = (ushort)value;

            byte msb = (byte)(val >> 8);
            byte lsb = (byte)val;

            if (!TryWriteByte(register, lsb))
            {
                return false;
            }

            if (!TryWriteByte((byte)(register + 1), msb))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Protected Methods

        protected abstract void WritePort(int port, byte value);
        protected abstract byte ReadPort(int port);

        #endregion

        #region Private Methods

        private bool WaitRead()
        {
            int timeout = RWTimeout;

            while (timeout > 0)
            {
                var status = (ECStatus)ReadPort(CommandPort);

                if (status.HasFlag(ECStatus.OutputBufferFull))
                {
                    waitReadConsecFails = 0;
                    return true;
                }

                timeout--;
                Thread.SpinWait(5);
            }

            if (waitReadConsecFails > WaitReadFailsLimit)
            {
                skipWaitRead = true;
            }

            waitReadConsecFails++;
            return false;
        }

        private bool WaitWrite()
        {
            int timeout = RWTimeout;

            while (timeout > 0)
            {
                var status = (ECStatus)ReadPort(CommandPort);

                if (!status.HasFlag(ECStatus.InputBufferFull))
                {
                    return true;
                }

                timeout--;
                Thread.SpinWait(5);
            }

            return false;
        }

        private bool WaitFree()
        {
            int timeout = RWTimeout;

            while (timeout > 0)
            {
                var status = (ECStatus)ReadPort(CommandPort);

                if (!status.HasFlag(ECStatus.InputBufferFull)
                    && !status.HasFlag(ECStatus.OutputBufferFull))
                {
                    return true;
                }

                timeout--;
                Thread.SpinWait(5);
            }

            return false;
        }

        #endregion
    }
}
