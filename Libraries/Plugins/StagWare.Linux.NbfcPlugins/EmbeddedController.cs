using StagWare.FanControl.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StagWare.Linux.NbfcPlugins
{
    [Export(typeof(IEmbeddedController))]
    [FanControlPluginMetadata("StagWare.Linux.EmbeddedController", PlatformID.Unix, MinOSVersion = "13.0")]
    public class EmbeddedController : IEmbeddedController
    {
        public bool IsInitialized
        {
            get;
            private set;
        }

        public void Initialize()
        {
            this.IsInitialized = true;
        }

        public void WriteByte(byte register, byte value)
        {
            Debug.WriteLine(string.Format("WriteByte: {0} => {1}", value, register));
        }

        public void WriteWord(byte register, ushort value)
        {
            Debug.WriteLine(string.Format("WriteWord: {0} => {1}", value, register));
        }

        public byte ReadByte(byte register)
        {
            Debug.WriteLine(string.Format("ReadByte: {0}", register));
            return 0;
        }

        public ushort ReadWord(byte register)
        {
            Debug.WriteLine(string.Format("ReadWord: {0}", register));
            return 0;
        }

        public bool AquireLock(int timeout)
        {
            Debug.WriteLine("AquireLock");
            return true;
        }

        public void ReleaseLock()
        {
            Debug.WriteLine("ReleaseLock");
        }

        public void Dispose()
        {
            Debug.WriteLine("Dispose EmbeddedController");
        }
    }
}
