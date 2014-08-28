using Microsoft.Deployment.WindowsInstaller;
using OpenHardwareMonitor.Hardware;
using System;
using System.IO;

namespace DriverSetupWixAction
{
    public class CustomActions
    {
        const string InstallDirPropertyName = "INSTALLFOLDER";

        [CustomAction]
        public static ActionResult InstallDriver(Session session)
        {
            session.Log("Begin installing driver");

            try
            {
                Computer.InstallDriver(session.CustomActionData[InstallDirPropertyName]);
            }
            catch (Exception e)
            {
                session.Log("Failed to install driver: " + e.Message);
                return ActionResult.Failure;
            }

            session.Log("Driver installed successfully");
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult UninstallDriver(Session session)
        {
            session.Log("Begin uninstalling driver");

            try
            {
                Computer.UninstallDriver();
            }
            catch (Exception e)
            {
                session.Log("Failed to uninstall: " + e.Message);
                return ActionResult.Failure;
            }

            try
            {
                string dir = session.CustomActionData[InstallDirPropertyName];

                if (Directory.Exists(dir))
                {
                    foreach (string file in Directory.GetFiles(
                        dir,
                        "*.sys",
                        SearchOption.TopDirectoryOnly))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch
            {
            }

            session.Log("Driver uninstalled successfully");
            return ActionResult.Success;
        }
    }
}
