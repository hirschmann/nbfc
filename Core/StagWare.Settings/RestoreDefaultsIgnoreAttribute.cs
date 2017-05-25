using System;

namespace StagWare.Settings
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RestoreDefaultsIgnoreAttribute : Attribute
    {
    }
}
