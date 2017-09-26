using System;

namespace StagWare.Settings
{
    public class LoadSettingsFailedEventArgs : EventArgs
    {
        public LoadSettingsFailedEventArgs(Exception e)
        {
            this.Exception = e;
        }

        public Exception Exception { get; set; }
    }
}
