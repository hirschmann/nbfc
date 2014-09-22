using System;

namespace StagWare.Hardware.LPC
{
    public abstract class EmbeddedControllerBase
    {
        #region Enums

        // See ACPI specs ch.12.2
        enum ECStatus : byte
        {
            OutputBufferFull = 0x01,    // EC_OBF
            InputBufferFull = 0x02,     // EC_IBF
            // 0x04 is ignored
            Command = 0x08,             // CMD
            BurstMode = 0x10,           // BURST
            SCIEventPending = 0x20,     // SCI_EVT
            SMIEventPending = 0x40      // SMI_EVT
            // 0x80 is ignored
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
        const int FailuresBeforeSkip = 20;
        const int MaxRetries = 5;

        #endregion

        #region Private Fields

        int waitReadFailures = 0;

        #endregion

        #region Public Methods

        public virtual void WriteByte(byte register, byte value)
        {
            int writes = 0;

            while (writes < MaxRetries)
            {
                if (TryWriteByte(register, value))
                {
                    return;
                }

                writes++;
            }
        }

        public virtual void WriteWord(byte register, ushort value)
        {
            int writes = 0;

            while (writes < MaxRetries)
            {
                if (TryWriteWord(register, value))
                {
                    return;
                }

                writes++;
            }
        }

        public virtual byte ReadByte(byte register)
        {
            byte result = 0;
            int reads = 0;

            while (reads < MaxRetries)
            {
                if (TryReadByte(register, out result))
                {
                    return result;
                }

                reads++;
            }

            return result;
        }

        public virtual ushort ReadWord(byte register)
        {
            int result = 0;
            int reads = 0;

            while (reads < MaxRetries)
            {
                if (TryReadWord(register, out result))
                {
                    return (ushort)result;
                }

                reads++;
            }

            return (ushort)result;
        }

        #endregion

        #region Protected Methods

        protected abstract void WritePort(int port, byte value);
        protected abstract byte ReadPort(int port);

        protected bool TryReadByte(byte register, out byte value)
        {
            if (WaitWrite())
            {
                WritePort(CommandPort, (byte)ECCommand.Read);

                if (WaitWrite())
                {
                    WritePort(DataPort, register);

                    if (WaitWrite() && WaitRead())
                    {
                        value = ReadPort(DataPort);
                        return true;
                    }
                }
            }

            value = 0;
            return false;
        }

        protected bool TryWriteByte(byte register, byte value)
        {
            if (WaitWrite())
            {
                WritePort(CommandPort, (byte)ECCommand.Write);

                if (WaitWrite())
                {
                    WritePort(DataPort, register);

                    if (WaitWrite())
                    {
                        WritePort(DataPort, value);
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool TryReadWord(byte register, out int value)
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

        protected bool TryWriteWord(byte register, int value)
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

        #region Private Methods

        private bool WaitRead()
        {
            if (waitReadFailures > FailuresBeforeSkip)
            {
                return true;
            }
            else if (WaitForEcStatus(ECStatus.OutputBufferFull, true))
            {
                waitReadFailures = 0;
                return true;
            }
            else
            {
                waitReadFailures++;
                return false;
            }
        }

        private bool WaitWrite()
        {
            return WaitForEcStatus(ECStatus.InputBufferFull, false);
        }

        private bool WaitForEcStatus(ECStatus status, bool isSet)
        {
            int timeout = RWTimeout;

            while (timeout > 0)
            {
                timeout--;
                byte value = ReadPort(CommandPort);

                if (isSet)
                {
                    value = (byte)~value;
                }

                if (((byte)status & value) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
