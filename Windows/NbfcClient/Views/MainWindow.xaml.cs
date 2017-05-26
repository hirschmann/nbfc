using GalaSoft.MvvmLight.Messaging;
using NbfcClient.Messages;
using NbfcClient.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using SettingsService = StagWare.Settings.SettingsService<NbfcClient.AppSettings>;

namespace NbfcClient.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constants

        private const int SaveWindowSizeDelay = 1; // seconds
        private const string StartInTrayParameter = "-tray";

        #endregion

        #region Private Fields

        private DispatcherTimer saveSizeTimer;
        private bool close;
        private double lastWidth;
        private double lastHeight;

        #endregion

        #region Constructors

        public MainWindow()
        {
            ProcessCommandLineArgs();
            InitializeComponent();

            Application.Current.SessionEnding += Current_SessionEnding;

            this.saveSizeTimer = new DispatcherTimer();
            this.saveSizeTimer.Interval = TimeSpan.FromSeconds(SaveWindowSizeDelay);
            this.saveSizeTimer.Tick += saveSizeTimer_Tick;

            this.Height = SettingsService.Settings.WindowHeight;
            this.Width = SettingsService.Settings.WindowWidth;
            this.SizeChanged += MainWindow_SizeChanged;

            var wbcd = new DateTime(DateTime.Now.Year, 5, 28);

            if ((DateTime.Today > wbcd.Subtract(new TimeSpan(14, 0, 0, 0)))
                && (DateTime.Today <= wbcd))
            {
                this.wbcd.Visibility = Visibility.Visible;
            }
            else
            {
                this.wbcd.Visibility = Visibility.Collapsed;
            }

            Messenger.Default.Register<OpenSelectConfigDialogMessage>(this, ShowSelectConfigDialog);
            Messenger.Default.Register<OpenSettingsDialogMessage>(this, ShowSettingsDialog);
        }

        #endregion

        #region Private Methods

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

        private void ShowSelectConfigDialog(OpenSelectConfigDialogMessage msg)
        {
            var dialog = new SelectConfigWindow() { Owner = this };
            dialog.ShowDialog();
        }

        private void ShowSettingsDialog(OpenSettingsDialogMessage msg)
        {
            var dialog = new SettingsWindow() { Owner = this };
            dialog.ShowDialog();
        }

        #endregion      

        #region EventHandlers

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

        void Current_SessionEnding(object sender, SessionEndingCancelEventArgs e)
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (SettingsService.Settings.CloseToTray && !close)
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

            SettingsService.Settings.WindowHeight = lastHeight;
            SettingsService.Settings.WindowWidth = lastWidth;
            SettingsService.Save();
        }

        #endregion

        #region Links

        private void donationLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void wbcd_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        #endregion

        #endregion
    }
}
