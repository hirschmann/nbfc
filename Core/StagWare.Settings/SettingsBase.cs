namespace StagWare.Settings
{
    public abstract class SettingsBase
    {
        [RestoreDefaultsIgnore]
        public int SettingsVersion { get; set; }

        public virtual void UpgradeSettings(int fileVersion, int settingsVersion)
        {
            SettingsVersion = settingsVersion;
        }
    }
}
