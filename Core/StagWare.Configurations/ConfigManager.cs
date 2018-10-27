using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Serialization;

namespace StagWare.Configurations
{
    public class ConfigManager<T> where T : ICloneable, new()
    {
        #region Constants

        private const string DefaultFileExtension = ".xml";

        // Use Windows specific invalid filename chars on all platforms
        private static readonly char[] InvalidChars = new byte[]
        {
            34, 60, 62, 124, 0, 1, 2, 3, 4, 5,
            6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
            16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
            26, 27, 28, 29, 30, 31, 58, 42, 63, 92, 47
        }.Select(x => (char)x).ToArray();

        #endregion

        #region Private Fields

        private readonly IFileSystem fs;
        private readonly Dictionary<string, Lazy<T>> configs;
        private readonly XmlSerializer serializer;
        private readonly string configDirPath;
        private readonly string configFileExtension;

        #endregion

        #region Properties

        public static ReadOnlyCollection<char> InvalidFileNameChars
        {
            get
            {
                return new ReadOnlyCollection<char>(InvalidChars);
            }
        }

        public ReadOnlyCollection<string> ConfigNames
        {
            get
            {
                return new ReadOnlyCollection<string>(this.configs.Keys.ToList());
            }
        }

        #endregion

        #region Constructors

        public ConfigManager(string configsDirPath)
            : this(configsDirPath, DefaultFileExtension, new FileSystem())
        {
        }

        public ConfigManager(string configsDirPath, string configFileExtension)
            : this(configsDirPath, configFileExtension, new FileSystem())
        {
        }

        public ConfigManager(string configsDirPath, string configFileExtension, IFileSystem fs)
        {
            if (fs == null)
            {
                throw new ArgumentNullException(nameof(fs));
            }

            if (configsDirPath == null)
            {
                throw new ArgumentNullException(nameof(configsDirPath));
            }

            if (configFileExtension == null)
            {
                throw new ArgumentNullException(nameof(configFileExtension));
            }

            this.fs = fs;
            this.configDirPath = configsDirPath;
            this.configFileExtension = configFileExtension;
            this.serializer = new XmlSerializer(typeof(T));

            if (!fs.Directory.Exists(configsDirPath))
            {
                fs.Directory.CreateDirectory(configsDirPath);
            }

            string[] files = fs.Directory.GetFiles(configsDirPath);
            this.configs = new Dictionary<string, Lazy<T>>(files.Length);

            foreach (string path in files)
            {
                string key = fs.Path.GetFileNameWithoutExtension(path);

                if (!this.configs.ContainsKey(key))
                {
                    var lazy = new Lazy<T>(() => LoadConfig(path, this.serializer), false);
                    this.configs.Add(key, lazy);
                }
            }
        }

        #endregion

        #region Public Methods

        public virtual T GetConfig(string configName)
        {
            if (Contains(configName))
            {
                return (T)configs[configName]?.Value?.Clone();
            }
            else
            {
                return default(T);
            }
        }

        public virtual bool ConfigFileExists(string configName)
        {
            return fs.File.Exists(GetConfigFilePath(configName));
        }

        public virtual bool Contains(string configName)
        {
            if (string.IsNullOrWhiteSpace(configName))
            {
                return false;
            }

            return configs.ContainsKey(configName);
        }

        public virtual void AddConfig(T config, string configName)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (string.IsNullOrWhiteSpace(configName))
            {
                throw new ArgumentException("The config name may not be null or consist only of whitespace.", nameof(configName));
            }

            if (configName.IndexOfAny(InvalidChars) != -1)
            {
                throw new ArgumentException("The config name may not contain invalid characters", nameof(configName));
            }

            string path = GetConfigFilePath(configName);

            if (Contains(configName) || fs.File.Exists(path))
            {
                throw new ArgumentException("A config with this name already exists", nameof(configName));
            }

            var clone = (T)config.Clone();

            using (var stream = fs.File.Open(path, System.IO.FileMode.Create))
            {
                this.serializer.Serialize(stream, clone);
            }

            var lazy = new Lazy<T>(() => clone, false);
            this.configs.Add(configName, lazy);
        }

        public virtual void RemoveConfig(string configName)
        {
            string path = string.Empty;

            if (this.configs.ContainsKey(configName))
            {
                path = GetConfigFilePath(configName);
                this.configs.Remove(configName);
            }

            if (!string.IsNullOrWhiteSpace(path) && fs.File.Exists(path))
            {
                fs.File.Delete(path);
            }
        }

        public virtual void UpdateConfig(string configName, T newConfig)
        {
            if (newConfig == null)
            {
                throw new ArgumentNullException(nameof(newConfig));
            }

            if (!Contains(configName))
            {
                throw new KeyNotFoundException();
            }

            var clone = (T)newConfig.Clone();

            using (var stream = fs.File.Open(GetConfigFilePath(configName), System.IO.FileMode.Create))
            {
                serializer.Serialize(stream, clone);
            }

            this.configs[configName] = new Lazy<T>(() => clone, false);
        }

        #endregion

        #region Private Methods

        private T LoadConfig(string path, XmlSerializer serializer)
        {
            try
            {
                using (var stream = fs.File.Open(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    object cfg = serializer.Deserialize(stream);

                    if (cfg != null)
                    {
                        return (T)cfg;
                    }
                }
            }
            catch
            {
            }

            return default(T);
        }

        private string GetConfigFilePath(string fileNameWithoutExt)
        {
            return fs.Path.Combine(this.configDirPath, fileNameWithoutExt + this.configFileExtension);
        }

        #endregion
    }
}
