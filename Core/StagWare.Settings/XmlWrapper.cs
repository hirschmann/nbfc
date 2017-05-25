using System.ComponentModel;
using System.Globalization;

namespace StagWare.Settings
{
    public sealed class XmlWrapper<T>
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
                return TypeDescriptor.GetConverter(typeof(T)).ConvertToString(null, CultureInfo.InvariantCulture, wrappedItem);
            }
            set
            {
                wrappedItem = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(null, CultureInfo.InvariantCulture, value);
            }
        }

        #endregion

        #region Operators

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
}
