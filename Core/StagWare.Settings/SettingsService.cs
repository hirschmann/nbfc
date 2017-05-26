using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace StagWare.Settings
{
    public static class SettingsService<T> where T : SettingsBase, new()
    {
        #region Nested Types

        private class Properties
        {
            #region Private Fields

            internal static readonly T instance;

            #endregion

            #region Constructors

            private Properties()
            { }

            // Explicit static constructor to tell C# compiler
            // not to mark type as 'beforefieldinit'.
            static Properties()
            {
                instance = new T();
                int settingsVersion = instance.SettingsVersion;

                if (SettingsFileExists)
                {
                    try
                    {
                        using (FileStream fs = new FileStream(SettingsFilePath, FileMode.Open))
                        {
                            var serializer = new XmlSerializer(typeof(T));
                            instance = (T)serializer.Deserialize(fs);
                            int fileVersion = instance.SettingsVersion;

                            if (fileVersion != settingsVersion)
                            {
                                instance.UpgradeSettings(fileVersion, settingsVersion);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        OnLoadSettingsFailed(e);
                        RestoreDefaults();
                    }
                }
                else
                {
                    RestoreDefaults();
                }
            }

            #endregion
        }

        #endregion

        #region Constants

        private const string DefaultSettingsFileNameSuffix = "Settings.xml";

        #endregion

        #region Events

        public static event EventHandler<LoadSettingsFailedEventArgs> LoadSettingsFailed;

        #endregion

        #region Properties

        public static T Settings => Properties.instance;
        public static bool SettingsFileExists => File.Exists(SettingsFilePath);
        public static string SettingsFilePath { get; private set; }

        private static string baseDirectory;

        public static string BaseDirectory
        {
            get { return baseDirectory; }
            set
            {
                if (baseDirectory != value)
                {
                    baseDirectory = value; UpdateSettingsFilePath();
                }
            }
        }

        private static string settingsFolderName;

        public static string SettingsFolderName
        {
            get { return settingsFolderName; }
            set
            {
                if (settingsFolderName != value)
                {
                    settingsFolderName = value; UpdateSettingsFilePath();
                }
            }
        }

        private static string settingsFileName;

        public static string SettingsFileName
        {
            get { return settingsFileName; }
            set
            {
                if (settingsFileName != value)
                {
                    settingsFileName = value; UpdateSettingsFilePath();
                }
            }
        }

        #endregion

        #region Constructors

        static SettingsService()
        {
            BaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            SettingsFolderName = Assembly.GetEntryAssembly()?.GetName()?.Name;

            if (SettingsFolderName == null)
            {
                SettingsFileName = DefaultSettingsFileNameSuffix;
            }
            else
            {
                SettingsFileName = SettingsFolderName + DefaultSettingsFileNameSuffix;
            }
        }

        #endregion

        #region Public Methods

        public static void RestoreDefaults()
        {
            RestoreDefaults(Settings);
        }

        public static void Save()
        {
            string dir = Path.GetDirectoryName(SettingsFilePath);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (FileStream fs = new FileStream(SettingsFilePath, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(fs, Settings);
            }
        }

        public static void DeleteSettingsFile()
        {
            if (SettingsFileExists)
            {
                File.Delete(SettingsFilePath);
            }

            string dir = Path.GetDirectoryName(SettingsFilePath);

            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }

        #endregion

        #region Private Methods

        private static void RestoreDefaults(T settings, bool force = false)
        {
            foreach (PropertyInfo pInfo in GetNonStaticProperties(typeof(T)))
            {
                if (force || !HasRestoreDefaultsIgnoreAttribute(pInfo))
                {
                    var defaultAttrib = pInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false)
                        .FirstOrDefault() as DefaultValueAttribute;

                    // If there is no DefaultValueAttribute, the property
                    // will be set to the default value of the property's type
                    pInfo.SetValue(settings, defaultAttrib?.Value, null);
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetNonStaticProperties(Type type)
        {
            foreach (PropertyInfo propInfo in type.GetProperties())
            {
                MethodInfo methInfo = propInfo.GetGetMethod(false);

                if ((methInfo != null) && (!methInfo.IsStatic))
                {
                    yield return propInfo;
                }
            }
        }

        private static bool HasRestoreDefaultsIgnoreAttribute(PropertyInfo info)
        {
            return info.GetCustomAttributes(typeof(RestoreDefaultsIgnoreAttribute), false).Length > 0;
        }

        private static void OnLoadSettingsFailed(Exception e)
        {
            LoadSettingsFailed?.Invoke(null, new LoadSettingsFailedEventArgs(e));
        }

        private static void UpdateSettingsFilePath()
        {
            string path = Path.Combine(
                BaseDirectory ?? "",
                SettingsFolderName ?? "",
                SettingsFileName ?? "");

            foreach (char c in Path.GetInvalidPathChars())
            {
                path = path.Replace(c, '_');
            }

            SettingsFilePath = path;
        }

        #endregion
    }
}

