using System;
using System.Reflection;

namespace StagWare.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DefaultValueAttribute : Attribute
    {
        #region Properties

        public object Value { get; set; }

        #endregion

        #region Constructors

        public DefaultValueAttribute(object value)
        {
            this.Value = value;
        }

        public DefaultValueAttribute(Type propertyType, params object[] constructorParameters)
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
                    string msg = $"There ist no default constructor for the type {propertyType}";
                    throw new ArgumentException(msg);
                }
                else
                {
                    string format = "There ist no default constructor for the type {0} which accepts the following arguments: {1}";
                    throw new ArgumentException(string.Format(format, propertyType, string.Join<Type>(", ", types)));
                }
            }
        }

        #endregion
    }
}
