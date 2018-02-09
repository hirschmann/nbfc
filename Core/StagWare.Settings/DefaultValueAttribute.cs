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
}
