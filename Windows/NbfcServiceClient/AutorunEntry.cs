using Microsoft.Win32;
using System;
using System.Linq;
using System.Reflection;

namespace StagWare.Windows
{
    public class AutorunEntry
    {
        #region Private Fields

        private const string RegistryAutorunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        private string valueName;
        private string parameters;
        private string applicationPath;
        private bool appliesToAllUsers;

        #endregion

        #region Properties

        public string ValueName
        {
            get
            {
                return valueName;
            }

            set
            {
                if (!value.Equals(valueName))
                {
                    if (Exists)
                    {
                        UpdateValueName(value);
                    }

                    valueName = value;
                }
            }
        }

        public string Parameters
        {
            get
            {
                return parameters;
            }

            set
            {
                if (!value.Equals(parameters))
                {
                    parameters = value;

                    if (Exists)
                    {
                        UpdateValue();
                    }
                }
            }
        }

        public string ApplicationPath
        {
            get
            {
                return applicationPath;
            }

            set
            {
                if (!value.Equals(applicationPath))
                {
                    applicationPath = value;

                    if (Exists)
                    {
                        UpdateValue();
                    }
                }
            }
        }

        public bool Exists
        {
            get
            {
                bool exists = false;
                RegistryKey key = AppliesToAllUsers ? Registry.LocalMachine : Registry.CurrentUser;

                using (key = key.OpenSubKey(RegistryAutorunPath))
                {
                    if (key.GetValueNames().Contains(ValueName)
                        && key.GetValue(ValueName).Equals(string.Format("\"{0}\" {1}", ApplicationPath, Parameters)))
                    {
                        exists = true;
                    }
                }

                return exists;
            }

            set
            {
                if (Exists != value)
                {
                    if (value)
                    {
                        UpdateValue();
                    }
                    else
                    {
                        DeleteValue();
                    }
                }
            }
        }

        public bool AppliesToAllUsers
        {
            get
            {
                return appliesToAllUsers;
            }

            set
            {
                if (appliesToAllUsers != value)
                {
                    if (Exists)
                    {
                        DeleteValue();
                    }

                    appliesToAllUsers = value;
                    UpdateValue();
                }
            }
        }

        #endregion

        #region Constructor

        public AutorunEntry(string valueName, bool appliesToAllUsers = false)
        {
            ValueName = valueName;
            Parameters = string.Empty;
            ApplicationPath = Assembly.GetExecutingAssembly().Location;
            AppliesToAllUsers = false;
        }

        #endregion

        #region Private Methods

        private void UpdateValue()
        {
            RegistryKey key = AppliesToAllUsers ? Registry.LocalMachine : Registry.CurrentUser;

            using (key = key.OpenSubKey(RegistryAutorunPath, true))
            {
                key.SetValue(ValueName, string.Format("\"{0}\" {1}", ApplicationPath, Parameters));
            }
        }

        private void UpdateValueName(string newName)
        {
            RegistryKey key = AppliesToAllUsers ? Registry.LocalMachine : Registry.CurrentUser;

            using (key = key.OpenSubKey(RegistryAutorunPath, true))
            {
                key.DeleteValue(ValueName);
                key.SetValue(newName, string.Format("\"{0}\" {1}", ApplicationPath, Parameters));
            }
        }

        private void DeleteValue()
        {
            RegistryKey key = AppliesToAllUsers ? Registry.LocalMachine : Registry.CurrentUser;

            using (key = key.OpenSubKey(RegistryAutorunPath, true))
            {
                key.DeleteValue(ValueName);
            }
        }

        #endregion
    }
}
