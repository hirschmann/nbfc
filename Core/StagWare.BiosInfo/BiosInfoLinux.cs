using System.IO;

namespace StagWare.BiosInfo
{
    internal class BiosInfoLinux : BiosInfo
    {
        #region Constants

        string DmiIdDirectoryPath = "/sys/devices/virtual/dmi/id";

        #endregion

        #region BiosInfo implementation

        public override string BIOSReleaseDate => GetValue("bios_date");
        public override string BIOSVendor => GetValue("bios_vendor");
        public override string BIOSVersion => GetValue("bios_version");
        public override string BoardName => GetValue("board_name");
        public override string BoardVendor => GetValue("board_vendor");
        public override string BoardVersion => GetValue("board_version");
        public override string SystemName => GetValue("product_name");
        public override string SystemVendor => GetValue("sys_vendor");
        public override string SystemVersion => GetValue("product_version");

        #endregion

        #region Private Methods

        private string GetValue(string valueName)
        {
            string path = Path.Combine(DmiIdDirectoryPath, valueName);

            try
            {
                return File.ReadAllText(path);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}