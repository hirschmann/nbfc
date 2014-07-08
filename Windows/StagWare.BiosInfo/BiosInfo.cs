using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace StagWare.BiosInfo
{
    public enum InfoValueType
    {
        String,
        Numeric,
        Binary,
        Unknown
    }

    public static class BiosInfo
    {
        #region Constants

        const string BiosRegKey = @"HARDWARE\DESCRIPTION\System\BIOS";

        #endregion

        #region Private Fields

        static ReadOnlyCollection<ValueInfo> valueInfo;
        static Dictionary<string, Tuple<InfoValueType, RegistryValueKind>> dict;

        #endregion

        #region Properties

        public static ReadOnlyCollection<ValueInfo> ValueInfo
        {
            get
            {
                if (valueInfo == null)
                {
                    valueInfo = LoadValueInfo();
                }

                return valueInfo;
            }
        }

        #endregion

        #region Public Methods

        public static long GetNumericValue(string name)
        {
            if (valueInfo == null)
            {
                valueInfo = LoadValueInfo();
            }

            if (!dict.ContainsKey(name))
            {
                throw new ArgumentException("A value with this name does not exist.");
            }
            else if (dict[name].Item1 != InfoValueType.Numeric)
            {
                throw new ArgumentException("The value with this name is non-numeric.");
            }

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(BiosRegKey))
            {
                return (long)key.GetValue(name);
            }
        }

        public static string GetStringValue(string name)
        {
            if (valueInfo == null)
            {
                valueInfo = LoadValueInfo();
            }

            if (!dict.ContainsKey(name))
            {
                throw new ArgumentException("A value with this name does not exist");
            }
            else if (dict[name].Item1 != InfoValueType.String)
            {
                throw new ArgumentException("The value with this name is not a string.");
            }

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(BiosRegKey))
            {
                if (dict[name].Item2 == RegistryValueKind.MultiString)
                {
                    return string.Join("\n", (string[])key.GetValue(name));
                }
                else
                {
                    return (string)key.GetValue(name);
                }
            }
        }

        public static byte[] GetBinaryValue(string name)
        {
            if (valueInfo == null)
            {
                valueInfo = LoadValueInfo();
            }

            if (!dict.ContainsKey(name))
            {
                throw new ArgumentException("A value with this name does not exist");
            }
            else if (dict[name].Item1 != InfoValueType.Binary)
            {
                throw new ArgumentException("The value with this name is non-binary.");
            }

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(BiosRegKey))
            {
                return (byte[])key.GetValue(name);
            }
        }

        public static object GetUnknownValue(string name)
        {
            if (valueInfo == null)
            {
                valueInfo = LoadValueInfo();
            }

            if (!dict.ContainsKey(name))
            {
                throw new ArgumentException("A value with this name does not exist");
            }

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(BiosRegKey))
            {
                return key.GetValue(name);
            }
        }

        #endregion

        #region Private Methods

        private static InfoValueType ValueKindToValueType(RegistryValueKind kind)
        {
            switch (kind)
            {
                case RegistryValueKind.Binary:
                    return InfoValueType.Binary;

                case RegistryValueKind.DWord:
                case RegistryValueKind.QWord:
                    return InfoValueType.Numeric;

                case RegistryValueKind.String:
                case RegistryValueKind.ExpandString:
                case RegistryValueKind.MultiString:
                    return InfoValueType.String;

                default:
                    return InfoValueType.Unknown;
            }
        }

        private static ReadOnlyCollection<ValueInfo> LoadValueInfo()
        {
            var info = new List<ValueInfo>();
            dict = new Dictionary<string, Tuple<InfoValueType, RegistryValueKind>>();

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(BiosRegKey))
            {
                foreach (string name in key.GetValueNames())
                {
                    RegistryValueKind kind = key.GetValueKind(name);
                    InfoValueType type = ValueKindToValueType(kind);
                    info.Add(new ValueInfo(name, type));
                    dict.Add(name, new Tuple<InfoValueType, RegistryValueKind>(type, kind));
                }
            }

            return info.AsReadOnly();
        }

        #endregion
    }
}
