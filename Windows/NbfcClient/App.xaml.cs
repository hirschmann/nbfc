using NbfcClient.Properties;
using NLog;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace NbfcClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public App()
        {
            logger.Info("App start");

            this.DispatcherUnhandledException += (sender, args) =>
            {
                logger.Error(args.Exception, "An unhandled exception occurred");
            };

            this.Exit += (sender, args) =>
            {
                logger.Info("App exit");
            };

            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement), 
                new FrameworkPropertyMetadata(
                     XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }
    }
}
