using System;

namespace StagWare.BiosInfo
{
    public abstract class BiosInfo
    {
        public abstract string BIOSReleaseDate { get; }
        public abstract string BIOSVendor { get; }
        public abstract string BIOSVersion { get; }
        public abstract string BoardVendor { get; }
        public abstract string BoardName { get; }
        public abstract string BoardVersion { get; }
        public abstract string SystemName { get; }
        public abstract string SystemVendor { get; }
        public abstract string SystemVersion { get; }

        public static BiosInfo Create()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return new BiosInfoWindows();

                case PlatformID.Unix:
                    return new BiosInfoLinux();

                default:
                    return null;
            }
        }
    }
}
