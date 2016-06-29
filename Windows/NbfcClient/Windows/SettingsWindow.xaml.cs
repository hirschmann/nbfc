using NbfcClient.Properties;
using StagWare.Windows;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NbfcClient.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        #region Constants

        private const string RegistryAutorunValueName = "NBFC-ClientApplication";
        private const string StartInTrayParameter = "-tray";
        private static readonly Brush Black = new SolidColorBrush(Colors.Black);
        private static readonly Brush White = new SolidColorBrush(Colors.White);

        #endregion

        #region Private Fields

        private AutorunEntry autorun;

        #endregion

        #region Constructors

        public SettingsWindow()
        {
            InitializeComponent();

            int count = 0;
            int idx = -1;
            Color current = Settings.Default.TrayIconForegroundColor;

            // Fill ComboBox
            colorPicker.ItemsSource = typeof(Colors).GetProperties().Select(propInfo =>
            {
                var c = (Color)propInfo.GetValue(null, null);

                // Get index of currently selected tray icon color
                if (c.GetHashCode() == current.GetHashCode())
                {
                    idx = count;
                }

                count++;

                // Make sure each color's name is readable
                Brush foreground;
                double lightness = (Math.Max(c.R, Math.Max(c.G, c.B)) + Math.Min(c.R, Math.Min(c.G, c.B))) / 2.0;

                if (lightness > 255 * 0.45)
                {
                    foreground = Black;
                }
                else
                {
                    foreground = White;
                }
                var cb = new ComboBoxItem();
                return new ComboBoxItem()
                {
                    Background = new SolidColorBrush(c),
                    Foreground = foreground,
                    Content = propInfo.Name
                };
            });

            colorPicker.SelectedIndex = idx;

            autorun = new AutorunEntry(RegistryAutorunValueName);
            autorun.Parameters = StartInTrayParameter;
            startWithWin.IsChecked = autorun.Exists;

            closeToTray.IsChecked = Settings.Default.CloseToTray;
        }

        #endregion

        #region EventHandlers

        private void startWithWin_Checked(object sender, RoutedEventArgs e)
        {
            autorun.Exists = true;
        }

        private void startWithWin_Unchecked(object sender, RoutedEventArgs e)
        {
            autorun.Exists = false;
        }

        private void closeToTray_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.CloseToTray = true;
            Settings.Default.Save();
        }

        private void closeToTray_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.CloseToTray = false;
            Settings.Default.Save();
        }

        private void colorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = colorPicker.SelectedItem as ComboBoxItem;

            if (item != null)
            {
                var brush = item.Background as SolidColorBrush;

                if (brush != null)
                {
                    Settings.Default.TrayIconForegroundColor = brush.Color;
                }
            }

            Settings.Default.Save();

            var parent = Owner as MainWindow;

            if (parent != null)
            {
                parent.UpdateNotifyIcon();
            }
        }

        #endregion        
    }
}
