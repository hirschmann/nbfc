using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace StagWare.Settings
{
    public sealed class AppSettings
    {
        #region Settings - Properties

        /* Put your properties here
         * 
         * Properties must be public; getters and setters as well.
         * You may assign any of the following attributes to your properties
         * to control how they are handled.
         * 
         * [LoadDefaultsIgnore]
         *   If this attribute is assigned, the affected property
         *   will be ignored on the call of LoadDefaults(), thus it will
         *   retain its value.
         * 
         *                       
         * [PropertyDefaultValue(object value)]
         *   Assigns the value to the property.
         *   Will only work for value types.
         *                                       
         *                                       
         * [PropertyDefaultValue(Type propertyType, params object[] constructorParameters)]
         *   Calls the constructor which matches
         *   the specified parameters.
         *   If no parameters are provided,
         *   a parameterless constructor will be called.
         *                                               
         * 
         * You can also encapsulate types with no public properties (e.g. Color) in XmlWrapper<T>
         * to enable them to be stored in XML settings file.
         * This type provides implicit cast operators to obviate casts.
         * 
         * 
         * Examples:
         * 
         * [PropertyDefaultValue(typeof(XmlWrapper<Color>), "Red")]
         * public XmlWrapper<Color> MyColor { get; set; }
         * 
         * [PropertyDefaultValue(typeof(XmlWrapper<Font>), "Tahoma, 8.25pt")]
         * public XmlWrapper<Font> MyFont { get; set; }
         */

        [PropertyDefaultValue(typeof(XmlWrapper<System.Windows.Media.Color>), "#FF000000")]
        public XmlWrapper<System.Windows.Media.Color> TrayIconForegroundColor { get; set; }

        [PropertyDefaultValue(false)]
        public bool CloseToTray { get; set; }

        [PropertyDefaultValue(350)]
        public double WindowHeight { get; set; }

        [PropertyDefaultValue(430)]
        public double WindowWidth { get; set; }

        #endregion

        //---------------------------//

        #region Code behind - DO NOT MODIFY!

        #region Nested Types

        private class Properties
        {
            #region Singleton instance

            internal static AppSettings Instance;

            #endregion

            #region Private Fields

            private static readonly object SyncRoot = new object();

            #endregion

            #region Constructors

            private Properties()
            { }

            // Explicit static constructor to tell compiler
            // not to mark type as 'beforefieldinit'.
            static Properties()
            {
                Reload();
            }

            #endregion

            #region Internal Methods

            internal static void Reload()
            {
                lock (SyncRoot)
                {
                    Instance = new AppSettings();

                    if (AppSettings.SettingsFileExists)
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(
                                Path.Combine(
                                AppSettings.settingsFileDir,
                                AppSettings.settingsFileName),
                                FileMode.Open))
                            {
                                Properties.Instance = (AppSettings)AppSettings.Values.serializer.Deserialize(fs);
                            }
                        }
                        catch
                        {
                            OnLoadSettingsFailed();
                        }
                    }
                }
            }

            #endregion
        }

        private class PropertyValue
        {
            #region Properties

            public Type ValueType { get; set; }
            public string ValueName { get; set; }
            public object ValueData { get; set; }

            #endregion

            #region Constructor

            public PropertyValue(Type valueType, string valueName, object valueData)
            {
                this.ValueType = valueType;
                this.ValueName = valueName;
                this.ValueData = valueData;
            }

            #endregion
        }

        public class XmlWrapper<T>
        {
            #region Private Fields

            T wrappedItem;

            #endregion

            #region Constructors

            public XmlWrapper()
            {
                this.wrappedItem = default(T);
            }

            public XmlWrapper(T wrappedItem)
            {
                this.wrappedItem = wrappedItem;
            }

            public XmlWrapper(string wrappedItemValue)
            {
                this.Value = wrappedItemValue;
            }

            #endregion

            #region Properties

            public string Value
            {
                get
                {
                    return TypeDescriptor.GetConverter(typeof(T)).ConvertToString(
                        null, 
                        CultureInfo.InvariantCulture, 
                        wrappedItem);
                }

                set
                {
                    wrappedItem = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(
                        null, 
                        CultureInfo.InvariantCulture, 
                        value);
                }
            }

            #endregion

            #region Type Conversion Operators

            public static implicit operator XmlWrapper<T>(T item)
            {
                if (item != null)
                {
                    return new XmlWrapper<T>(item);
                }
                else
                {
                    return null;
                }
            }

            public static implicit operator T(XmlWrapper<T> wrapper)
            {
                if (wrapper != null)
                {
                    return wrapper.wrappedItem;
                }
                else
                {
                    return default(T);
                }
            }

            #endregion
        }

        #region Attributes

        [AttributeUsage(AttributeTargets.Property)]
        private sealed class LoadDefaultsIgnoreAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Property)]
        private sealed class PropertyDefaultValueAttribute : Attribute
        {
            #region Properties

            public object Value { get; set; }

            #endregion

            #region Constructors

            public PropertyDefaultValueAttribute(object value)
            {
                this.Value = value;
            }

            public PropertyDefaultValueAttribute(Type propertyType, params object[] constructorParameters)
            {
                Type[] types = new Type[constructorParameters.Length];

                for (int i = 0; i < constructorParameters.Length; i++)
                {
                    types[i] = constructorParameters[i].GetType();
                }

                ConstructorInfo constructor = propertyType.GetConstructor(types);

                if (constructor != null)
                {
                    this.Value = constructor.Invoke(constructorParameters);
                }
                else
                {
                    if (types.Length <= 0)
                    {
                        string msg = "There ist no default constructor for the type " + propertyType;

                        throw new ArgumentException(msg);
                    }
                    else
                    {
                        string msg = "There ist no default constructor for the type "
                            + propertyType.ToString() + " which accepts the following arguments: ";

                        for (int i = 0; i < types.Length; i++)
                        {
                            msg += types[i].ToString();

                            if (i < types.Length - 1)
                            {
                                msg += ", ";
                            }
                        }

                        throw new ArgumentException(msg);
                    }
                }
            }

            #endregion
        }

        #endregion

        #endregion

        #region Constants

        private const string SettingsFileExtension = ".xml";
        private const string SingletonInstancePropertyName = "Values";

        #endregion

        #region Private Static Fields

        static string settingsFileDir = GetDefaultSettingsFileDirPath();
        static string settingsFileName = GetDefaultSettingsFileName();

        #endregion

        #region Private Fields

        List<PropertyValue> defaults;
        List<PropertyValue> stored;
        XmlSerializer serializer;

        #endregion

        #region Static Events

        public static event EventHandler LoadSettingsFailed;
        public static event EventHandler SaveSettingsFailed;

        #endregion

        #region Static Properties

        // Singleton instance.
        public static AppSettings Values
        {
            get { return Properties.Instance; }
        }

        public static string SettingsDirectoryPath
        {
            get { return AppSettings.settingsFileDir; }
            set { AppSettings.settingsFileDir = value; }
        }

        public static string SettingsFileName
        {
            get { return AppSettings.settingsFileName; }
            set { AppSettings.settingsFileName = value; }
        }

        public static bool SettingsFileExists
        {
            get
            {
                return File.Exists(Path.Combine(
                    AppSettings.settingsFileDir, AppSettings.settingsFileName));
            }
        }

        #endregion

        #region Constructors

        // Hide constructor. (Singleton)
        private AppSettings()
        {
            this.defaults = new List<PropertyValue>();
            this.stored = new List<PropertyValue>();

            // Fill the list of default values.
            foreach (PropertyInfo propInfo in this.GetType().GetProperties())
            {
                // Assign defaults only to non static properties
                MethodInfo methodInfo = propInfo.GetGetMethod(false);

                if (methodInfo == null || methodInfo.IsStatic)
                {
                    continue;
                }

                object[] loadDefaultsIgnore = propInfo.GetCustomAttributes(typeof(LoadDefaultsIgnoreAttribute), false);
                object[] defaultValues;

                try
                {
                    defaultValues = propInfo.GetCustomAttributes(typeof(PropertyDefaultValueAttribute), false);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException(
                        "The default value for the "
                            + propInfo.Name +
                            " property could not be set. Check inner exception for more information.",
                        ex);
                }

                // 'Values' property should not have a default value as it is
                // the singleton instance.
                if (!propInfo.Name.Equals(AppSettings.SingletonInstancePropertyName))
                {
                    if (defaultValues.Length > 0)
                    {
                        var defaultValueAttribute = defaultValues[0] as PropertyDefaultValueAttribute;

                        if (defaultValueAttribute != null)
                        {
                            try
                            {
                                propInfo.SetValue(this, defaultValueAttribute.Value, null);
                            }
                            catch (Exception ex)
                            {
                                throw new ArgumentException(
                                    "The default value for the "
                                        + propInfo.Name +
                                        " property could not be set. Check inner exception for more information.",
                                    ex);
                            }
                        }
                    }

                    // Save the current value as default value.
                    // Properies with 'LoadDefaultsIgnore' attribute will be ignored.
                    if (loadDefaultsIgnore.Length <= 0)
                    {
                        this.defaults.Add(new PropertyValue(
                            propInfo.PropertyType, propInfo.Name, propInfo.GetValue(this, null)));
                    }
                }
            }

            serializer = new XmlSerializer(typeof(AppSettings));
        }

        #endregion

        #region Public Static Methods

        public static void LoadDefaults()
        {
            PropertyInfo[] propInfos = Values.GetType().GetProperties();

            // Assign default values to the corresponding properties.
            foreach (PropertyInfo propInfo in propInfos)
            {
                if (!propInfo.Name.Equals(AppSettings.SingletonInstancePropertyName))
                {
                    // Assign defaults only to non static properties
                    MethodInfo methodInfo = propInfo.GetGetMethod(false);

                    if (methodInfo == null || methodInfo.IsStatic)
                    {
                        continue;
                    }

                    var value = AppSettings.Values.defaults.Find(x => x.ValueName.Equals(propInfo.Name));

                    if (value != null)
                    {
                        propInfo.SetValue(Values, value.ValueData, null);
                    }
                }
            }
        }

        public static void Save()
        {
            // Serialize this.
            try
            {
                if (!Directory.Exists(settingsFileDir))
                {
                    Directory.CreateDirectory(settingsFileDir);
                }

                using (FileStream fs = new FileStream(
                    Path.Combine(AppSettings.settingsFileDir, AppSettings.settingsFileName), 
                    FileMode.Create))
                {
                    AppSettings.Values.serializer.Serialize(fs, AppSettings.Values);
                }
            }
            catch (Exception)
            {
                AppSettings.OnSaveSettingsFailed();
            }
        }

        public static void StoreCurrentSettings()
        {
            Properties.Instance.stored.Clear();

            foreach (PropertyInfo propInfo in Values.GetType().GetProperties())
            {
                // 'Values' property should not be stored as it is
                // the singleton instance.
                if (!propInfo.Name.Equals(AppSettings.SingletonInstancePropertyName))
                {
                    AppSettings.Values.stored.Add(new PropertyValue(
                        propInfo.PropertyType, propInfo.Name, propInfo.GetValue(Values, null)));
                }
            }
        }

        public static void LoadLastStoredSettings()
        {
            PropertyInfo[] propInfos = Values.GetType().GetProperties();

            //Gespeicherte Werte aus der Liste per Reflection den jeweiligen
            //Eigenschaften der Klasse zuweisen
            foreach (PropertyInfo propInfo in propInfos)
            {
                if (!propInfo.Name.Equals(AppSettings.SingletonInstancePropertyName))
                {
                    foreach (PropertyValue value in Values.stored)
                    {
                        if (propInfo.Name.Equals(value.ValueName))
                        {
                            propInfo.SetValue(Values, value.ValueData, null);
                            break;
                        }
                    }
                }
            }
        }

        public static void DeleteSettingsFile()
        {
            if (AppSettings.SettingsFileExists)
            {
                File.Delete(Path.Combine(AppSettings.settingsFileDir, AppSettings.settingsFileName));
            }

            if (Directory.Exists(AppSettings.settingsFileDir))
            {
                Directory.Delete(AppSettings.settingsFileDir, true);
            }
        }

        public static void Reload()
        {
            Properties.Reload();
        }

        #endregion

        #region Private Static Methods

        private static string GetDefaultSettingsFileDirPath()
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                GetProductName());

            foreach (char c in Path.GetInvalidPathChars())
            {
                path = path.Replace(c, '_');
            }

            return path;
        }

        private static string GetDefaultSettingsFileName()
        {
            string name = GetProductName() + AppSettings.SettingsFileExtension;

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name;
        }

        private static string GetProductName()
        {
            var attribute = (AssemblyProductAttribute)AssemblyProductAttribute
                .GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute));

            return attribute.Product;
        }

        private static void OnLoadSettingsFailed()
        {
            if (AppSettings.LoadSettingsFailed != null)
            {
                AppSettings.LoadSettingsFailed(null, EventArgs.Empty);
            }
        }

        private static void OnSaveSettingsFailed()
        {
            if (AppSettings.SaveSettingsFailed != null)
            {
                AppSettings.SaveSettingsFailed(null, EventArgs.Empty);
            }
        }

        #endregion

        #endregion
    }
}