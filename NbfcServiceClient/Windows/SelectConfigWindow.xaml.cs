using StagWare.FanControl.Configurations;
using StagWare.Settings;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace NbfcServiceClient.Windows
{
    /// <summary>
    /// Interaction logic for SelectConfigWindow.xaml
    /// </summary>
    public partial class SelectConfigWindow : Window
    {
        private const string ConfigsDirectoryName = "Configs";

        private FanControlClient client;
        private FanControlConfigManager configManager;

        public SelectConfigWindow(FanControlClient client)
        {
            InitializeComponent();

            string path = Assembly.GetExecutingAssembly().Location;
            path = Path.GetDirectoryName(path);
            path = Path.Combine(path, ConfigsDirectoryName);

            this.configManager = new FanControlConfigManager(path);
            this.client = client;
            this.configSelector.ItemsSource = configManager.ConfigNames;

            bool configFound = false;

            if (!string.IsNullOrWhiteSpace(client.ViewModel.SelectedConfig))
            {
                configFound = configManager.SelectConfig(client.ViewModel.SelectedConfig);
            }
            else
            {
                configFound = configManager.AutoSelectConfig();
            }

            if (configFound)
            {
                this.configSelector.SelectedValue = configManager.SelectedConfigName;
            }
        }

        private void apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if (configSelector.SelectedValue != null
                    && configManager.SelectConfig(configSelector.SelectedValue.ToString()))
                {
                    if (configManager.SelectedConfig.FanConfigurations.Any(
                        x => (x.TemperatureThresholds == null || x.TemperatureThresholds.Count <= 0)))
                    {
                        MessageBox.Show(
                            "In the selected config, for one ore more fans are no temperature thresholds defined."
                                + "NBFC auto fan control will load the default thresholds instead."
                                + "\n\nIf you encounter problems, try defining thresholds in Config Editor",
                            "Warning", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Warning);
                    }

                    this.client.SetConfig(configManager.SelectedConfigName);
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
                Close();
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
