using System;

namespace StagWare.FanControl.Plugins
{
    public class PluginInitializationException : Exception
    {
        public PluginInitializationException()
        {
        }

        public PluginInitializationException(string message) : base(message)
        {
        }
    }
}
