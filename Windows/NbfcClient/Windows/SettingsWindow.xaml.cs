using StagWare.Settings;
using StagWare.Windows;
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
            Color current = AppSettings.Values.TrayIconForegroundColor;

            colorPicker.ItemsSource = typeof(Colors).GetProperties().Select(propInfo =>
            {
                var c = (Color)propInfo.GetValue(null, null);

                if (c.GetHashCode() == current.GetHashCode())
                {
                    idx = count;
                }

                count++;

                return new ComboBoxItem()
                {
                    Background = new SolidColorBrush(c),
                    Content = propInfo.Name
                };
            });

            colorPicker.SelectedIndex = idx;

            autorun = new AutorunEntry(RegistryAutorunValueName);
            autorun.Parameters = StartInTrayParameter;
            startWithWin.IsChecked = autorun.Exists;

            closeToTray.IsChecked = AppSettings.Values.CloseToTray;
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
            AppSettings.Values.CloseToTray = true;
            AppSettings.Save();
        }        

        private void closeToTray_Unchecked(object sender, RoutedEventArgs e)
        {
            AppSettings.Values.CloseToTray = false;
            AppSettings.Save();
        }

        private void colorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = colorPicker.SelectedItem as ComboBoxItem;

            if (item != null)
            {
                var brush = item.Background as SolidColorBrush;

                if (brush != null)
                {
                    AppSettings.Values.TrayIconForegroundColor = brush.Color;
                }
            }

            AppSettings.Save();

            var parent = Owner as MainWindow;

            if (parent != null)
            {
                parent.UpdateNotifyIcon();
            }
        }

        #endregion        
    }
}
