using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace NbfcService
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main()
        {
            ServiceBase.Run(new NoteBookFanControlService());
        }
    }
}
