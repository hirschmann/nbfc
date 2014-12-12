using NbfcClient.Properties;
using NbfcClient.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NbfcClient.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        #region Nested Types

        private static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool DestroyIcon(IntPtr hIcon);
        }

        #endregion

        #region Constants

        private const int UpdateInterval = 3; // seconds
        private const int SaveWindowSizeDelay = 1; // seconds
        private const string StartInTrayParameter = "-tray";

        #endregion

        #region Private Fields

        private FanControlClient client;
        private MainWindowViewModel viewModel;
        private DispatcherTimer saveSizeTimer;
        private TrayIconRenderer renderer;
        private bool close;
        private double lastWidth;
        private double lastHeight;

        #endregion

        #region Constructors

        public MainWindow()
        {
            ProcessCommandLineArgs();
            InitializeComponent();

            this.renderer = new TrayIconRenderer();
            this.renderer.Color = Settings.Default.TrayIconForegroundColor;

            this.saveSizeTimer = new DispatcherTimer();
            this.saveSizeTimer.Interval = TimeSpan.FromSeconds(SaveWindowSizeDelay);
            this.saveSizeTimer.Tick += saveSizeTimer_Tick;

            this.viewModel = new MainWindowViewModel();
            viewModel.PropertyChanged += viewModel_PropertyChanged;

            this.client = new FanControlClient(viewModel, UpdateInterval);
            this.DataContext = viewModel;
            client.UpdateViewModel();

            this.Height = Settings.Default.WindowHeight;
            this.Width = Settings.Default.WindowWidth;
            this.SizeChanged += MainWindow_SizeChanged;
        }

        #region Helper Methods

        private void ProcessCommandLineArgs()
        {
            foreach (string s in Environment.GetCommandLineArgs())
            {
                if (s.Equals(StartInTrayParameter, StringComparison.OrdinalIgnoreCase))
                {
                    this.WindowState = System.Windows.WindowState.Minimized;
                    this.Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        #endregion

        #endregion

        #region Public Methods

        public void UpdateNotifyIcon()
        {
            this.renderer.Color = Settings.Default.TrayIconForegroundColor;

            using (var bmp = this.renderer.RenderIcon(viewModel.Temperature.ToString()))
            {
                var tmp = notifyIcon.Icon;
                notifyIcon.Icon = System.Drawing.Icon.FromHandle(bmp.GetHicon());

                if (tmp != null)
                {
                    NativeMethods.DestroyIcon(tmp.Handle);
                    tmp.Dispose();
                }
            }
        }

        #endregion

        #region Private Methods

        private static bool IsPathValid(string path)
        {
            bool isValid = false;

            try
            {
                var tmp = new FileInfo(path);
                isValid = true;
            }
            catch
            {
            }

            return isValid;
        }

        #endregion

        #region EventHandlers

        #region FanControl

        private void selectConfig_Click(object sender, RoutedEventArgs e)
        {
            var window = new SelectConfigWindow(client);
            window.Owner = this;
            window.ShowDialog();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if (this.viewModel.IsServiceAvailable)
                {
                    this.client.StartFanControl();
                }
                else
                {
                    this.client.StopFanControl();
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        #endregion

        #region NotifyIcon

        private void window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            close = true;
            Close();
        }

        private void notifyIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                WindowState = WindowState.Minimized;
                Hide();
            }
            else
            {
                Show();
                Activate();
                WindowState = WindowState.Normal;
            }
        }

        void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CpuTemperature")
            {
                UpdateNotifyIcon();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Settings.Default.CloseToTray && !close)
            {
                e.Cancel = true;
                WindowState = System.Windows.WindowState.Minimized;
            }
        }

        #endregion

        #region Window

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                lastHeight = this.Height;
                lastWidth = this.Width;
            }

            saveSizeTimer.Stop();
            saveSizeTimer.Start();
        }

        void saveSizeTimer_Tick(object sender, EventArgs e)
        {
            this.saveSizeTimer.Stop();

            Settings.Default.WindowHeight = lastHeight;
            Settings.Default.WindowWidth = lastWidth;
            Settings.Default.Save();
        }

        #endregion

        #region Settings & Donation

        private void donationLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new NbfcClient.Windows.SettingsWindow();
            wnd.Owner = this;

            wnd.ShowDialog();
        }

        #endregion

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            if (this.client != null)
            {
                this.client.Dispose();
                this.client = null;
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
