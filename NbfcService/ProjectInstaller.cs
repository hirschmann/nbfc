using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;

namespace NbfcService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            // StagWare.Configurations.FanControlConfigManager uses WMI to get notebook model
            // update: Service does not call the NotebookModel getter

            // this.fanControlServiceInstaller.ServicesDependedOn = new string[]
            // {
            //     "Winmgmt"
            // };
        }
    }
}
