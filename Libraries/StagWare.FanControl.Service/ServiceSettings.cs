using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace StagWare.FanControl.Service.Settings
{
    public sealed class ServiceSettings : IDisposable
    {
        #region Constants

        private const string SettingsFileName = "NbfcServiceSettings.xml";

        #endregion

        #region Private Fields

        private XmlSerializer serializer;
        private FileStream fileStream;

        #endregion

        #region Properties

        public string SelectedConfigId { get; set; }
        public bool AutoStart { get; set; }
        public double[] TargetFanSpeeds { get; set; }

        #endregion

        #region Public Static Methods

        public static ServiceSettings Load(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string path = Path.Combine(directory, SettingsFileName);

            var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            var serializer = new XmlSerializer(typeof(ServiceSettings));
            ServiceSettings settings = null;

            if (File.Exists(path))
            {
                try
                {
                    settings = serializer.Deserialize(fileStream) as ServiceSettings;
                }
                catch
                {
                }
            }

            if (settings == null)
            {
                settings = new ServiceSettings();
            }

            settings.serializer = serializer;
            settings.fileStream = fileStream;

            return settings;
        }

        #endregion

        #region Public Methods

        public void Save()
        {
            // clear file before saving
            this.fileStream.SetLength(0);

            this.serializer.Serialize(this.fileStream, this);
        }

        #endregion

        #region IDisposable implementation

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposeManagedResources)
        {
            if (!disposed)
            {
                if (disposeManagedResources)
                {
                    this.serializer = null;
                    this.fileStream.Dispose();
                    this.fileStream = null;
                }

                //// TODO: Dispose unmanaged resources.

                disposed = true;
            }
        }

        ~ServiceSettings()
        {
            Dispose(false);
        }

        #endregion
    }
}
