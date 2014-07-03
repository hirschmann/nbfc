namespace NbfcService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.fanControlServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.fanControlServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // fanControlServiceProcessInstaller
            // 
            this.fanControlServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.fanControlServiceProcessInstaller.Password = null;
            this.fanControlServiceProcessInstaller.Username = null;
            // 
            // fanControlServiceInstaller
            // 
            this.fanControlServiceInstaller.Description = "Allows to control the fans on many different notebook models via user defined con" +
    "figuration files. ";
            this.fanControlServiceInstaller.DisplayName = "NoteBook FanControl Service";
            this.fanControlServiceInstaller.ServiceName = "NbfcService";
            this.fanControlServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.fanControlServiceProcessInstaller,
            this.fanControlServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller fanControlServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller fanControlServiceInstaller;
    }
}