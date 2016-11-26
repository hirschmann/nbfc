using Microsoft.Win32;

namespace StagWare.BiosInfo
{
    internal class BiosInfoWindows : BiosInfo
    {
        #region Constants

        const string BiosRegKey = @"HARDWARE\DESCRIPTION\System\BIOS";

        #endregion

        #region BiosInfo implementation

        public override string BIOSReleaseDate => GetValue("BIOSReleaseDate");
        public override string BIOSVendor => GetValue("BIOSVendor");
        public override string BIOSVersion => GetValue("BIOSVersion");
        public override string BoardName => GetValue("BaseBoardProduct");
        public override string BoardVendor => GetValue("BaseBoardManufacturer");
        public override string BoardVersion => GetValue("BaseBoardVersion");
        public override string SystemName => GetValue("SystemProductName");
        public override string SystemVendor => GetValue("SystemManufacturer");
        public override string SystemVersion => GetValue("SystemVersion");

        #endregion

        #region Private Methods

        private string GetValue(string valueName)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(BiosRegKey))
                {
                    return key.GetValue(valueName) as string;
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
