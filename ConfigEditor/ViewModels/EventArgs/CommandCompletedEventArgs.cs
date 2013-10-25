using System;

namespace ConfigEditor.ViewModels
{
    public class CommandExecutedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public Exception Exception { get; set; }
    }
}
