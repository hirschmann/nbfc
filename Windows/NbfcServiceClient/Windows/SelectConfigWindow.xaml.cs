using StagWare.FanControl.Configurations;
using StagWare.Settings;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics;
using StagWare.BiosInfo;

namespace NbfcClient.Windows
{
    /// <summary>
    /// Interaction logic for SelectConfigWindow.xaml
    /// </summary>
    public partial class SelectConfigWindow : Window
    {
        #region Constants

        private const string ConfigsDirectoryName = "Configs";
        private const string ConfigEditorExecutableName = "ConfigEditor.exe";
        private const string ConfigEditorSelectConfigArgumentPrefix = "-s:";
        private const string NotebookModelValueName = "SystemProductName";

        #endregion

        #region Private Fields

        private FanControlClient client;
        private FanControlConfigManager configManager;
        private string parentDirName;

        #endregion

        #region Constructors

        public SelectConfigWindow()
        {
            InitializeComponent();

            parentDirName = Assembly.GetExecutingAssembly().Location;
            parentDirName = Path.GetDirectoryName(parentDirName);

            string path = Path.Combine(parentDirName, ConfigsDirectoryName);

            this.configManager = new FanControlConfigManager(path);
            this.configSelector.ItemsSource = configManager.ConfigNames;            

            if (!File.Exists(Path.Combine(parentDirName, ConfigEditorExecutableName)))
            {
                this.edit.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public SelectConfigWindow(FanControlClient client)
            : this()
        {
            this.client = client;

            bool configFound = false;

            if (!string.IsNullOrWhiteSpace(client.ViewModel.SelectedConfig))
            {
                configFound = configManager.SelectConfig(client.ViewModel.SelectedConfig);
            }
            else if(configManager.SelectConfig(GetNotebookModel()))
            {
                this.configSelector.SelectedValue = configManager.SelectedConfigName;
            }
        }

        #endregion

        #region Private Methods

        private static string GetNotebookModel()
        {
            if (BiosInfo.ValueInfo.Any(x => x.ValueName.Equals(
                NotebookModelValueName, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    return BiosInfo.GetStringValue(NotebookModelValueName);
                }
                catch
                {
                }
            }

            return null;
        }

        #endregion

        #region EventHandlers

        private void edit_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(parentDirName, ConfigEditorExecutableName);

            if (File.Exists(path))
            {
                try
                {
                    string arg = string.Format(
                        "{0}\"{1}\"",
                        ConfigEditorSelectConfigArgumentPrefix,
                        configSelector.SelectedValue.ToString());

                    Process.Start(path, arg);
                }
                catch
                {
                }
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

        #endregion
    }
}
